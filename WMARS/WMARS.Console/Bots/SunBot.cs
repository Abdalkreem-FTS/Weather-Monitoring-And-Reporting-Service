using WMARS.Models;
using WMARS.Reporting;

namespace WMARS.Bots;

/// <summary>Activates when the temperature rises above its configured threshold.</summary>
public sealed class SunBot(bool enabled, double temperatureThreshold, string message, IActivationReporter reporter)
    : WeatherBot("SunBot", enabled, temperatureThreshold, message, reporter)
{
    protected override bool ShouldActivate(WeatherData data) => data.Temperature > Threshold;
}
