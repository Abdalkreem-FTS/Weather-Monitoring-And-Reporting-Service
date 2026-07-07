using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using WMARS.App;
using WMARS.Bots;
using WMARS.Configuration;
using WMARS.Observers;
using WMARS.Parsing;
using WMARS.Reporting;

// Ensure box-drawing glyphs and symbols render correctly.
try
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;
}
catch
{
    // Some redirected/legacy consoles reject this; safe to ignore.
}

IReadOnlyDictionary<string, BotConfiguration> configuration;
try
{
    var configurationPath = Path.Combine(AppContext.BaseDirectory, "bots.json");
    configuration = await ConfigurationLoader.Load(configurationPath);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Failed to load configuration:[/] {Markup.Escape(ex.Message)}");
    return;
}

var services = new ServiceCollection();

// Presentation: one Spectre implementation, exposed through two focused interfaces.
services.AddSingleton<SpectreConsoleUi>();
services.AddSingleton<IConsoleUi>(sp => sp.GetRequiredService<SpectreConsoleUi>());
services.AddSingleton<IActivationReporter>(sp => sp.GetRequiredService<SpectreConsoleUi>());

services.AddSingleton<IWeatherDataParser, JsonWeatherDataParser>();
services.AddSingleton<IWeatherDataParser, XmlWeatherDataParser>();
services.AddSingleton<WeatherDataParserResolver>();

services.AddSingleton<IWeatherSubject, WeatherStation>();

RegisterBots(services, configuration);

services.AddSingleton<WeatherMonitorApp>();

await using var provider = services.BuildServiceProvider();

var app = provider.GetRequiredService<WeatherMonitorApp>();
app.Run();

return;

static void RegisterBots(IServiceCollection services, IReadOnlyDictionary<string, BotConfiguration> config)
{
    if (config.TryGetValue("RainBot", out var rain))
    {
        services.AddSingleton<WeatherBot>(sp =>
            new RainBot(rain.Enabled, rain.HumidityThreshold ?? 0, rain.Message,
                sp.GetRequiredService<IActivationReporter>()));
    }

    if (config.TryGetValue("SunBot", out var sun))
    {
        services.AddSingleton<WeatherBot>(sp =>
            new SunBot(sun.Enabled, sun.TemperatureThreshold ?? 0, sun.Message,
                sp.GetRequiredService<IActivationReporter>()));
    }

    if (config.TryGetValue("SnowBot", out var snow))
    {
        services.AddSingleton<WeatherBot>(sp =>
            new SnowBot(snow.Enabled, snow.TemperatureThreshold ?? 0, snow.Message,
                sp.GetRequiredService<IActivationReporter>()));
    }
}
