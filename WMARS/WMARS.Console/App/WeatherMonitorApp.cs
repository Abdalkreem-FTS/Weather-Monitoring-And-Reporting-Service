using WMARS.Bots;
using WMARS.Models;
using WMARS.Observers;
using WMARS.Parsing;
using WMARS.Reporting;
using WMARS.Results;

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

            var result = ReadWeatherData(input);
            if (result.IsError)
            {
                ui.ShowError(result.TopError.Description);
                continue;
            }

            ui.ShowReceived(result.Value);
            station.Publish(result.Value);
        }

        ui.ShowGoodbye();
    }

    private Result<WeatherData> ReadWeatherData(string path)
    {
        var resolvedPath = ResolvePath(path);
        if (resolvedPath.IsError)
        {
            return resolvedPath.Errors;
        }

        var extension = Path.GetExtension(resolvedPath.Value).TrimStart('.');
        if (extension.Length == 0)
        {
            return Error.Validation(
                "File.NoExtension",
                "The file has no extension, so its format cannot be determined.");
        }

        var parser = resolver.Resolve(extension);
        if (parser.IsError)
        {
            return parser.Errors;
        }

        string content;
        try
        {
            content = File.ReadAllText(resolvedPath.Value);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Error.Failure("File.ReadError", $"Could not read the file: {ex.Message}");
        }

        return string.IsNullOrWhiteSpace(content)
            ? Error.Validation("File.Empty", "The file is empty.")
            : parser.Value.Parse(content);
    }

    private static Result<string> ResolvePath(string path)
    {
        if (File.Exists(path))
        {
            return path;
        }

        var baseRelative = Path.Combine(AppContext.BaseDirectory, path);

        return File.Exists(baseRelative)
            ? baseRelative
            : Error.NotFound("File.NotFound", $"File not found: {path}");
    }
}
