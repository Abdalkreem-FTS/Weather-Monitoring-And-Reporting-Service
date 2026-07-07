using WMARS.Models;

namespace WMARS.Observers;

/// <summary>
/// Concrete subject that keeps a list of observers and broadcasts each published <see cref="WeatherData"/> reading to all of them.
/// </summary>
public sealed class WeatherStation : IWeatherSubject
{
    private readonly List<IWeatherObserver> _observers = [];

    public void Subscribe(IWeatherObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Unsubscribe(IWeatherObserver observer) => _observers.Remove(observer);

    public void Publish(WeatherData data) => _observers.ForEach(observer => observer.OnWeatherUpdate(data));
}
