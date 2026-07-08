using Moq;
using WMARS.Parsing;

namespace WMARS.Tests.Parsing;

public class WeatherDataParserResolverTests
{
    private readonly WeatherDataParserResolver _resolver = new([new JsonWeatherDataParser(), new XmlWeatherDataParser()]);

    [Theory]
    [InlineData("json")]
    [InlineData("JSON")]
    [InlineData("xml")]
    [InlineData("XML")]
    public void Resolve_Known_Format_Is_Case_Insensitive(string format)
    {
        var result = _resolver.Resolve(format);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public void Resolve_Unknown_Format_Returns_NotFound_Error()
    {
        var result = _resolver.Resolve("csv");

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.TopError.Type);
    }

    [Fact]
    public void Resolve_Selects_The_Parser_That_Supports_The_Format()
    {
        var result = _resolver.Resolve("xml");

        Assert.True(result.IsSuccess);
        Assert.IsType<XmlWeatherDataParser>(result.Value);
    }

    [Fact]
    public void SupportedFormats_Aggregates_All_Parsers_And_Deduplicates_Case_Insensitively()
    {
        var first = new Mock<IWeatherDataParser>();
        first.Setup(p => p.SupportedFormats).Returns(["json", "geojson"]);
        var second = new Mock<IWeatherDataParser>();
        second.Setup(p => p.SupportedFormats).Returns(["JSON", "xml"]);
        var resolver = new WeatherDataParserResolver([first.Object, second.Object]);

        var formats = resolver.SupportedFormats.ToList();

        Assert.Equal(3, formats.Count);
        Assert.Contains("geojson", formats);
        Assert.Contains("xml", formats);
    }
}
