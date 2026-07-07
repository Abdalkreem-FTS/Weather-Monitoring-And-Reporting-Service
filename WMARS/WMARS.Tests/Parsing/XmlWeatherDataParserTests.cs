using WMARS.Models;
using WMARS.Parsing;

namespace WMARS.Tests.Parsing;

public class XmlWeatherDataParserTests
{
    private readonly XmlWeatherDataParser _parser = new();

    [Fact]
    public void SupportedFormats_Contains_Xml() =>
        Assert.Contains("xml", _parser.SupportedFormats);

    [Fact]
    public void Parse_Valid_Multiline_Xml_Returns_Weather_Data()
    {
        const string xml =
            "<WeatherData>\n" +
            "  <Location>London</Location>\n" +
            "  <Temperature>12.0</Temperature>\n" +
            "  <Humidity>85.0</Humidity>\n" +
            "</WeatherData>";

        var result = _parser.Parse(xml);

        Assert.Equal(new WeatherData("London", 12.0, 85.0), result);
    }

    [Fact]
    public void Parse_Malformed_Xml_Throws_FormatException()
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Parse("<WeatherData>"));
        Assert.Contains("well-formed", ex.Message);
    }

    [Fact]
    public void Parse_Missing_Element_Throws_With_Element_Name()
    {
        const string xml = "<WeatherData><Location>X</Location><Humidity>50</Humidity></WeatherData>";

        var ex = Assert.Throws<FormatException>(() => _parser.Parse(xml));
        Assert.Contains("Temperature", ex.Message);
    }

    [Fact]
    public void Parse_Non_Numeric_Temperature_Throws_FormatException()
    {
        const string xml =
            "<WeatherData><Location>X</Location><Temperature>hot</Temperature><Humidity>50</Humidity></WeatherData>";

        var ex = Assert.Throws<FormatException>(() => _parser.Parse(xml));
        Assert.Contains("Temperature", ex.Message);
    }
}
