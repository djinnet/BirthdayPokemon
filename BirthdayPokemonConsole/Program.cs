using BirthdayPokemonCore;
using BirthdayPokemonCore.Repo;

var repo = new PokeAPIRepo();
var service = new PokemonService(repo);

while (true)
{
    Console.Write("Enter your birthday (MM/DD or DD/MM): ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        break;

    Console.Write("Is this MM/DD format or DD/MM format? (Enter 'M' for MM/DD, 'D' for DD/MM, or press Enter for default DD/MM): ");
    string? formatInput = Console.ReadLine();

    var formatType = formatInput?.Equals("M", StringComparison.OrdinalIgnoreCase) == true
        ? BirthdayPokemonCore.Data.FormatType.MonthDay
        : BirthdayPokemonCore.Data.FormatType.DayMonth;



    try
    {
        if (input.Equals("stats", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Calculating distribution... please wait.");
            var stats = await service.GenerateBirthdayPokemonDistributionAsync();

            Console.WriteLine("\nTop 10 Most Common Birthday Pokémon:");
            foreach (var entry in stats.Take(10))
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
        string explanation = await service.ExplainBirthdayPokemonAsync(input, formatType);
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
