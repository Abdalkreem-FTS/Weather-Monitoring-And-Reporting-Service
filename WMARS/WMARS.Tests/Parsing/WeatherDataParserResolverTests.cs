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

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Resolve_Unknown_Format_Returns_NotFound_Error()
    {
        var result = _resolver.Resolve("csv");

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Resolve_Selects_The_Parser_That_Supports_The_Format()
    {
        var result = _resolver.Resolve("xml");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<XmlWeatherDataParser>();
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

        formats.Should().HaveCount(3);
        formats.Should().Contain("geojson");
        formats.Should().Contain("xml");
    }
}
