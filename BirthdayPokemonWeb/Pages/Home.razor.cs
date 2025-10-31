using BirthdayPokemonCore;
using BirthdayPokemonCore.Data;
using Microsoft.AspNetCore.Components;

namespace BirthdayPokemonWeb.Pages
{
    public partial class Home
    {
        public string pokemonName { get; set; } = string.Empty;
        public string pokemonImageUrl { get; set; } = string.Empty;
        public int pokedexNumber { get; set; } = 0;
        public DateOnly birthday { get; set; }

        public string inputFormat => MonthDayFormat ? "MM/dd" : "dd/MM";

        public bool isLoading { get; set; } = false;

        public bool IsError { get; set; } = false;

        public string ErrorMessage { get; set; } = string.Empty;

        public bool MonthDayFormat { get; set; } = true;

        [Inject]
        public IPokemonService PokemonService { get; set; }

        protected override void OnInitialized()
        {
            birthday = DateOnly.FromDateTime(DateTime.Today);
        }

        //onclick handler
        public async Task OnGetBirthdayPokemonAsync()
        {
            isLoading = true;
            IsError = false;
            try
            {
                FormatType formatUsed = MonthDayFormat ? FormatType.MonthDay : FormatType.DayMonth;
                int rawNumber = PokemonService.GetBirthdayNumber(birthday, formatUsed);
                int normalizedDex = PokemonService.NormalizeDexNumber(rawNumber);
                var pokemonInfo = await PokemonService.GetPokemonInfoByDexAsync(normalizedDex);
                if (pokemonInfo == null || string.IsNullOrEmpty(pokemonInfo.ImageUrl))
                {
                    ErrorMessage = "Pokémon not found.";
                    IsError = true;
                    return;
                }
                pokemonName = pokemonInfo.Name;
                pokemonImageUrl = pokemonInfo.ImageUrl;
                pokedexNumber = pokemonInfo.DexNumber;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                IsError = true;
                pokemonName = string.Empty;
                pokemonImageUrl = string.Empty;
                pokedexNumber = 0;
            }
            finally
            {
                if (isLoading)
                {
                    await InvokeAsync(() => StateHasChanged());
                }
                isLoading = false;
            }
        }


    }
}
