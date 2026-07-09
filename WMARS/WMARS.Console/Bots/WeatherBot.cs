using WMARS.Models;
using WMARS.Observers;
using WMARS.Reporting;

namespace WMARS.Bots;

/// <summary>
/// Base class for all weather bots (Template Method pattern). The activation
/// flow is fixed here; each concrete bot only decides <see cref="ShouldActivate"/>.
/// Adding a new bot means adding a subclass - no existing code changes OCP
/// </summary>
public abstract class WeatherBot(
    string name,
    bool enabled,
    double threshold,
    string message,
    IActivationReporter reporter) : IWeatherObserver
{
    private string Name { get; } = name;
    private bool Enabled { get; } = enabled;
    protected double Threshold { get; } = threshold;
    private string Message { get; } = message;

    public void OnWeatherUpdate(WeatherData data)
    {
        if (!Enabled)
        {
            return;
        }

        if (ShouldActivate(data))
        {
            Activate();
        }
    }

    /// <summary>Bot-specific activation condition.</summary>
    protected abstract bool ShouldActivate(WeatherData data);

    /// <summary>Common activation behavior: announce the activation and message.</summary>
    private void Activate() => reporter.ReportActivation(Name, Message);
}
