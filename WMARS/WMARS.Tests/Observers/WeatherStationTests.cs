using NSubstitute;
using WMARS.Models;
using WMARS.Observers;

namespace WMARS.Tests.Observers;

public class WeatherStationTests
{
    private static readonly WeatherData Sample = new("City", 20, 50);

    private readonly WeatherStation _station = new();

    [Fact]
    public void Publish_Notifies_All_Subscribed_Observers()
    {
        var first = Substitute.For<IWeatherObserver>();
        var second = Substitute.For<IWeatherObserver>();
        _station.Subscribe(first);
        _station.Subscribe(second);

        _station.Publish(Sample);

        first.Received(1).OnWeatherUpdate(Sample);
        second.Received(1).OnWeatherUpdate(Sample);
    }

    [Fact]
    public void Unsubscribe_Stops_Further_Notifications()
    {
        var observer = Substitute.For<IWeatherObserver>();
        _station.Subscribe(observer);
        _station.Unsubscribe(observer);

        _station.Publish(Sample);

        observer.DidNotReceive().OnWeatherUpdate(Arg.Any<WeatherData>());
    }

    [Fact]
    public void Subscribing_The_Same_Observer_Twice_Notifies_It_Once()
    {
        var observer = Substitute.For<IWeatherObserver>();
        _station.Subscribe(observer);
        _station.Subscribe(observer);

        _station.Publish(Sample);

        observer.Received(1).OnWeatherUpdate(Sample);
    }
}
