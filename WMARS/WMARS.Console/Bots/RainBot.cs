using WMARS.Models;
using WMARS.Reporting;

namespace WMARS.Bots;

/// <summary>Activates when the humidity exceeds its configured threshold.</summary>
public sealed class RainBot(bool enabled, double humidityThreshold, string message, IActivationReporter reporter)
    : WeatherBot("RainBot", enabled, humidityThreshold, message, reporter)
{
    protected override bool ShouldActivate(WeatherData data) => data.Humidity > Threshold;
}
