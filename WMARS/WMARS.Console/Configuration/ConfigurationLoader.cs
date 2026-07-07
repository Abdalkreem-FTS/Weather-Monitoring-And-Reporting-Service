using System.Text.Json;

namespace WMARS.Configuration;

/// <summary>
/// Reads the bots.json configuration file into a map of bot name to <see cref="BotConfiguration"/>.
/// </summary>
public static class ConfigurationLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<IReadOnlyDictionary<string, BotConfiguration>> Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Configuration file not found: {path}");
        }

        var json = await File.ReadAllTextAsync(path);

        Dictionary<string, BotConfiguration>? configuration;
        try
        {
            configuration = JsonSerializer.Deserialize<Dictionary<string, BotConfiguration>>(json, Options);
        }
        catch (JsonException ex)
        {
            throw new FormatException("The configuration file is not valid JSON.", ex);
        }

        return configuration ?? throw new FormatException("The configuration file is empty.");
    }
}
