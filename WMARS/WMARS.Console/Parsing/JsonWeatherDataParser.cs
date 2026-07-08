using System.Text.Json;
using WMARS.Models;
using WMARS.Results;

namespace WMARS.Parsing;

/// <summary>
/// Parses weather data supplied as JSON
/// </summary>
public sealed class JsonWeatherDataParser : IWeatherDataParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> SupportedFormats { get; } = ["json"];

    public Result<WeatherData> Parse(string content)
    {
        WeatherDataDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<WeatherDataDto>(content, Options);
        }
        catch (JsonException)
        {
            return Error.Validation("Parser.Json.Malformed", "The JSON content is not well-formed.");
        }

        return dto is null
            ? Error.Validation("Parser.Json.Empty", "The JSON content is empty.")
            : dto.ToWeatherData();
    }

    /// <summary>
    /// Nullable data-transfer object so missing fields can be detected and reported instead of silently defaulting to 0.
    /// </summary>
    private sealed class WeatherDataDto
    {
        public string? Location { get; init; }
        public double? Temperature { get; init; }
        public double? Humidity { get; init; }

        public Result<WeatherData> ToWeatherData()
        {
            if (string.IsNullOrWhiteSpace(Location))
            {
                return Error.Validation("WeatherData.Location", "Missing required field: Location.");
            }

            if (Temperature is null)
            {
                return Error.Validation("WeatherData.Temperature", "Missing required field: Temperature.");
            }

            return Humidity is null
                ? Error.Validation("WeatherData.Humidity", "Missing required field: Humidity.")
                : new WeatherData(Location, Temperature.Value, Humidity.Value);
        }
    }
}
