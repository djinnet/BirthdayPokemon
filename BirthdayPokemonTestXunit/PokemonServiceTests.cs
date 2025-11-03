using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Repo;
using BirthdayPokemonCore.Services;
using BirthdayPokemonTestXunit.Tests;

namespace BirthdayPokemonTestXunit
{
    public class PokemonServiceTests
    {
        private readonly PokemonService _service;

        public PokemonServiceTests()
        {
            var mockRepo = new FakePokeAPIRepo();
            _service = new PokemonService(mockRepo);
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

            string name = await _service.GetPokemonNameByDexAsync(normalized);
            Assert.Equal(expectedName, name);
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

        [Fact(Skip = "Requires internet connection; live API call.")]
        public async Task BirthdayPokemon_LiveApi_31_03_2025_Works()
        {
            var repo = new PokeAPIRepo();
            var service = new PokemonService(repo);

            //inputs
            var format = EFormatType.DayMonth;

            // tests
            DateOnly birthday = new(2025, 03, 31);
            var pokemon = await service.GetBirthdayPokemonInfoAsync(birthday, format, EPokemonBirthdayCalculationType.Standard);

            Assert.NotNull(pokemon);

            // results
            Assert.InRange(pokemon.Info!.DexNumber, 1, 1025);
            Assert.False(string.IsNullOrWhiteSpace(pokemon.Info.Name));

            Console.WriteLine($"[LIVE TEST] 31/03/2025 -> #{pokemon.Info.DexNumber}: {pokemon.Info.Name}");
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
