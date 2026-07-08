using WMARS.Models;
using WMARS.Results;

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
    /// Returns a failed <see cref="Result{WeatherData}"/> carrying a validation
    /// <see cref="Error"/> when the content is malformed or missing required fields.
    /// </summary>
    Result<WeatherData> Parse(string content);
}
