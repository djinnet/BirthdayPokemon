namespace BirthdayPokemonCore.Repo
{
    public interface IPokemonRepository
    {
        Task<PokemonInfo> GetPokemonInfoAsync(int dexNumber);
        Task<string> GetPokemonNameAsync(int dexNumber);
    }
}
