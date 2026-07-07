using System.Text.Json;
using WMARS.Models;

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

    public WeatherData Parse(string content)
    {
        WeatherDataDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<WeatherDataDto>(content, Options);
        }
        catch (JsonException ex)
        {
            throw new FormatException("The JSON content is not well-formed.", ex);
        }

        return dto is null ? throw new FormatException("The JSON content is empty.") : dto.ToWeatherData();
    }

    /// <summary>
    /// Nullable data-transfer object so missing fields can be detected and reported instead of silently defaulting to 0.
    /// </summary>
    private sealed class WeatherDataDto
    {
        public string? Location { get; init; }
        public double? Temperature { get; init; }
        public double? Humidity { get; init; }

        public WeatherData ToWeatherData()
        {
            if (string.IsNullOrWhiteSpace(Location))
            {
                throw new FormatException("Missing required field: Location.");
            }
            
            if (Temperature is null)
            {
                throw new FormatException("Missing required field: Temperature.");
            }
            
            return Humidity is null ? throw new FormatException("Missing required field: Humidity.") : new WeatherData(Location, Temperature.Value, Humidity.Value);
        }
    }
}
