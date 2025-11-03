using BirthdayPokemonCore.Interfaces;
using BirthdayPokemonCore.Repo;
using BirthdayPokemonCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BirthdayPokemonCore.Data.Extensions
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddBirthdayPokemonServices(this IServiceCollection services)
        {
            services.AddScoped<IPokemonService, PokemonService>();
            services.AddScoped<IPokemonRepository, PokeAPIRepo>();
            return services;
        }
    }
}
