using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Interfaces;
using Microsoft.AspNetCore.Components;
using BirthdayPokemonCore.Models;

namespace BirthdayPokemonRazorCore.Pages
{
    public partial class Home
    {
        public string PokemonName { get; set; } = string.Empty;
        public string PokemonImageUrl { get; set; } = string.Empty;
        public int PokedexNumber { get; set; } = 0;
        public DateOnly Birthday { get; set; }

        public string InputFormat => MonthDayFormat ? "MM/dd" : "dd/MM";

        public bool IsLoading { get; set; } = false;

        public bool IsError { get; set; } = false;

        public string ErrorMessage { get; set; } = string.Empty;

        public bool MonthDayFormat { get; set; } = true;

        public EPokemonBirthdayCalculationType CurrentCalcuationType { get; set; } = EPokemonBirthdayCalculationType.Standard;

        private string? calculationDetails;   // holds explanation text

        [Inject]
        public IPokemonService PokemonService { get; set; } = null!;

        protected override void OnInitialized()
        {
            Birthday = DateOnly.FromDateTime(DateTime.Today);
        }

        //Onclick handler
        public void OnSelectCalculationType(EPokemonBirthdayCalculationType type)
        {
            CurrentCalcuationType = type;
        }

        //onclick handler
        public async Task OnGetBirthdayPokemonAsync()
        {
            IsLoading = true;
            IsError = false;
            calculationDetails = null;
            try
            {
                EFormatType formatUsed = MonthDayFormat ? EFormatType.MonthDay : EFormatType.DayMonth;
                PokemonInfoWithSteps? pokemonInfo = await PokemonService.GetBirthdayPokemonInfoAsync(Birthday, formatUsed, CurrentCalcuationType);
                
                if(pokemonInfo == null) {
                    ErrorMessage = "Pokémon not found.";
                    IsError = true;
                    return;
                }

                if (pokemonInfo.Info == null || string.IsNullOrEmpty(pokemonInfo.Info.ImageUrl))
                {
                    ErrorMessage = "Pokémon info not found.";
                    IsError = true;
                    return;
                }
                PokemonName = pokemonInfo.Info.Name;
                PokemonImageUrl = pokemonInfo.Info.ImageUrl;
                PokedexNumber = pokemonInfo.Info.DexNumber;
                calculationDetails = pokemonInfo.Steps;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                IsError = true;
                PokemonName = string.Empty;
                PokemonImageUrl = string.Empty;
                PokedexNumber = 0;
                calculationDetails = null;
            }
            finally
            {
                if (IsLoading)
                {
                    await InvokeAsync(() => StateHasChanged());
                }
                IsLoading = false;
            }
        }


    }
}
