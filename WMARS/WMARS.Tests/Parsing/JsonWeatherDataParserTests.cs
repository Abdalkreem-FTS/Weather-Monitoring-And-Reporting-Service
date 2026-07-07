using WMARS.Models;
using WMARS.Parsing;

namespace WMARS.Tests.Parsing;

public class JsonWeatherDataParserTests
{
    private readonly JsonWeatherDataParser _parser = new();

    [Fact]
    public void SupportedFormats_Contains_Json() =>
        Assert.Contains("json", _parser.SupportedFormats);

    [Fact]
    public void Parse_Valid_Json_Returns_Weather_Data()
    {
        var result = _parser.Parse("""{ "Location": "Amman", "Temperature": 23.5, "Humidity": 85 }""");

        Assert.Equal(new WeatherData("Amman", 23.5, 85), result);
    }

    [Fact]
    public void Parse_Is_Case_Insensitive_For_Property_Names()
    {
        var result = _parser.Parse("""{ "location": "Cairo", "temperature": 40, "humidity": 20 }""");

        Assert.Equal(new WeatherData("Cairo", 40, 20), result);
    }

    [Fact]
    public void Parse_Malformed_Json_Throws_FormatException()
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Parse("{ not valid"));
        Assert.Contains("well-formed", ex.Message);
    }

    [Theory]
    [InlineData("""{ "Temperature": 10, "Humidity": 50 }""", "Location")]
    [InlineData("""{ "Location": "X", "Humidity": 50 }""", "Temperature")]
    [InlineData("""{ "Location": "X", "Temperature": 10 }""", "Humidity")]
    public void Parse_Missing_Field_Throws_With_Field_Name(string json, string field)
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Parse(json));
        Assert.Contains(field, ex.Message);
    }
}
