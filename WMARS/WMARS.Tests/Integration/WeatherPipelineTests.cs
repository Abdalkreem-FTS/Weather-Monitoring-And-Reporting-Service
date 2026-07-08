using Moq;
using WMARS.App;
using WMARS.Bots;
using WMARS.Models;
using WMARS.Observers;
using WMARS.Parsing;
using WMARS.Reporting;

namespace WMARS.Tests.Integration;

/// <summary>
/// End-to-end tests for the whole read-parse-publish-react pipeline. Unlike the
/// unit tests, these wire the <em>real</em> parsers, resolver, station and bots
/// together and drive <see cref="WeatherMonitorApp"/> through a fake console,
/// feeding it real files on disk. Only the two edges are doubled: input/output
/// (<see cref="FakeConsoleUi"/>) and the bot activation sink (a mock reporter).
/// </summary>
public sealed class WeatherPipelineTests : IDisposable
{
    private readonly List<string> _tempFiles = [];

    [Fact]
    public void Valid_Xml_File_Flows_Through_Pipeline_And_Activates_Rain_Bot()
    {
        var path = CreateTempFile("xml",
            "<WeatherData><Location>London</Location><Temperature>12</Temperature><Humidity>85</Humidity></WeatherData>");

        var (reporter, ui) = RunPipeline(path);

        reporter.Verify(r => r.ReportActivation("RainBot", "rain!"), Times.Once);
        reporter.Verify(r => r.ReportActivation("SunBot", It.IsAny<string>()), Times.Never);
        reporter.Verify(r => r.ReportActivation("SnowBot", It.IsAny<string>()), Times.Never);

        Assert.Empty(ui.Errors);
        var reading = Assert.Single(ui.Received);
        Assert.Equal(new WeatherData("London", 12, 85), reading);
    }

    [Fact]
    public void Valid_Json_File_Flows_Through_Pipeline_And_Activates_Sun_Bot()
    {
        var path = CreateTempFile("json", """{ "Location": "Nablus", "Temperature": 33, "Humidity": 40 }""");

        var (reporter, ui) = RunPipeline(path);

        reporter.Verify(r => r.ReportActivation("SunBot", "sun!"), Times.Once);
        reporter.Verify(r => r.ReportActivation("RainBot", It.IsAny<string>()), Times.Never);

        Assert.Empty(ui.Errors);
        Assert.Single(ui.Received);
    }

    [Fact]
    public void Multiple_Files_In_One_Session_Each_Trigger_Their_Own_Bot()
    {
        var xml = CreateTempFile("xml",
            "<WeatherData><Location>London</Location><Temperature>12</Temperature><Humidity>85</Humidity></WeatherData>");
        var json = CreateTempFile("json", """{ "Location": "Nablus", "Temperature": 33, "Humidity": 40 }""");

        var (reporter, ui) = RunPipeline(xml, json);

        reporter.Verify(r => r.ReportActivation("RainBot", "rain!"), Times.Once);
        reporter.Verify(r => r.ReportActivation("SunBot", "sun!"), Times.Once);
        Assert.Equal(2, ui.Received.Count);
        Assert.Empty(ui.Errors);
    }

    [Fact]
    public void Nonexistent_File_Surfaces_Error_And_Activates_No_Bot()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        var (reporter, ui) = RunPipeline(missing);

        var error = Assert.Single(ui.Errors);
        Assert.Contains("not found", error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(ui.Received);
        reporter.Verify(r => r.ReportActivation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Malformed_File_Surfaces_Parser_Error_And_Activates_No_Bot()
    {
        var path = CreateTempFile("json", "{ not valid json");

        var (reporter, ui) = RunPipeline(path);

        var error = Assert.Single(ui.Errors);
        Assert.Contains("well-formed", error);
        Assert.Empty(ui.Received);
        reporter.Verify(r => r.ReportActivation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void A_Bad_Input_Does_Not_Stop_The_Session_From_Processing_Later_Ones()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        var good = CreateTempFile("json", """{ "Location": "Nablus", "Temperature": 33, "Humidity": 40 }""");

        var (reporter, ui) = RunPipeline(missing, good);

        Assert.Single(ui.Errors);
        Assert.Single(ui.Received);
        reporter.Verify(r => r.ReportActivation("SunBot", "sun!"), Times.Once);
    }

    /// <summary>
    /// Builds the real object graph the application uses and runs it through the
    /// given sequence of console inputs. Bot thresholds mirror the shipped
    /// bots.json defaults so the assertions read like the real configuration.
    /// </summary>
    private static (Mock<IActivationReporter> Reporter, FakeConsoleUi Ui) RunPipeline(params string?[] inputs)
    {
        var reporter = new Mock<IActivationReporter>();

        WeatherBot[] bots =
        [
            new RainBot(enabled: true, humidityThreshold: 70, message: "rain!", reporter.Object),
            new SunBot(enabled: true, temperatureThreshold: 30, message: "sun!", reporter.Object),
            new SnowBot(enabled: true, temperatureThreshold: 0, message: "snow!", reporter.Object),
        ];

        var station = new WeatherStation();
        var resolver = new WeatherDataParserResolver([new JsonWeatherDataParser(), new XmlWeatherDataParser()]);
        var ui = new FakeConsoleUi(inputs);

        var app = new WeatherMonitorApp(station, bots, resolver, ui);
        app.Run();

        return (reporter, ui);
    }

    private string CreateTempFile(string extension, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"weather-{Guid.NewGuid():N}.{extension}");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles.Where(File.Exists))
        {
            File.Delete(file);
        }
    }

    /// <summary>
    /// Drives <see cref="WeatherMonitorApp"/> without a real console: replays a
    /// fixed sequence of inputs (null ends the session) and records everything
    /// the app tries to display so tests can assert on it.
    /// </summary>
    private sealed class FakeConsoleUi(IEnumerable<string?> inputs) : IConsoleUi
    {
        private readonly Queue<string?> _inputs = new(inputs);

        public List<WeatherData> Received { get; } = [];
        public List<string> Errors { get; } = [];

        public void ShowWelcome(IReadOnlyCollection<string> supportedFormats)
        {
        }

        public string? ReadPath() => _inputs.Count > 0 ? _inputs.Dequeue() : null;

        public void ShowReceived(WeatherData data) => Received.Add(data);

        public void ShowError(string message) => Errors.Add(message);

        public void ShowGoodbye()
        {
        }
    }
}
