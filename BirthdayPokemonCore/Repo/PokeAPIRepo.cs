using PokeApiNet;
using System.Collections.Concurrent;
using System.Globalization;

namespace BirthdayPokemonCore.Repo
{
    public class PokeAPIRepo : IPokemonRepository
    {
        private readonly PokeApiClient _client = new();
        private readonly ConcurrentDictionary<int, string> _cache = new();

        private readonly ConcurrentDictionary<int, PokemonInfo> _cachePokemonInfo = new();

        public async Task<string> GetPokemonNameAsync(int dexNumber)
        {
            if (_cache.TryGetValue(dexNumber, out string? cached))
                return cached;

            try
            {
                var pokemon = await _client.GetResourceAsync<Pokemon>(dexNumber);
                string name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pokemon.Name);
                _cache[dexNumber] = name;
                return name;
            }
            catch
            {
                string fallback = $"Pokémon #{dexNumber}";
                _cache[dexNumber] = fallback;
                return fallback;
            }
        }

        public async Task<PokemonInfo> GetPokemonInfoAsync(int dexNumber)
        {
            if (_cachePokemonInfo.TryGetValue(dexNumber, out PokemonInfo? cached))
                return cached;

            try
            {
                var pokemon = await _client.GetResourceAsync<Pokemon>(dexNumber);
                string name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pokemon.Name);
                string imageUrl = pokemon.Sprites.Other.Home.FrontDefault;
                var info = new PokemonInfo(name, imageUrl, dexNumber);
                _cachePokemonInfo[dexNumber] = info;
                return info;
            }
            catch
            {
                string fallback = $"Pokémon #{dexNumber}";
                var info = new PokemonInfo(fallback, string.Empty, dexNumber);
                _cachePokemonInfo[dexNumber] = info;
                return info;
            }
        }
    }

    public record PokemonInfo(string Name, string ImageUrl, int DexNumber);
}
