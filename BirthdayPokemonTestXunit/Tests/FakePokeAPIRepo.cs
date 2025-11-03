using BirthdayPokemonCore.Interfaces;
using BirthdayPokemonCore.Models;

namespace BirthdayPokemonTestXunit.Tests
{
    public class FakePokeAPIRepo : IPokemonRepository
    {
        private readonly Dictionary<int, string> _data = new()
        {
            { 1, "Bulbasaur" },
            { 25, "Pikachu" },
            { 78, "Rapidash" },
            { 1025, "Pecharunt" }
        };

        private readonly Dictionary<int, PokemonInfo> _PokeinfoData = new()
        {
            { 1, new PokemonInfo("Bulbasaur", "https://example.com/bulbasaur.png", 1) },
            { 25, new PokemonInfo("Pikachu", "https://example.com/pikachu.png", 25) },
            { 78, new PokemonInfo("Rapidash", "https://example.com/rapidash.png", 78) },
            { 1025, new PokemonInfo("Pecharunt", "https://example.com/pecharunt.png", 1025) }
        };

        public Task<PokemonInfo> GetPokemonInfoAsync(int dexNumber)
        {
            if (_PokeinfoData.TryGetValue(dexNumber, out var info))
                return Task.FromResult(info);

            return Task.FromResult(new PokemonInfo($"Pokémon #{dexNumber}", string.Empty, dexNumber));
        }

        public Task<string> GetPokemonNameAsync(int dexNumber)
        {
            if (_data.TryGetValue(dexNumber, out var name))
                return Task.FromResult(name);

            return Task.FromResult($"Pokémon #{dexNumber}");
        }
    }
}
