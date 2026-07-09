using WMARS.Models;

namespace WMARS.Observers;

/// <summary>
/// Observer that reacts to weather updates broadcast by a weather station.
/// </summary>
public interface IWeatherObserver
{
    void OnWeatherUpdate(WeatherData data);
}
