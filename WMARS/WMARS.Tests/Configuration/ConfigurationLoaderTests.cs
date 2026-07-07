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
            var config = await ConfigurationLoader.Load(path);

            Assert.True(config["RainBot"].Enabled);
            Assert.Equal(70d, config["RainBot"].HumidityThreshold);
            Assert.Equal("pour", config["RainBot"].Message);

            Assert.False(config["SunBot"].Enabled);
            Assert.Equal(30d, config["SunBot"].TemperatureThreshold);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Missing_File_Throws_FileNotFoundException()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        await Assert.ThrowsAsync<FileNotFoundException>(() => ConfigurationLoader.Load(path));
    }

    [Fact]
    public async Task Load_Invalid_Json_Throws_FormatException()
    {
        var path = await WriteTempAsync("{ not valid json");
        try
        {
            await Assert.ThrowsAsync<FormatException>(() => ConfigurationLoader.Load(path));
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
