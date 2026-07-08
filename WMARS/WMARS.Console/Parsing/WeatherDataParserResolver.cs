using WMARS.Results;

namespace WMARS.Parsing;

/// <summary>
/// Selects the appropriate <see cref="IWeatherDataParser"/> for a given format
/// (Strategy selection). New formats are added by registering another parser -
/// this class never needs to change.
/// </summary>
public sealed class WeatherDataParserResolver(IEnumerable<IWeatherDataParser> parsers)
{
    private readonly IReadOnlyList<IWeatherDataParser> _parsers = parsers.ToList();

    /// <summary>All formats supported by the registered parsers.</summary>
    public IEnumerable<string> SupportedFormats => _parsers.SelectMany(parser => parser.SupportedFormats).Distinct(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Finds the parser that handles <paramref name="format"/> (case-insensitive),
    /// or a failed <see cref="Result{IWeatherDataParser}"/> describing the
    /// unsupported format when none is registered.
    /// </summary>
    public Result<IWeatherDataParser> Resolve(string format)
    {
        var parser = _parsers.FirstOrDefault(p => p.SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase));

        if (parser is not null)
        {
            return Result<IWeatherDataParser>.From(parser);
        }
        
        var supported = string.Join(", ", SupportedFormats.Select(f => "." + f));
        return Error.NotFound(
            "Parser.UnsupportedFormat",
            $"Unsupported format '.{format}'. Supported formats: {supported}.");

    }
}
