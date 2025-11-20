using BirthdayPokemonCore.Models;

namespace BirthdayPokemonCore.Interfaces
{
    public interface IPokemonRepository
    {
        Task<PokemonInfo> GetPokemonInfoAsync(int dexNumber);
    }
}
