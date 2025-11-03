namespace BirthdayPokemonCore.Data.Enums
{
    /// <summary>
    /// There are 3 types of calculation methods for determining the birthday Pokémon.
    /// Standard Calculation: Proper modular wrap around maximum dex number
    /// Seed Calculation: Uses a seed value to offset the calculation
    /// Year-Based Calculation: Incorporates the birth year into the calculation
    /// </summary>
    public enum EPokemonBirthdayCalculationType
    {
        Standard,
        Seed,
        YearBased
    }
}
