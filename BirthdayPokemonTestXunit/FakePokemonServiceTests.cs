using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Models;
using BirthdayPokemonCore.Repo;
using BirthdayPokemonCore.Services;
using BirthdayPokemonTestXunit.Tests;

namespace BirthdayPokemonTestXunit
{
    public class FakePokemonServiceTests
    {
        private readonly PokemonService _service;

        public FakePokemonServiceTests()
        {
            // use the fake repo for testing
            var mockRepo = new FakePokeAPIRepo();

            //and use in-memory logger
            InMemoryLogger<PokemonService> logger = new();
            _service = new PokemonService(mockRepo, logger);
        }

        [Theory]
        [InlineData("07/07", 707, 707, "Pokémon #707")] // valid direct
        [InlineData("11/03", 1103, 78, "Rapidash")]    // wraps
        [InlineData("10/25", 1025, 1025, "Pecharunt")] // edge max
        [InlineData("01/01", 101, 101, "Pokémon #101")] // low number
        public async Task GetBirthdayPokemonAsync_ReturnsExpectedResults(string date, int expectedRaw, int expectedDex, string expectedName)
        {
            DateOnly birthday = DateOnly.Parse(date);
            int raw = _service.GetBirthdayNumber(birthday);
            Assert.Equal(expectedRaw, raw);

            int normalized = _service.CalculateNormalizedBirthdayNumber(birthday, EPokemonBirthdayCalculationType.Standard);
            Assert.Equal(expectedDex, normalized);

            PokemonInfo? pokemon = await _service.GetPokemonInfoByDexAsync(normalized);
            Assert.NotNull(pokemon);
            Assert.Equal(expectedName,  pokemon.Name);
        }

        [Theory]
        [InlineData("01/01", 101)]
        [InlineData("11/03", 78)]
        [InlineData("10/25", 1025)]
        [InlineData("10/26", 1)] // Wraps around
        [InlineData("03/07", 307)]
        [InlineData("02/29/2020", 229)] // Leap year
        public void GetBirthdayPokemon_WorksForAllEdgeCases(string date, int expectedDex)
        {
            var birthday = DateOnly.Parse(date);
            var normalized = _service.CalculateNormalizedBirthdayNumber(birthday, EPokemonBirthdayCalculationType.Standard);
            Assert.Equal(expectedDex, normalized);
        }

        [Theory]
        [InlineData("13/40")]
        [InlineData("00/00")]
        public void InvalidDates_ThrowOrFailGracefully(string date)
        {
            Assert.ThrowsAny<Exception>(() => DateTime.Parse(date));
        }

        [Fact]
        public void InvalidDate_ThrowsError()
        {
            Assert.Throws<FormatException>(() => DateTime.Parse("13/40"));
        }

        [Fact]
        public void NormalizeDexNumber_ThrowsForInvalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.NormalizeDexNumber(0));
        }
    }
}
