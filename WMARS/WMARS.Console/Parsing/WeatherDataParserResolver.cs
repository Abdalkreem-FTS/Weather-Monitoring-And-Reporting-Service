using System.Diagnostics.CodeAnalysis;

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
    /// Finds the parser that handles <paramref name="format"/> (case-insensitive).
    /// </summary>
    public bool TryResolve(string format, [MaybeNullWhen(false)] out IWeatherDataParser parser)
    {
        parser = _parsers.FirstOrDefault(p => p.SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase));

        return parser is not null;
    }
}
