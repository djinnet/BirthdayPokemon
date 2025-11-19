using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Models;

namespace BirthdayPokemonCore.Interfaces
{
    public interface IPokemonService
    {
        int CalculateNormalizedBirthdayNumber(DateOnly birthday, EPokemonBirthdayCalculationType calculationType, EFormatType formatType = EFormatType.DayMonth);

        Task<string> ExplainBirthdayPokemonAsync(string input, EFormatType formatUsed, EPokemonBirthdayCalculationType birthdayCalculationType = EPokemonBirthdayCalculationType.Standard);
        Task<string> ExportBirthdayPokemonDistributionToCsvAsync(string filePath = "birthday_pokemon_stats.csv", EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard);
        Task<PokemonDictionaryDistributation> GenerateBirthdayPokemonDistributionAsync(EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard);
        int GetBirthdayNumber(DateOnly birthday, EFormatType formatused = EFormatType.DayMonth);
        Task<PokemonInfoWithSteps?> GetBirthdayPokemonInfoAsync(DateOnly birthday, EFormatType formatType, EPokemonBirthdayCalculationType calculationType = EPokemonBirthdayCalculationType.Standard);
        Task<PokemonInfo?> GetPokemonInfoByDexAsync(int dexNumber);
        int NormalizeDexNumber(int number);
    }
}