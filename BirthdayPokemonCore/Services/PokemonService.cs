using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Data.Extensions;
using BirthdayPokemonCore.Interfaces;
using BirthdayPokemonCore.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BirthdayPokemonCore.Services
{
    /// <summary>
    /// Service to determine Pokemon Dex number based on birthday.
    /// </summary>
    public class PokemonService(IPokemonRepository pokemonRepository, ILogger<PokemonService> logger) : IPokemonService
    {
        private const int MaxDexNumber = 1025;

        /// <summary>
        /// Gets the birthday number based on the provided birthday and format.
        /// </summary>
        /// <param name="birthday">The birthday date.</param>
        /// <param name="formatused">The format used for the birthday number calculation.</param>
        /// <returns>An integer representing the birthday number in the specified format.</returns>
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
                    logger.LogError("Invalid format specified: {formatused}. Use 'DD/MM' or 'MM/DD'.", formatused);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating birthday number for date {birthday} with format {formatused}.", birthday, formatused);
                return -1;
            }
        }


        public async Task<string> ExportBirthdayPokemonDistributionToCsvAsync(string filePath = "birthday_pokemon_stats.csv", EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard)
        {
            PokemonDictionaryDistributation stats = await GenerateBirthdayPokemonDistributionAsync(calculationType);
            StringBuilder sb = new ();

            sb.AppendLine("DexNumber,Name,Count");

            foreach (var kvp in stats.Dictionary)
            {
                sb.AppendLine($"{kvp.Key},{kvp.Value.Name},{kvp.Value.Count}");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            return Path.GetFullPath(filePath);
        }

        /// <summary>
        /// <para>Only for console applications</para>
        /// Explains the birthday Pokémon based on input date string and format.
        /// </summary>
        /// <param name="input">
        /// The input date string representing the birthday.
        /// </param>
        /// <param name="formatUsed">
        /// The format used for the birthday number calculation.
        /// </param>
        /// <param name="birthdayCalculationType">
        /// The type of birthday calculation to use (Standard, Seed, YearBased).
        /// </param>
        /// <returns>
        /// A string explanation of the birthday Pokémon calculation and result.
        /// </returns>
        public async Task<string> ExplainBirthdayPokemonAsync(string input, EFormatType formatUsed, EPokemonBirthdayCalculationType birthdayCalculationType = EPokemonBirthdayCalculationType.Standard)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    logger.LogError("Input date string is null or empty.");
                    return string.Empty;
                }


                // Step 1: Parse and detect format
                DateOnly birthday = input.ParseDateWithFormat(formatUsed);


                // Step 3: Normalize to valid Pokédex range
                (int normalized, string output) = CalculateNormalizedBirthdayNumberWithExplanation(birthday, formatUsed, birthdayCalculationType);


                // Step 4: Lookup Pokémon
                PokemonInfo? pokemon = await GetPokemonInfoByDexAsync(normalized);

                if (pokemon == null)
                {
                    logger.LogError("No Pokémon found for normalized Dex number: {normalized}.", normalized);
                    return string.Empty;
                }

                if(string.IsNullOrEmpty(pokemon.Name))
                {
                    logger.LogError("Pokémon name is empty for normalized Dex number: {normalized}.", normalized);
                    return string.Empty;
                }

                var colorStringBuilder = birthday.ToExplainationString(formatUsed, normalized, pokemon.Name);

                string plaintextExplanation = colorStringBuilder.ToString();

                if(string.IsNullOrEmpty(plaintextExplanation))
                {
                    logger.LogError("Generated explanation is empty for input: {input} with format: {formatUsed} and calculation type: {birthdayCalculationType}.", input, formatUsed, birthdayCalculationType);
                    return string.Empty;
                }

                //only for console applications
                bool is_console = Console.OpenStandardInput(1) != Stream.Null;
                if (is_console)
                {
                    colorStringBuilder.WriteToConsole();
                }
                return plaintextExplanation;
            }
            catch (Exception ex)
            {
                //log the exception
                logger.LogError(ex, "Error explaining birthday Pokémon for input: {input} with format: {formatUsed} and calculation type: {birthdayCalculationType}.", input, formatUsed, birthdayCalculationType);
                return string.Empty;
            }
        }

        public int CalculateNormalizedBirthdayNumber(DateOnly birthday, EPokemonBirthdayCalculationType calculationType, EFormatType formatType = EFormatType.DayMonth)
        {
            if (calculationType == EPokemonBirthdayCalculationType.Standard)
            {
                int raw = GetBirthdayNumber(birthday, formatType);

                return NormalizeDexNumber(raw);
            }
            else if (calculationType == EPokemonBirthdayCalculationType.Seed)
            {
                // Seed calculation logic here
                (int result, string explaination) result = CalculateNumberWithBirthdayWithSteps(birthday);
                return result.result;
            }
            else if (calculationType == EPokemonBirthdayCalculationType.YearBased)
            {
                // Year-based calculation logic here
                return CalculateNumberWithYear(birthday, formatType);
            }
            else
            {
                logger.LogError("Invalid calculation type specified: {calculationType}.", calculationType);
                return -1;
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
                int result = CalculateNumberWithYear(birthday, formatUsed);
                if(formatUsed == EFormatType.DayMonth)
                {
                    string output = $"Year based calculation (day * month % year) = ({birthday.Day} * {birthday.Month} % {birthday.Year}). Resulted: {result}";
                    return (result, output);
                }
                else
                {
                    string output = $"Year based calculation (month * day % year) = ({birthday.Month} * {birthday.Day} % {birthday.Year}). Resulted: {result}";
                    return (result, output);
                }
            }
            else
            {
                logger.LogError("Invalid calculation type specified: {calculationType}.", calculationType);
                return (-1, string.Empty);
            }
        }


        public int NormalizeDexNumber(int number)
        {
            try
            {
                if (number <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(number), "Dex number cannot be 0 or negative.");
                }

                if (number <= MaxDexNumber)
                {
                    return number;
                }

                // Proper modular wrap for any range
                return (number - 1) % MaxDexNumber + 1;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error normalizing Dex number: {number}.", number);
                return -1;
            }
        }

        public static (int result, string explaination) CalculateNumberWithBirthdayWithSteps(DateOnly birthday)
        {
            DateOnly date = birthday;
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            int baseYear = year % 2 == 0 ? year - 2 : year - 1;
            DateOnly baseDate = new(baseYear, month, day);
            int[] yearDigits = year.ToString().Select(c => int.Parse(c.ToString())).ToArray();
            int seed = yearDigits[3] * 100 + yearDigits[2];
            int datediff = date.DayOfYear - baseDate.DayOfYear;
            int result = (datediff / 86400000 + seed) % MaxDexNumber + 1;

            //write explanation log with whole calculation steps
            string explanation = birthday.ToExplainationString(MaxDexNumber, result);
            return (result, explanation);
        }

        public int CalculateNumberWithYear(DateOnly birthday, EFormatType formatType = EFormatType.DayMonth)
        {
            try
            {
                birthday.DateValidation();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Invalid date provided for year-based calculation: {birthday}.", birthday);
                return -1;
            }

            return formatType switch
            {
                EFormatType.DayMonth => birthday.Day * birthday.Month % birthday.Year,
                _ => birthday.Month * birthday.Day % birthday.Year
            };
        }

        public async Task<PokemonInfo?> GetPokemonInfoByDexAsync(int dexNumber)
        {
            try
            {
                PokemonInfo pokemon = await pokemonRepository.GetPokemonInfoAsync(dexNumber);
                if(string.IsNullOrEmpty(pokemon.Name))
                {
                    logger.LogWarning("Pokémon name is empty for Dex number: {dexNumber}.", dexNumber);
                    return null;
                }

                if(string.IsNullOrEmpty(pokemon.ImageUrl))
                {
                    logger.LogWarning("Pokémon image URL is empty for Dex number: {dexNumber}.", dexNumber);
                }

                if(pokemon.DexNumber <= 0)
                {
                    logger.LogWarning("Pokémon Dex number is invalid ({dexNumber}) for Dex number: {dexNumber}.", pokemon.DexNumber, dexNumber);
                    return null;
                }
                return pokemon;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error fetching Pokémon info for Dex number: {dexNumber}.", dexNumber);
                return null;
            }
        }

        public async Task<PokemonInfoWithSteps?> GetBirthdayPokemonInfoAsync(DateOnly birthday, EFormatType formatType, EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard)
        {
            try
            {
                (int normalizedDex, string output) = CalculateNormalizedBirthdayNumberWithExplanation(birthday, formatType, calculationType);
                if (normalizedDex <= 0)
                {
                    logger.LogError("Calculated normalized Dex number is invalid: {normalizedDex} for birthday: {birthday}, format: {formatType}, calculation type: {calculationType}.", normalizedDex, birthday, formatType, calculationType);
                    return null;
                }

                if (string.IsNullOrEmpty(output))
                {
                    logger.LogError("Calculation output is empty for birthday: {birthday}, format: {formatType}, calculation type: {calculationType}.", birthday, formatType, calculationType);
                    return null;
                }

                PokemonInfo? pokemonInfo = await GetPokemonInfoByDexAsync(normalizedDex);
                if(pokemonInfo == null) {
                    logger.LogError("No Pokémon info found for normalized Dex number: {normalizedDex}.", normalizedDex);
                    return null;
                }
                string explanation = pokemonInfo.ToExplaniationString(birthday, formatType, calculationType, output, normalizedDex);
                return new PokemonInfoWithSteps(pokemonInfo, explanation);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting birthday Pokémon info for birthday: {birthday}, format: {formatType}, calculation type: {calculationType}.", birthday, formatType, calculationType);
                return null;
            }
        }

        public async Task<PokemonDictionaryDistributation> GenerateBirthdayPokemonDistributionAsync(EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard)
        {
            Dictionary<int, PokemonDistributionEntry> results = [];
            results.Clear();

            // Iterate through all days of the year (leap year)
            var currentYear = DateTime.Now.Year;
            logger.LogInformation("Generating birthday Pokémon distribution for year: {currentYear}", currentYear);

            for (int month = 1; month <= 12; month++)
            {
                // Get number of days in the month
                for (int day = 1; day <= DateTime.DaysInMonth(currentYear, month); day++)
                {
                    // Create DateOnly for the current day
                    DateOnly date = new(currentYear, month, day);

                    logger.LogInformation("Processing date: {date:dd/MM/yyyy}", date);

                    // Calculate normalized Dex number
                    int normalizedForDayMonth = CalculateNormalizedBirthdayNumber(date, calculationType);
                    int normalizedForMonthDay = CalculateNormalizedBirthdayNumber(date, calculationType, EFormatType.MonthDay);

                    // Process Day/Month format
                    if (!results.TryGetValue(normalizedForDayMonth, out PokemonDistributionEntry? value))
                    {
                        // Fetch Pokémon info
                        PokemonInfo? pokemon = await GetPokemonInfoByDexAsync(normalizedForDayMonth);
                        if (pokemon == null)
                        {
                            logger.LogWarning("No Pokémon found for normalized Dex number: {normalized} during distribution generation.", normalizedForDayMonth);
                            continue;
                        }

                        if(string.IsNullOrEmpty(pokemon.Name))
                        {
                            logger.LogWarning("Pokémon name is empty for normalized Dex number: {normalized} during distribution generation.", normalizedForDayMonth);
                            continue;
                        }

                        // Initialize new entry
                        value = new PokemonDistributionEntry
                        {
                            DexNumber = normalizedForDayMonth,
                            Name = pokemon.Name,
                            Count = 0
                        };
                        // Add to results
                        results[normalizedForDayMonth] = value;
                        logger.LogInformation("Added new Pokémon to distribution: {Name} (Dex #{DexNumber})", value.Name, value.DexNumber);
                    }

                    // Increment count for this Pokémon
                    logger.LogInformation("Incrementing count for Pokémon: {Name} (Dex #{DexNumber})", value.Name, value.DexNumber);
                    value.Count++;

                    // Process Month/Day format
                    if (!results.TryGetValue(normalizedForMonthDay, out PokemonDistributionEntry? valueMD))
                    {
                        // Fetch Pokémon info
                        PokemonInfo? pokemonMD = await GetPokemonInfoByDexAsync(normalizedForMonthDay);
                        if (pokemonMD == null)
                        {
                            logger.LogWarning("No Pokémon found for normalized Dex number: {normalized} during distribution generation.", normalizedForMonthDay);
                            continue;
                        }
                        if (string.IsNullOrEmpty(pokemonMD.Name))
                        {
                            logger.LogWarning("Pokémon name is empty for normalized Dex number: {normalized} during distribution generation.", normalizedForMonthDay);
                            continue;
                        }
                        // Initialize new entry
                        valueMD = new PokemonDistributionEntry
                        {
                            DexNumber = normalizedForMonthDay,
                            Name = pokemonMD.Name,
                            Count = 0
                        };
                        // Add to results
                        results[normalizedForMonthDay] = valueMD;
                        logger.LogInformation("Added new Pokémon to distribution: {Name} (Dex #{DexNumber})", valueMD.Name, valueMD.DexNumber);
                    }

                    // Increment count for this Pokémon
                    logger.LogInformation("Incrementing count for Pokémon: {Name} (Dex #{DexNumber})", valueMD.Name, valueMD.DexNumber);
                    valueMD.Count++;

                }
            }

            logger.LogInformation("Completed generating birthday Pokémon distribution.");
            return new PokemonDictionaryDistributation
            {
                Dictionary = results
            };
        }
    }
}
