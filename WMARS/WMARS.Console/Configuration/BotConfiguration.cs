namespace WMARS.Configuration;

/// <summary>
/// Configuration for a single bot, mirroring the bots.json schema. Both
/// threshold keys are nullable because each bot reads only the one relevant
/// to it (RainBot: humidity, Sun/SnowBot: temperature).
/// </summary>
public sealed class BotConfiguration
{
    public bool Enabled { get; set; }
    public double? HumidityThreshold { get; set; }
    public double? TemperatureThreshold { get; set; }
    public string Message { get; set; } = string.Empty;
}
