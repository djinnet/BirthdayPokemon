using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Data.Extensions;
using BirthdayPokemonCore.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayPokemonConsole
{
    public class Core : ICore
    {
        private IPokemonService service { get; set; }
        private ILogger _logger { get; set; }
        public Core(IPokemonService pokemonService, ILogger<Core> logger)
        {
            _logger = logger;
            service = pokemonService;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Birthday Pokémon Core Module started.");
            while (true)
            {
                Console.WriteLine("=== Birthday Pokémon Calculator ===");
                Console.WriteLine("Type 'stats' to see distribution stats, 'statscsv' to export CSV, or 'birthday' to enter your birthday and find out about your pokemon.");

                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                await ExecuteUserCommandAsync(input);

                Console.Write("\nTry another command? (Y/N): ");
                string? again = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(again))
                {
                    break;
                }

                if (!again.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            Console.WriteLine("Goodbye!");
        }

        private async Task ExecuteUserCommandAsync(string input)
        {
            switch (input.ToLower())
            {
                case "stats":
                    await CalculatePokemonDistributionAsync();
                    break;
                case "statscsv":
                    await ExportStatsToCsvAsync();
                    break;
                case "birthday":
                    Console.Write("Enter your birthday (MM/DD or DD/MM): ");
                    await FindYourPokemon();
                    break;
                default:
                    ErrorConsole("Invalid command. Please enter 'stats', 'statscsv', or 'birthday'.");
                    break;
            }
        }

        private async Task FindYourPokemon()
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!input.isInputStringFormattedCorrectly())
            {
                ErrorConsole("Invalid date format. Please enter in MM/DD or DD/MM format.");
                return;
            }

            Console.Write("Is this birthday in MM/DD format or DD/MM format? (Enter 'M' for MM/DD, 'D' for DD/MM, or press Enter for default DD/MM): ");
            string? formatInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(formatInput))
            {
                formatInput = "D"; // default to DD/MM
                Console.WriteLine("Defaulting to DD/MM format.");
            }

            EFormatType formatType = formatInput?.Equals("M", StringComparison.OrdinalIgnoreCase) == true
                ? EFormatType.MonthDay
                : EFormatType.DayMonth;

            //Which calculation type to use
            Console.WriteLine("Select calculation type: (1) Standard (2) Seed (3) Year");
            string? calcInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(calcInput))
            {
                calcInput = "1"; // default to Standard
                Console.WriteLine("Defaulting to Standard calculation.");
            }

            EPokemonBirthdayCalculationType calculationType = calcInput switch
            {
                "1" => EPokemonBirthdayCalculationType.Standard,
                "2" => EPokemonBirthdayCalculationType.Seed,
                "3" => EPokemonBirthdayCalculationType.YearBased,
                _ => EPokemonBirthdayCalculationType.Standard,
            };

            try
            {
                await service.ExplainBirthdayPokemonAsync(input, formatType, calculationType);
            }
            catch (Exception ex)
            {
                LogErrorConsole(ex);
                _logger.LogError(ex, "Error calculating birthday Pokémon.");
            }
        }

        private async Task ExportStatsToCsvAsync()
        {
            try
            {
                //Which calculation type to use
                Console.WriteLine("Select calculation type: (1) Standard (2) Seed (3) Year");
                string? calcInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(calcInput))
                {
                    calcInput = "1"; // default to Standard
                    Console.WriteLine("Defaulting to Standard calculation.");
                }

                EPokemonBirthdayCalculationType calculationType = calcInput switch
                {
                    "1" => EPokemonBirthdayCalculationType.Standard,
                    "2" => EPokemonBirthdayCalculationType.Seed,
                    "3" => EPokemonBirthdayCalculationType.YearBased,
                    _ => EPokemonBirthdayCalculationType.Standard,
                };

                Console.WriteLine("Generating CSV... this may take a minute.");
                string path = await service.ExportBirthdayPokemonDistributionToCsvAsync(calculationType: calculationType);
                Console.WriteLine($"\nCSV export completed! File saved to:\n{path}\n");
            }
            catch (Exception ex)
            {
                LogErrorConsole(ex);
                _logger.LogError(ex, "Error exporting stats to CSV.");
            }
        }

        private async Task CalculatePokemonDistributionAsync()
        {
            try
            {
                //Which calculation type to use
                Console.WriteLine("Select calculation type: (1) Standard (2) Seed (3) Year");
                string? calcInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(calcInput))
                {
                    calcInput = "1"; // default to Standard
                    Console.WriteLine("Defaulting to Standard calculation.");
                }

                EPokemonBirthdayCalculationType calculationType = calcInput switch
                {
                    "1" => EPokemonBirthdayCalculationType.Standard,
                    "2" => EPokemonBirthdayCalculationType.Seed,
                    "3" => EPokemonBirthdayCalculationType.YearBased,
                    _ => EPokemonBirthdayCalculationType.Standard,
                };

                Console.WriteLine("Calculating distribution... please wait.");
                var stats = await service.GenerateBirthdayPokemonDistributionAsync(calculationType);

                Console.WriteLine("\nTop 10 Most Common Birthday Pokémon:");
                var top10 = stats.GetOrderedEntries().Take(10);
                foreach (var entry in top10)
                {
                    Console.WriteLine($"#{entry.DexNumber} {entry.Name}: {entry.Count} birthdays");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                //red console text
                LogErrorConsole(ex);
                _logger.LogError(ex, "Error generating stats.");
            }
        }

        private static void LogErrorConsole(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        private static void ErrorConsole(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
