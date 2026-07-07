using WMARS.Models;
using WMARS.Reporting;

namespace WMARS.Bots;

/// <summary>Activates when the temperature drops below its configured threshold.</summary>
public sealed class SnowBot(bool enabled, double temperatureThreshold, string message, IActivationReporter reporter)
    : WeatherBot("SnowBot", enabled, temperatureThreshold, message, reporter)
{
    protected override bool ShouldActivate(WeatherData data) => data.Temperature < Threshold;
}
