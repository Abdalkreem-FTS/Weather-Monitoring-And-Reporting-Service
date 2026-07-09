using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using WMARS.Models;
using WMARS.Results;

namespace WMARS.Parsing;

/// <summary>
/// Parses weather data supplied as XML
/// </summary>
public sealed class XmlWeatherDataParser : IWeatherDataParser
{
    public IReadOnlyCollection<string> SupportedFormats { get; } = ["xml"];

    public Result<WeatherData> Parse(string content)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(content);
        }
        catch (XmlException)
        {
            return Error.Validation("Parser.Xml.Malformed", "The XML content is not well-formed.");
        }

        if (document.Root is not { } root)
        {
            return Error.Validation("Parser.Xml.NoRoot", "The XML content has no root element.");
        }

        var location = GetRequired(root, "Location");
        if (location.IsError)
        {
            return location.Errors;
        }

        var temperatureText = GetRequired(root, "Temperature");
        if (temperatureText.IsError)
        {
            return temperatureText.Errors;
        }

        var temperature = ParseNumber(temperatureText.Value, "Temperature");
        if (temperature.IsError)
        {
            return temperature.Errors;
        }

        var humidityText = GetRequired(root, "Humidity");
        if (humidityText.IsError)
        {
            return humidityText.Errors;
        }

        var humidity = ParseNumber(humidityText.Value, "Humidity");
        if (humidity.IsError)
        {
            return humidity.Errors;
        }

        return new WeatherData(location.Value, temperature.Value, humidity.Value);
    }

    private static Result<string> GetRequired(XElement root, string name)
    {
        var element = root.Element(name);
        if (element is null || string.IsNullOrWhiteSpace(element.Value))
        {
            return Error.Validation("Parser.Xml.MissingElement", $"Missing required element: {name}.");
        }

        return element.Value.Trim();
    }

    private static Result<double> ParseNumber(string value, string name)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : Error.Validation("Parser.Xml.InvalidNumber", $"Element '{name}' is not a valid number: '{value}'.");
    }
}
