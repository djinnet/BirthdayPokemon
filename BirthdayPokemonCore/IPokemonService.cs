using BirthdayPokemonCore.Data;
using BirthdayPokemonCore.Repo;

namespace BirthdayPokemonCore
{
    public interface IPokemonService
    {
        Task<string> ExplainBirthdayPokemonAsync(string input);
        Task<string> ExplainBirthdayPokemonAsync(string input, FormatType formatUsed);
        Task<string> ExportBirthdayPokemonDistributionToCsvAsync(string filePath = "birthday_pokemon_stats.csv");
        Task<Dictionary<int, (string Name, int Count)>> GenerateBirthdayPokemonDistributionAsync();
        int GetBirthdayNumber(DateOnly birthday, FormatType formatused = FormatType.DayMonth);
        Task<(int DexNumber, string Name)> GetBirthdayPokemonAsync(DateOnly birthday, FormatType formatType);
        Task<(PokemonInfo? info, string steps)> GetBirthdayPokemonInfoAsync(DateOnly birthday, FormatType formatType);
        Task<PokemonInfo?> GetPokemonInfoByDexAsync(int dexNumber);
        Task<string> GetPokemonNameByDexAsync(int dexNumber);
        int NormalizeDexNumber(int number);
    }
}