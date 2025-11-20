using BirthdayPokemonCore.Interfaces;
using BirthdayPokemonCore.Models;
using Microsoft.Extensions.Logging;
using PokeApiNet;
using System.Collections.Concurrent;
using System.Globalization;

namespace BirthdayPokemonCore.Repo
{
    public class PokeAPIRepo : IPokemonRepository
    {
        private readonly PokeApiClient _client = new();

        private readonly ConcurrentDictionary<int, PokemonInfo> _cachePokemonInfo = new();

        private ILogger<PokeAPIRepo> _logger { get; }

        public PokeAPIRepo(ILogger<PokeAPIRepo> logger)
        {
            _logger = logger;
        }

        public async Task<PokemonInfo> GetPokemonInfoAsync(int dexNumber)
        {
            if (_cachePokemonInfo.TryGetValue(dexNumber, out PokemonInfo? cached))
                return cached;

            try
            {
                var pokemon = await _client.GetResourceAsync<Pokemon>(dexNumber) ?? throw new Exception($"Pokémon with dex number {dexNumber} not found.");
                if (string.IsNullOrEmpty(pokemon.Name))
                {
                    throw new Exception($"Pokémon with dex number {dexNumber} has no name.");
                }
                string name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pokemon.Name);
                string imageUrl = pokemon.Sprites.Other.Home.FrontDefault;
                PokemonInfo info = new(name, imageUrl, dexNumber);
                _cachePokemonInfo[dexNumber] = info;
                return info;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error fetching Pokémon info for dex number {DexNumber}", dexNumber);
                return new PokemonInfo($"Pokémon #{dexNumber}", string.Empty, dexNumber);
            }
        }
    }
}
