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

        private string? calculationDetails;   // holds explanation text

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
            calculationDetails = null;
            try
            {
                FormatType formatUsed = MonthDayFormat ? FormatType.MonthDay : FormatType.DayMonth;
                (BirthdayPokemonCore.Repo.PokemonInfo? info, string steps) pokemonInfo = await PokemonService.GetBirthdayPokemonInfoAsync(birthday, formatUsed);
                if (pokemonInfo.info == null || string.IsNullOrEmpty(pokemonInfo.info.ImageUrl))
                {
                    ErrorMessage = "Pokémon not found.";
                    IsError = true;
                    return;
                }
                pokemonName = pokemonInfo.info.Name;
                pokemonImageUrl = pokemonInfo.info.ImageUrl;
                pokedexNumber = pokemonInfo.info.DexNumber;
                calculationDetails = pokemonInfo.steps;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                IsError = true;
                pokemonName = string.Empty;
                pokemonImageUrl = string.Empty;
                pokedexNumber = 0;
                calculationDetails = null;
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
