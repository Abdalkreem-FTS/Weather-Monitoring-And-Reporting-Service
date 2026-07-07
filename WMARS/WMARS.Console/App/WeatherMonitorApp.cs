using WMARS.Bots;
using WMARS.Models;
using WMARS.Observers;
using WMARS.Parsing;
using WMARS.Reporting;

namespace WMARS.App;

/// <summary>
/// Application shell: subscribes the configured bots to the station and runs
/// the interactive read-parse-publish loop. All I/O goes through <see cref="IConsoleUi"/>.
/// </summary>
public sealed class WeatherMonitorApp(
    IWeatherSubject station,
    IEnumerable<WeatherBot> bots,
    WeatherDataParserResolver resolver,
    IConsoleUi ui)
{
    private readonly IReadOnlyList<WeatherBot> _bots = bots.ToList();

    public void Run()
    {
        foreach (var bot in _bots)
        {
            station.Subscribe(bot);
        }

        ui.ShowWelcome(resolver.SupportedFormats.ToList());

        while (true)
        {
            var input = ui.ReadPath();

            if (input is null)
            {
                break;
            }

            input = input.Trim().Trim('"');

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (input.Length == 0)
            {
                continue;
            }

            try
            {
                var data = ReadWeatherData(input);
                ui.ShowReceived(data);
                station.Publish(data);
            }
            catch (Exception ex)
            {
                ui.ShowError(ex.Message);
            }
        }

        ui.ShowGoodbye();
    }

    private WeatherData ReadWeatherData(string path)
    {
        var resolvedPath = ResolvePath(path);

        var extension = Path.GetExtension(resolvedPath).TrimStart('.');
        if (extension.Length == 0)
        {
            throw new FormatException("The file has no extension, so its format cannot be determined.");
        }

        if (!resolver.TryResolve(extension, out var parser))
        {
            var supported = string.Join(", ", resolver.SupportedFormats.Select(f => "." + f));
            throw new NotSupportedException($"Unsupported format '.{extension}'. Supported formats: {supported}.");
        }

        var content = File.ReadAllText(resolvedPath);
        return string.IsNullOrWhiteSpace(content) ? throw new FormatException("The file is empty.") : parser.Parse(content);
    }

    private static string ResolvePath(string path)
    {
        if (File.Exists(path))
        {
            return path;
        }

        var baseRelative = Path.Combine(AppContext.BaseDirectory, path);

        return File.Exists(baseRelative) ? baseRelative : throw new FileNotFoundException($"File not found: {path}");
    }
}
