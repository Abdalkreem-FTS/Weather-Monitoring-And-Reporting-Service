using NSubstitute;
using WMARS.Parsing;

namespace WMARS.Tests.Parsing;

public class WeatherDataParserResolverTests
{
    private readonly WeatherDataParserResolver _resolver =
        new([new JsonWeatherDataParser(), new XmlWeatherDataParser()]);

    [Theory]
    [InlineData("json")]
    [InlineData("JSON")]
    [InlineData("xml")]
    [InlineData("XML")]
    public void TryResolve_Known_Format_Is_Case_Insensitive(string format)
    {
        Assert.True(_resolver.TryResolve(format, out var parser));
        Assert.NotNull(parser);
    }

    [Fact]
    public void TryResolve_Unknown_Format_Returns_False_And_Null()
    {
        Assert.False(_resolver.TryResolve("csv", out var parser));
        Assert.Null(parser);
    }

    [Fact]
    public void TryResolve_Selects_The_Parser_That_Supports_The_Format()
    {
        _resolver.TryResolve("xml", out var parser);
        Assert.IsType<XmlWeatherDataParser>(parser);
    }

    [Fact]
    public void SupportedFormats_Aggregates_All_Parsers_And_Deduplicates_Case_Insensitively()
    {
        var first = Substitute.For<IWeatherDataParser>();
        first.SupportedFormats.Returns(["json", "geojson"]);
        var second = Substitute.For<IWeatherDataParser>();
        second.SupportedFormats.Returns(["JSON", "xml"]);
        var resolver = new WeatherDataParserResolver([first, second]);

        var formats = resolver.SupportedFormats.ToList();

        Assert.Equal(3, formats.Count);
        Assert.Contains("geojson", formats);
        Assert.Contains("xml", formats);
    }
}
