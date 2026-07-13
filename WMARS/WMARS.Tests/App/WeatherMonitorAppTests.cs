using Moq;
using WMARS.App;
using WMARS.Models;
using WMARS.Observers;
using WMARS.Parsing;
using WMARS.Reporting;

namespace WMARS.Tests.App;

public sealed class WeatherMonitorAppTests : IDisposable
{
    private readonly List<string> _tempFiles = [];

    private readonly WeatherDataParserResolver _resolver =
        new([new JsonWeatherDataParser(), new XmlWeatherDataParser()]);

    [Fact]
    public void Run_Shows_Welcome_And_Goodbye_Around_The_Session()
    {
        var ui = new Mock<IConsoleUi>();
        ui.Setup(u => u.ReadPath()).Returns((string?)null);

        RunApp(ui.Object);

        ui.Verify(u => u.ShowWelcome(It.IsAny<IReadOnlyCollection<string>>()), Times.Once);
        ui.Verify(u => u.ShowGoodbye(), Times.Once);
    }

    [Fact]
    public void Typing_Exit_Ends_Session_Without_Treating_It_As_A_File()
    {
        var ui = new Mock<IConsoleUi>();
        ui.SetupSequence(u => u.ReadPath()).Returns("exit");

        RunApp(ui.Object);

        ui.Verify(u => u.ShowError(It.IsAny<string>()), Times.Never);
        ui.Verify(u => u.ShowReceived(It.IsAny<WeatherData>()), Times.Never);
        ui.Verify(u => u.ShowGoodbye(), Times.Once);
    }

    [Fact]
    public void Blank_Input_Is_Skipped_Without_Error()
    {
        var ui = new Mock<IConsoleUi>();
        ui.SetupSequence(u => u.ReadPath()).Returns("   ").Returns((string?)null);

        RunApp(ui.Object);

        ui.Verify(u => u.ShowError(It.IsAny<string>()), Times.Never);
        ui.Verify(u => u.ShowReceived(It.IsAny<WeatherData>()), Times.Never);
    }

    [Fact]
    public void Valid_File_Is_Received_And_Published()
    {
        var path = CreateTempFile("json", """{ "Location": "Amman", "Temperature": 25, "Humidity": 40 }""");

        var ui = ReadOnce(path);
        RunApp(ui.Object);

        ui.Verify(u => u.ShowReceived(new WeatherData("Amman", 25, 40)), Times.Once);
        ui.Verify(u => u.ShowError(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Unreadable_File_Reports_An_Error_Instead_Of_Crashing()
    {
        var path = CreateTempFile("json", """{ "Location": "Amman", "Temperature": 25, "Humidity": 40 }""");
        using var _ = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);

        var ui = ReadOnce(path);
        RunApp(ui.Object);

        ui.Verify(u => u.ShowError(It.IsAny<string>()), Times.Once);
        ui.Verify(u => u.ShowReceived(It.IsAny<WeatherData>()), Times.Never);
    }

    private static Mock<IConsoleUi> ReadOnce(string path)
    {
        var ui = new Mock<IConsoleUi>();
        ui.SetupSequence(u => u.ReadPath()).Returns(path).Returns((string?)null);
        return ui;
    }

    private void RunApp(IConsoleUi ui)
    {
        var app = new WeatherMonitorApp(new WeatherStation(), [], _resolver, ui);
        app.Run();
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
}
