using BirthdayPokemonCore.Data;
using BirthdayPokemonCore.Repo;
using System.Text;

namespace BirthdayPokemonCore
{
    /// <summary>
    /// Service to determine Pokemon Dex number based on birthday.
    /// </summary>
    public class PokemonService : IPokemonService
    {
        private const int MaxDexNumber = 1025;
        private readonly IPokemonRepository _repo;

        public PokemonService(IPokemonRepository pokemonRepository)
        {
            _repo = pokemonRepository;
        }

        /// <summary>
        /// Gets the birthday number based on the provided birthday and format.
        /// </summary>
        /// <param name="birthday">The birthday date.</param>
        /// <param name="formatused">The format used for the birthday number calculation.</param>
        /// <returns>An integer representing the birthday number in the specified format.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid format is specified.</exception>
        /// <remarks>Defaults to MM/DD format if none specified.</remarks>
        public int GetBirthdayNumber(DateOnly birthday, FormatType formatused = FormatType.DayMonth)
        {
            try
            {
                if (formatused == FormatType.DayMonth)
                {
                    return int.Parse($"{birthday:ddMM}");
                }
                else if (formatused == FormatType.MonthDay) // MM/DD
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

            foreach (var kvp in stats)
            {
                sb.AppendLine($"{kvp.Key},{kvp.Value.Name},{kvp.Value.Count}");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            return Path.GetFullPath(filePath);
        }

        public async Task<string> ExplainBirthdayPokemonAsync(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                    throw new ArgumentException("Input date cannot be null or empty.");

                var explanation = new StringBuilder();

                // Step 1: Parse and detect format
                DateOnly birthday = input.ParseFlexibleDate();
                explanation.AppendLine($"Parsed date: {birthday:dd/MM/yyyy}");

                // Step 2: Determine which format was used
                FormatType formatUsed = input.ToFormatType();
                explanation.AppendLine($"Detected format: {formatUsed.ToString()}");

                // Step 3: Compute value
                int raw = GetBirthdayNumber(birthday, formatUsed);
                explanation.AppendLine($"Birthday number ({formatUsed}): {raw}");

                // Step 4: Normalize to valid Pokédex range
                int normalized = NormalizeDexNumber(raw);
                if (raw > MaxDexNumber)
                    explanation.AppendLine($"Wrapped around Pokédex limit ({MaxDexNumber}): (({raw}-1) % {MaxDexNumber}) + 1 = {normalized}");
                else
                    explanation.AppendLine($"Within Pokédex range: {normalized}");

                // Step 5: Lookup Pokémon
                string name = await GetPokemonNameByDexAsync(normalized);
                explanation.AppendLine($"Result -> #{normalized}: {name}");

                return explanation.ToString();
            }
            catch (Exception ex)
            {
                return $"Error explaining birthday Pokémon: {ex.Message}";
            }
        }


        public async Task<string> ExplainBirthdayPokemonAsync(string input, FormatType formatUsed)
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

                // Step 2: Compute value
                int raw = GetBirthdayNumber(birthday, formatUsed);
                explanation.AppendLine($"Birthday number ({formatUsed}): {raw}");

                // Step 3: Normalize to valid Pokédex range
                int normalized = NormalizeDexNumber(raw);
                if (raw > MaxDexNumber)
                    explanation.AppendLine($"Wrapped around Pokédex limit ({MaxDexNumber}): (({raw}-1) % {MaxDexNumber}) + 1 = {normalized}");
                else
                    explanation.AppendLine($"Within Pokédex range: {normalized}");

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


        public int NormalizeDexNumber(int number)
        {
            try
            {
                if (number <= 0)
                    throw new ArgumentOutOfRangeException(nameof(number), "Dex number cannot be 0 or negative.");

                if (number <= MaxDexNumber)
                    return number;

                // Proper modular wrap for any range
                return ((number - 1) % MaxDexNumber) + 1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GetPokemonNameByDexAsync(int dexNumber)
        {
            try
            {
                var pokemon = await _repo.GetPokemonNameAsync(dexNumber);
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
                var pokemon = await _repo.GetPokemonInfoAsync(dexNumber);
                return pokemon;
            }
            catch
            {
                return new PokemonInfo($"Pokémon #{dexNumber}", string.Empty, dexNumber);
            }
        }

        public async Task<(int DexNumber, string Name)> GetBirthdayPokemonAsync(DateOnly birthday, FormatType formatType)
        {
            int raw = GetBirthdayNumber(birthday, formatType);
            int normalized = NormalizeDexNumber(raw);
            string name = await GetPokemonNameByDexAsync(normalized);
            return (normalized, name);
        }

        public async Task<(PokemonInfo? info, string steps)> GetBirthdayPokemonInfoAsync(DateOnly birthday, FormatType formatType)
        {
            int rawNumber = GetBirthdayNumber(birthday, formatType);
            int normalizedDex = NormalizeDexNumber(rawNumber);
            var pokemonInfo = await GetPokemonInfoByDexAsync(normalizedDex);
            string explanation = GeneratePokemonInfoExplanation(birthday, formatType, rawNumber, normalizedDex, pokemonInfo);
            return (pokemonInfo, explanation);
        }

        private static string GeneratePokemonInfoExplanation(DateOnly birthday, FormatType formatType, int rawNumber, int normalizedDex, PokemonInfo? pokemonInfo)
        {
            var explanation = new StringBuilder();
            explanation.AppendLine($"Parsed date: {birthday:dd/MM/yyyy}");
            explanation.AppendLine($"Using specified format: {formatType.ToString()}");
            explanation.AppendLine($"Birthday number ({formatType}): {rawNumber}");
            if (rawNumber > MaxDexNumber)
            {
                explanation.AppendLine($"Wrapped around Pokédex limit ({MaxDexNumber}): (({rawNumber}-1) % {MaxDexNumber}) + 1 = {normalizedDex}");
            }
            else
            {
                explanation.AppendLine($"Within Pokédex range: {normalizedDex}");
            }

            explanation.AppendLine($"Result -> #{normalizedDex}: {pokemonInfo?.Name}");
            return explanation.ToString();
        }

        public async Task<Dictionary<int, (string Name, int Count)>> GenerateBirthdayPokemonDistributionAsync()
        {
            var results = new Dictionary<int, (string Name, int Count)>();

            for (int month = 1; month <= 12; month++)
            {
                for (int day = 1; day <= DateTime.DaysInMonth(2024, month); day++)
                {
                    DateOnly date = new(2024, month, day);
                    int raw = GetBirthdayNumber(date);
                    int normalized = NormalizeDexNumber(raw);

                    if (!results.TryGetValue(normalized, out (string Name, int Count) entry))
                    {
                        string? name = await _repo.GetPokemonNameAsync(normalized);
                        results[normalized] = (name, 1);
                    }
                    else
                    {
                        results[normalized] = (entry.Name, entry.Count + 1);
                    }
                }
            }

            return results
                .OrderByDescending(kvp => kvp.Value.Count)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
