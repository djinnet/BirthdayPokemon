# Birthday Pokémon — Find Your Personal Pokédex Match!
Welcome to Birthday Pokémon — a fun little project that tells you which Pokémon matches your birthday!
Your birthday is converted into a number that maps to a Pokémon in the National Pokédex.

You can run the project in two ways:

✅ Console App (C#)
✅ Blazor WebAssembly Web App
➡️ Live website: https://djinnet.github.io/BirthdayPokemon/

Both versions use the PokéAPI to fetch live Pokémon names.

# What this project does
When you enter your birthday (like 07/07 or 31/03), the program:
1.Detects which date format you want: MM/DD or DD/MM

2. Converts your birthday into a number between 1 and 1025 (the max number of Pokémon in the National Pokédex)
- 07/07 → 0707 → 707
- 31/03 (DD/MM) → 3103 → wraps → 28

3. Wraps the number so it fits inside the Pokédex (1–1025): 
'normalized = ((raw - 1) % 1025) + 1'

4. Fetches the Pokémon name with that normalized Pokédex number from the PokéAPI

5. Displays your Birthday Pokémon! Alternatively you can also get an more detailed breakdown of the calculation steps in the console app. Web app shows only the final result for now.

# Main Features
✅ You choose the date format (DD/MM or MM/DD)

No confusion — you pick the format.

✅ Detailed explanations

The console app shows how the program interpreted your date and how it calculated your Pokémon.

✅ PokéAPI live integration

Pokémon names are fetched from the official PokéAPI using the .NET wrapper PokeApiNet.

✅ Caching system

All Pokémon lookups are automatically cached to avoid unnecessary API calls.

✅ Statistical analysis mode

Type stats to analyze every possible birthday and see which Pokémon appear most often.

✅ CSV export mode

Type statscsv to export the full statistical dataset to a CSV file you can open in Excel.

✅ Blazor WebAssembly version

A friendly browser version is available here:
👉 https://djinnet.github.io/BirthdayPokemon/

(also included inside the solution)

# Technologies Used
| Component    | Technology                                |
| ------------ | ----------------------------------------- |
| Console App  | C# (.NET 9)                               |
| Web App      | Blazor WebAssembly                        |
| Pokémon Data | PokéAPI (via PokeApiNet)                  |
| Testing      | xUnit                                     |
| Architecture | Repository Pattern + Dependency Injection |
| Caching      | ConcurrentDictionary                      |

# Supported Commands (Console App)
1. follow the prompts in the console to enter your birthday and get your Birthday Pokémon.

Find your birthday Pokémon with a full explanation.

2. stats

Runs the full statistical analysis:

Loops all 366 birthdays (leap year considered)

Normalizes the numbers

Counts how often each Pokémon appears

Shows the most common ones

3. statscsv

Same as stats but saves birthday_pokemon_stats.csv in the app folder.

Perfect for:

- Data analysis

- Excel charts

- Experiments


