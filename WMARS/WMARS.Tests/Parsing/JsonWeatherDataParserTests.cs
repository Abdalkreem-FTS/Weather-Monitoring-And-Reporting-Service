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

        Assert.True(result.IsSuccess);
        Assert.Equal(new WeatherData("Amman", 23.5, 85), result.Value);
    }

    [Fact]
    public void Parse_Is_Case_Insensitive_For_Property_Names()
    {
        var result = _parser.Parse("""{ "location": "Cairo", "temperature": 40, "humidity": 20 }""");

        Assert.True(result.IsSuccess);
        Assert.Equal(new WeatherData("Cairo", 40, 20), result.Value);
    }

    [Fact]
    public void Parse_Malformed_Json_Returns_Validation_Error()
    {
        var result = _parser.Parse("{ not valid");

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.TopError.Type);
        Assert.Contains("well-formed", result.TopError.Description);
    }

    [Theory]
    [InlineData("""{ "Temperature": 10, "Humidity": 50 }""", "Location")]
    [InlineData("""{ "Location": "X", "Humidity": 50 }""", "Temperature")]
    [InlineData("""{ "Location": "X", "Temperature": 10 }""", "Humidity")]
    public void Parse_Missing_Field_Returns_Error_With_Field_Name(string json, string field)
    {
        var result = _parser.Parse(json);

        Assert.True(result.IsError);
        Assert.Contains(field, result.TopError.Description);
    }
}
