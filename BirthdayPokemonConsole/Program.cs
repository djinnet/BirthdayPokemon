using BirthdayPokemonCore;
using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Repo;
using BirthdayPokemonCore.Services;

var repo = new PokeAPIRepo();
var service = new PokemonService(repo);

while (true)
{
    Console.WriteLine("=== Birthday Pokémon Calculator ===");
    Console.WriteLine("Type 'stats' to see distribution stats, 'statscsv' to export CSV, or enter your birthday to find your Pokémon.");

    Console.Write("Enter your birthday (MM/DD or DD/MM): ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        break;

    Console.Write("Is this birthday in MM/DD format or DD/MM format? (Enter 'M' for MM/DD, 'D' for DD/MM, or press Enter for default DD/MM): ");
    string? formatInput = Console.ReadLine();

    var formatType = formatInput?.Equals("M", StringComparison.OrdinalIgnoreCase) == true
        ? EFormatType.MonthDay
        : EFormatType.DayMonth;

    //Which calculation type to use
    Console.WriteLine("Select calculation type: (1) Standard (2) Seed (3) Year");
    string? calcInput = Console.ReadLine();
    var calculationType = calcInput switch
    {
        "1" => EPokemonBirthdayCalculationType.Standard,
        "2" => EPokemonBirthdayCalculationType.Seed,
        "3" => EPokemonBirthdayCalculationType.YearBased,
        _ => EPokemonBirthdayCalculationType.Standard,
    };

    try
    {
        if (input.Equals("stats", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Calculating distribution... please wait.");
            var stats = await service.GenerateBirthdayPokemonDistributionAsync();

            Console.WriteLine("\nTop 10 Most Common Birthday Pokémon:");
            foreach (var entry in stats.Dictionary.Take(10))
            {
                Console.WriteLine($"#{entry.Key}: {entry.Value.Name} ({entry.Value.Count} birthdays)");
            }
            Console.WriteLine();
            continue;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error generating stats: {ex.Message}");
    }

    try
    {
        if (input.Equals("statscsv", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Generating CSV... this may take a minute.");
            string path = await service.ExportBirthdayPokemonDistributionToCsvAsync();
            Console.WriteLine($"\nCSV export completed! File saved to:\n{path}\n");
            continue;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error exporting CSV: {ex.Message}");
    }

    try
    {
        string explanation = await service.ExplainBirthdayPokemonAsync(input, formatType, calculationType);
        Console.WriteLine("\n=== Birthday Pokémon Analysis ===");
        Console.WriteLine(explanation);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }



    Console.Write("\nTry another birthday? (Y/N): ");
    var again = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(again))
    {
        break;
    }
    if (!again.Equals("Y", StringComparison.OrdinalIgnoreCase))
        break;
}

Console.WriteLine("Goodbye!");
