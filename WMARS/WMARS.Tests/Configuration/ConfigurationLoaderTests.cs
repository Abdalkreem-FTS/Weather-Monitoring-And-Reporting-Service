using WMARS.Configuration;

namespace WMARS.Tests.Configuration;

public class ConfigurationLoaderTests
{
    [Fact]
    public async Task Load_Reads_Bot_Settings_From_File()
    {
        const string json =
            "{ \"RainBot\": { \"enabled\": true, \"humidityThreshold\": 70, \"message\": \"pour\" }, " +
            "\"SunBot\": { \"enabled\": false, \"temperatureThreshold\": 30, \"message\": \"hot\" } }";

        var path = await WriteTempAsync(json);
        try
        {
            var result = await ConfigurationLoader.Load(path);

            result.IsSuccess.Should().BeTrue();

            var config = result.Value;
            config["RainBot"].Enabled.Should().BeTrue();
            config["RainBot"].HumidityThreshold.Should().Be(70d);
            config["RainBot"].Message.Should().Be("pour");

            config["SunBot"].Enabled.Should().BeFalse();
            config["SunBot"].TemperatureThreshold.Should().Be(30d);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Missing_File_Returns_NotFound_Error()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        var result = await ConfigurationLoader.Load(path);

        result.IsError.Should().BeTrue();
        result.TopError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Load_Invalid_Json_Returns_Validation_Error()
    {
        var path = await WriteTempAsync("{ not valid json");
        try
        {
            var result = await ConfigurationLoader.Load(path);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorType.Validation);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task<string> WriteTempAsync(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"bots-{Guid.NewGuid():N}.json");

        await File.WriteAllTextAsync(path, content);

        return path;
    }
}
