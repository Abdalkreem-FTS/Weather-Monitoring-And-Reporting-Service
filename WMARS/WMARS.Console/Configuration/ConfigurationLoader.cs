using System.Text.Json;
using WMARS.Results;

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

    public static async Task<Result<IReadOnlyDictionary<string, BotConfiguration>>> Load(string path)
    {
        if (!File.Exists(path))
        {
            return Error.NotFound("Configuration.NotFound", $"Configuration file not found: {path}");
        }

        string json;
        try
        {
            json = await File.ReadAllTextAsync(path);
        }
        catch (IOException ex)
        {
            return Error.Failure("Configuration.ReadError", $"Could not read the configuration file: {ex.Message}");
        }

        Dictionary<string, BotConfiguration>? configuration;
        try
        {
            configuration = JsonSerializer.Deserialize<Dictionary<string, BotConfiguration>>(json, Options);
        }
        catch (JsonException)
        {
            return Error.Validation("Configuration.InvalidJson", "The configuration file is not valid JSON.");
        }

        if (configuration is null)
        {
            return Error.Validation("Configuration.Empty", "The configuration file is empty.");
        }

        return Result<IReadOnlyDictionary<string, BotConfiguration>>.From(configuration);
    }
}
