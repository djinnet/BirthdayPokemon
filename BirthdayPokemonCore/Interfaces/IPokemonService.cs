using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Models;

namespace BirthdayPokemonCore.Interfaces
{
    public interface IPokemonService
    {
        int CalculateNormalizedBirthdayNumber(DateOnly birthday, EPokemonBirthdayCalculationType calculationType);

        Task<string> ExplainBirthdayPokemonAsync(string input, EFormatType formatUsed, EPokemonBirthdayCalculationType birthdayCalculationType = EPokemonBirthdayCalculationType.Standard);
        Task<string> ExportBirthdayPokemonDistributionToCsvAsync(string filePath = "birthday_pokemon_stats.csv");
        Task<PokemonDictionaryDistributation> GenerateBirthdayPokemonDistributionAsync();
        int GetBirthdayNumber(DateOnly birthday, EFormatType formatused = EFormatType.DayMonth);
        Task<PokemonInfoWithSteps> GetBirthdayPokemonInfoAsync(DateOnly birthday, EFormatType formatType, EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard);
        Task<PokemonInfo?> GetPokemonInfoByDexAsync(int dexNumber);
        Task<string> GetPokemonNameByDexAsync(int dexNumber);
        int NormalizeDexNumber(int number);
    }
}