using WMARS.Models;

namespace WMARS.Observers;

/// <summary>
/// Subject in the Observer pattern: weather bots subscribe here and are notified whenever a new reading is published.
/// </summary>
public interface IWeatherSubject
{
    void Subscribe(IWeatherObserver observer);
    void Unsubscribe(IWeatherObserver observer);
    void Publish(WeatherData data);
}
