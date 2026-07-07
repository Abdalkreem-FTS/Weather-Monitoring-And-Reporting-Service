using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using WMARS.Models;

namespace WMARS.Parsing;

/// <summary>
/// Parses weather data supplied as XML
/// </summary>
public sealed class XmlWeatherDataParser : IWeatherDataParser
{
    public IReadOnlyCollection<string> SupportedFormats { get; } = ["xml"];

    public WeatherData Parse(string content)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(content);
        }
        catch (XmlException ex)
        {
            throw new FormatException("The XML content is not well-formed.", ex);
        }

        var root = document.Root ?? throw new FormatException("The XML content has no root element.");

        var location = GetRequired(root, "Location");
        var temperature = ParseNumber(GetRequired(root, "Temperature"), "Temperature");
        var humidity = ParseNumber(GetRequired(root, "Humidity"), "Humidity");

        return new WeatherData(location, temperature, humidity);
    }

    private static string GetRequired(XElement root, string name)
    {
        var element = root.Element(name);
        if (element is null || string.IsNullOrWhiteSpace(element.Value))
        {
            throw new FormatException($"Missing required element: {name}.");
        }

        return element.Value.Trim();
    }

    private static double ParseNumber(string value, string name)
    {
        return !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? throw new FormatException($"Element '{name}' is not a valid number: '{value}'.") : result;
    }
}
