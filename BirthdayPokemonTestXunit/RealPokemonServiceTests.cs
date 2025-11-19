using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Repo;
using BirthdayPokemonCore.Services;
using BirthdayPokemonTestXunit.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayPokemonTestXunit
{
    public class RealPokemonServiceTests
    {
        private readonly PokemonService _service;

        public RealPokemonServiceTests()
        {
            // use the real repo for testing
            var realRepo = new PokeAPIRepo();
            //and use in-memory logger
            InMemoryLogger<PokemonService> logger = new();
            _service = new PokemonService(realRepo, logger);
        }

        [Fact(Skip = "Requires internet connection; live API call.")]
        public async Task BirthdayPokemon_LiveApi_31_03_2025_Works()
        {

            //inputs
            var format = EFormatType.DayMonth;

            // tests
            DateOnly birthday = new(2025, 03, 31);
            var pokemon = await _service.GetBirthdayPokemonInfoAsync(birthday, format, EPokemonBirthdayCalculationType.Standard);

            Assert.NotNull(pokemon);

            // results
            Assert.InRange(pokemon.Info!.DexNumber, 1, 1025);
            Assert.False(string.IsNullOrWhiteSpace(pokemon.Info.Name));

            Console.WriteLine($"[LIVE TEST] 31/03/2025 -> #{pokemon.Info.DexNumber}: {pokemon.Info.Name}");
        }
    }
}
