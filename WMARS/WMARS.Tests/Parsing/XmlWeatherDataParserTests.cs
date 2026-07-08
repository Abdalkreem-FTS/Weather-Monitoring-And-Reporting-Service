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

        Assert.True(result.IsSuccess);
        Assert.Equal(new WeatherData("London", 12.0, 85.0), result.Value);
    }

    [Fact]
    public void Parse_Malformed_Xml_Returns_Validation_Error()
    {
        var result = _parser.Parse("<WeatherData>");

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.TopError.Type);
        Assert.Contains("well-formed", result.TopError.Description);
    }

    [Fact]
    public void Parse_Missing_Element_Returns_Error_With_Element_Name()
    {
        const string xml = "<WeatherData><Location>X</Location><Humidity>50</Humidity></WeatherData>";

        var result = _parser.Parse(xml);

        Assert.True(result.IsError);
        Assert.Contains("Temperature", result.TopError.Description);
    }

    [Fact]
    public void Parse_Non_Numeric_Temperature_Returns_Validation_Error()
    {
        const string xml =
            "<WeatherData><Location>X</Location><Temperature>hot</Temperature><Humidity>50</Humidity></WeatherData>";

        var result = _parser.Parse(xml);

        Assert.True(result.IsError);
        Assert.Contains("Temperature", result.TopError.Description);
    }
}
