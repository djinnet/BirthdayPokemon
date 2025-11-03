using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Data.Extensions;
using BirthdayPokemonCore.Interfaces;
using BirthdayPokemonCore.Models;
using System.Text;

namespace BirthdayPokemonCore.Services
{
    /// <summary>
    /// Service to determine Pokemon Dex number based on birthday.
    /// </summary>
    public class PokemonService(IPokemonRepository pokemonRepository) : IPokemonService
    {
        private const int MaxDexNumber = 1025;

        /// <summary>
        /// Gets the birthday number based on the provided birthday and format.
        /// </summary>
        /// <param name="birthday">The birthday date.</param>
        /// <param name="formatused">The format used for the birthday number calculation.</param>
        /// <returns>An integer representing the birthday number in the specified format.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid format is specified.</exception>
        /// <remarks>Defaults to MM/DD format if none specified.</remarks>
        public int GetBirthdayNumber(DateOnly birthday, EFormatType formatused = EFormatType.DayMonth)
        {
            try
            {
                if (formatused == EFormatType.DayMonth)
                {
                    return int.Parse($"{birthday:ddMM}");
                }
                else if (formatused == EFormatType.MonthDay) // MM/DD
                {
                    return int.Parse($"{birthday:MMdd}");
                }
                else
                {
                    throw new ArgumentException("Invalid format specified. Use 'DD/MM' or 'MM/DD'.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<string> ExportBirthdayPokemonDistributionToCsvAsync(string filePath = "birthday_pokemon_stats.csv")
        {
            var stats = await GenerateBirthdayPokemonDistributionAsync();
            var sb = new StringBuilder();

            sb.AppendLine("DexNumber,Name,Count");

            foreach (var kvp in stats.Dictionary)
            {
                sb.AppendLine($"{kvp.Key},{kvp.Value.Name},{kvp.Value.Count}");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            return Path.GetFullPath(filePath);
        }

        public async Task<string> ExplainBirthdayPokemonAsync(string input, EFormatType formatUsed, EPokemonBirthdayCalculationType birthdayCalculationType = EPokemonBirthdayCalculationType.Standard)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    throw new ArgumentException("Input date cannot be null or empty.");

                var explanation = new StringBuilder();

                // Step 1: Parse and detect format
                DateOnly birthday = input.ParseDateWithFormat(formatUsed);
                explanation.AppendLine($"Parsed date: {birthday:dd/MM/yyyy}");

                // Step 1.5: Use specified format
                explanation.AppendLine($"Using specified format: {formatUsed.ToString()}");

                // Step 3: Normalize to valid Pokédex range
                var (normalized, output) = CalculateNormalizedBirthdayNumberWithExplanation(birthday, formatUsed, birthdayCalculationType);
                explanation.AppendLine(output);

                // Step 4: Lookup Pokémon
                string name = await GetPokemonNameByDexAsync(normalized);
                explanation.AppendLine($"Result -> #{normalized}: {name}");

                return explanation.ToString();
            }
            catch (Exception ex)
            {
                return $"Error explaining birthday Pokémon: {ex.Message}";
            }
        }

        public int CalculateNormalizedBirthdayNumber(DateOnly birthday, EPokemonBirthdayCalculationType calculationType)
        {
            if (calculationType == EPokemonBirthdayCalculationType.Standard)
            {
                int raw = GetBirthdayNumber(birthday);

                return NormalizeDexNumber(raw);
            }
            else if (calculationType == EPokemonBirthdayCalculationType.Seed)
            {
                // Seed calculation logic here
                return CalculateNumberWithBirthday(birthday);
            }
            else if (calculationType == EPokemonBirthdayCalculationType.YearBased)
            {
                // Year-based calculation logic here
                return CalculateNumberWithYear(birthday);
            }
            else
            {
                throw new ArgumentException("Invalid calculation type specified.");
            }
        }

        public (int result, string output) CalculateNormalizedBirthdayNumberWithExplanation(DateOnly birthday, EFormatType formatUsed, EPokemonBirthdayCalculationType calculationType)
        {
            if (calculationType == EPokemonBirthdayCalculationType.Standard)
            {
                int raw = GetBirthdayNumber(birthday, formatUsed);
                int normalized = NormalizeDexNumber(raw);
                StringBuilder output = new();
                output.AppendLine($"Birthday number ({formatUsed}): {raw}");
                if (raw > MaxDexNumber)
                {
                    output.AppendLine($"Wrapped around Pokédex limit ({MaxDexNumber}): (({raw}-1) % {MaxDexNumber}) + 1 = {normalized}");
                }
                else
                {
                    output.AppendLine($"Within Pokédex range: {normalized}");
                }


                return (normalized, output.ToString());
            }
            else if (calculationType == EPokemonBirthdayCalculationType.Seed)
            {
                // Seed calculation logic here
                (int result, string explaination) result = CalculateNumberWithBirthdayWithSteps(birthday);
                return (result.result, result.explaination);
            }
            else if (calculationType == EPokemonBirthdayCalculationType.YearBased)
            {
                // Year-based calculation logic here
                int result = CalculateNumberWithYear(birthday);
                string output = $"Year based calculation (day * month % year) = ({birthday.Day} * {birthday.Month} % {birthday.Year}). Resulted: {result}";
                return (result, output);
            }
            else
            {
                throw new ArgumentException("Invalid calculation type specified.");
            }
        }


        public int NormalizeDexNumber(int number)
        {
            try
            {
                if (number <= 0)
                    throw new ArgumentOutOfRangeException(nameof(number), "Dex number cannot be 0 or negative.");

                if (number <= MaxDexNumber)
                    return number;

                // Proper modular wrap for any range
                return (number - 1) % MaxDexNumber + 1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int CalculateNumberWithBirthday(DateOnly birthday)
        {
            var date = birthday;
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            int baseYear = year % 2 == 0 ? year - 2 : year - 1;
            DateOnly baseDate = new(baseYear, month, day);
            var yearDigits = year.ToString().Select(c => int.Parse(c.ToString())).ToArray();
            var seed = yearDigits[3] * 100 + yearDigits[2];
            var datediff = date.DayOfYear - baseDate.DayOfYear;
            var result = (datediff / 86400000 + seed) % MaxDexNumber + 1;
            return result;
        }

        public static (int result, string explaination) CalculateNumberWithBirthdayWithSteps(DateOnly birthday)
        {
            var date = birthday;
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            int baseYear = year % 2 == 0 ? year - 2 : year - 1;
            DateOnly baseDate = new(baseYear, month, day);
            var yearDigits = year.ToString().Select(c => int.Parse(c.ToString())).ToArray();
            var seed = yearDigits[3] * 100 + yearDigits[2];
            var datediff = date.DayOfYear - baseDate.DayOfYear;
            var result = (datediff / 86400000 + seed) % MaxDexNumber + 1;

            //write explanation log with whole calculation steps
            StringBuilder explanation = new();
            //How we calculated baseyear
            explanation.AppendLine($"Base Year Calculation: {(year % 2 == 0 ? $"{year} is even, so base year is {year - 2}" : $"{year} is odd, so base year is {year - 1}")}");

            //how we calculated seed
            explanation.AppendLine($"Seed Calculation: Last two digits of year {year} are {yearDigits[2]} and {yearDigits[3]}, so seed is ({yearDigits[3]} * 100) + {yearDigits[2]} = {seed}");

            explanation.AppendLine($"Date Difference Calculation: Day of year for {date:dd/MM/yyyy} is {date.DayOfYear}, Day of year for base date {baseDate:dd/MM/yyyy} is {baseDate.DayOfYear}, so difference is {date.DayOfYear} - {baseDate.DayOfYear} = {datediff}");

            explanation.AppendLine($"Final Calculation: (((( {datediff} ) / 86400000) + {seed}) % {MaxDexNumber}) + 1 = {result}");

            return (result, explanation.ToString());
        }

        public static int CalculateNumberWithYear(DateOnly birthday)
        {
            int day = birthday.Day;
            int month = birthday.Month;
            int year = birthday.Year;

            try
            {
                DateValidation(day, month, year);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Date validation failed: {ex.Message}");
            }

            return day * month % year;
        }

        public async Task<string> GetPokemonNameByDexAsync(int dexNumber)
        {
            try
            {
                var pokemon = await pokemonRepository.GetPokemonNameAsync(dexNumber);
                if (string.IsNullOrEmpty(pokemon))
                {
                    return $"Pokémon #{dexNumber}";
                }

                return pokemon;
            }
            catch
            {
                return $"Pokémon #{dexNumber}";
            }
        }

        public async Task<PokemonInfo?> GetPokemonInfoByDexAsync(int dexNumber)
        {
            try
            {
                var pokemon = await pokemonRepository.GetPokemonInfoAsync(dexNumber);
                return pokemon;
            }
            catch
            {
                return new PokemonInfo($"Pokémon #{dexNumber}", string.Empty, dexNumber);
            }
        }

        public async Task<PokemonInfoWithSteps> GetBirthdayPokemonInfoAsync(DateOnly birthday, EFormatType formatType, EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard)
        {
            (int normalizedDex, string output) = CalculateNormalizedBirthdayNumberWithExplanation(birthday, formatType, calculationType);
            var pokemonInfo = await GetPokemonInfoByDexAsync(normalizedDex);
            string explanation = GeneratePokemonInfoExplanation(birthday, formatType, normalizedDex, pokemonInfo, output, calculationType);
            return new PokemonInfoWithSteps(pokemonInfo, explanation);
        }

        private static bool DateValidation(int day, int month, int year)
        {
            try
            {
                //validate every edge cases with day, month and year
                if (day == 0)
                {
                    throw new ArgumentException("Day cannot be zero.");
                }

                if (month == 0)
                {
                    throw new ArgumentException("Month cannot be zero.");
                }

                if (year == 0)
                {
                    throw new ArgumentException("Year cannot be zero.");
                }

                if (day > 31)
                {
                    throw new ArgumentException("Day cannot be greater than 31.");
                }

                if (month > 12)
                {
                    throw new ArgumentException("Month cannot be greater than 12.");
                }

                if (year < 1850 && year > 2100)
                {
                    throw new ArgumentException("Year must be between 1850 and 2100.");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private static string GeneratePokemonInfoExplanation(DateOnly birthday, EFormatType formatType, int normalizedDex, PokemonInfo? pokemonInfo, string outputfromcalculation, EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard)
        {
            var explanation = new StringBuilder();
            explanation.AppendLine($"Parsed date: {birthday:dd/MM/yyyy}");
            explanation.AppendLine($"Using specified format: {formatType.ToString()}");
            explanation.AppendLine($"Using calculation type: {calculationType}");
            explanation.AppendLine(outputfromcalculation);
            explanation.AppendLine($"Result -> #{normalizedDex}: {pokemonInfo?.Name}");
            return explanation.ToString();
        }

        public async Task<PokemonDictionaryDistributation> GenerateBirthdayPokemonDistributionAsync()
        {
            var results = new Dictionary<int, PokemonDistributionEntry>();

            for (int month = 1; month <= 12; month++)
            {
                for (int day = 1; day <= DateTime.DaysInMonth(2024, month); day++)
                {
                    DateOnly date = new(2024, month, day);
                    int normalized = CalculateNormalizedBirthdayNumber(date, EPokemonBirthdayCalculationType.Standard);

                    if (!results.TryGetValue(normalized, out PokemonDistributionEntry? value))
                    {
                        string name = await GetPokemonNameByDexAsync(normalized);
                        value = new PokemonDistributionEntry
                        {
                            DexNumber = normalized,
                            Name = name,
                            Count = 0
                        };
                        results[normalized] = value;
                    }
                    value.Count++;
                }
            }

            return new PokemonDictionaryDistributation
            {
                Dictionary = results
            };
        }
    }
}
