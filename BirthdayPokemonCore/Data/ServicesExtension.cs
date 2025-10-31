using BirthdayPokemonCore.Repo;
using Microsoft.Extensions.DependencyInjection;

namespace BirthdayPokemonCore.Data
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
