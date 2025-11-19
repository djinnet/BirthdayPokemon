using BirthdayPokemonConsole;
using BirthdayPokemonCore.Data.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddBirthdayPokemonServices();
services.AddScoped<ICore, Core>();
services.AddLogging(options =>
{
    options.ClearProviders();
    options.AddConsole();
});
var serviceProvider = services.BuildServiceProvider();

var core = serviceProvider.GetRequiredService<ICore>();
await core.RunAsync();


