using Moq;
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
        var first = new Mock<IWeatherObserver>();
        var second = new Mock<IWeatherObserver>();
        _station.Subscribe(first.Object);
        _station.Subscribe(second.Object);

        _station.Publish(Sample);

        first.Verify(o => o.OnWeatherUpdate(Sample), Times.Once);
        second.Verify(o => o.OnWeatherUpdate(Sample), Times.Once);
    }

    [Fact]
    public void Unsubscribe_Stops_Further_Notifications()
    {
        var observer = new Mock<IWeatherObserver>();
        _station.Subscribe(observer.Object);
        _station.Unsubscribe(observer.Object);

        _station.Publish(Sample);

        observer.Verify(o => o.OnWeatherUpdate(It.IsAny<WeatherData>()), Times.Never);
    }

    [Fact]
    public void Subscribing_The_Same_Observer_Twice_Notifies_It_Once()
    {
        var observer = new Mock<IWeatherObserver>();
        _station.Subscribe(observer.Object);
        _station.Subscribe(observer.Object);

        _station.Publish(Sample);

        observer.Verify(o => o.OnWeatherUpdate(Sample), Times.Once);
    }
}
