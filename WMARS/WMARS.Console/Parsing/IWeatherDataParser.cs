using WMARS.Models;

namespace WMARS.Parsing;

/// <summary>
/// Parses raw weather data of a specific format into <see cref="WeatherData"/>.
/// Adding support for a new format means adding a new implementation of this interface - no existing code changes (OCP).
/// </summary>
public interface IWeatherDataParser
{
    /// <summary>
    /// The format identifiers this parser handles (e.g. "json", "xml").
    /// Matched case-insensitively against a file's extension.
    /// </summary>
    IReadOnlyCollection<string> SupportedFormats { get; }

    /// <summary>
    /// Parses the full file content into a <see cref="WeatherData"/> instance.
    /// Throws <see cref="FormatException"/> when the content is malformed or
    /// missing required fields.
    /// </summary>
    WeatherData Parse(string content);
}
