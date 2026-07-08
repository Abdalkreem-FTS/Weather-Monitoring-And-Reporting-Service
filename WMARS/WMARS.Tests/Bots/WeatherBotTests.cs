using Moq;
using WMARS.Bots;
using WMARS.Models;
using WMARS.Reporting;

namespace WMARS.Tests.Bots;

public class WeatherBotTests
{
    private readonly Mock<IActivationReporter> _reporter = new();

    private static WeatherData Weather(double temperature, double humidity) => new("City", temperature, humidity);

    [Theory]
    [InlineData(80, true)]   // above threshold
    [InlineData(70, false)]  // exactly at threshold -> strict '>' means no
    [InlineData(60, false)]  // below threshold
    public void RainBot_Activates_Only_When_Humidity_Exceeds_Threshold(double humidity, bool expected)
    {
        var bot = new RainBot(enabled: true, humidityThreshold: 70, message: "rain!", _reporter.Object);

        bot.OnWeatherUpdate(Weather(temperature: 20, humidity));

        _reporter.Verify(r => r.ReportActivation("RainBot", "rain!"), Times.Exactly(expected ? 1 : 0));
    }

    [Theory]
    [InlineData(35, true)]
    [InlineData(30, false)]
    [InlineData(25, false)]
    public void SunBot_Activates_Only_When_Temperature_Exceeds_Threshold(double temperature, bool expected)
    {
        var bot = new SunBot(enabled: true, temperatureThreshold: 30, message: "hot!", _reporter.Object);

        bot.OnWeatherUpdate(Weather(temperature, humidity: 40));

        _reporter.Verify(r => r.ReportActivation("SunBot", "hot!"), Times.Exactly(expected ? 1 : 0));
    }

    [Theory]
    [InlineData(-5, true)]
    [InlineData(0, false)]
    [InlineData(5, false)]
    public void SnowBot_Activates_Only_When_Temperature_Below_Threshold(double temperature, bool expected)
    {
        var bot = new SnowBot(enabled: true, temperatureThreshold: 0, message: "cold!", _reporter.Object);

        bot.OnWeatherUpdate(Weather(temperature, humidity: 40));

        _reporter.Verify(r => r.ReportActivation("SnowBot", "cold!"), Times.Exactly(expected ? 1 : 0));
    }

    [Fact]
    public void Disabled_Bot_Never_Activates()
    {
        var bot = new SunBot(enabled: false, temperatureThreshold: 30, message: "hot!", _reporter.Object);

        bot.OnWeatherUpdate(Weather(temperature: 45, humidity: 10));

        _reporter.Verify(r => r.ReportActivation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Activation_Reports_The_Bot_Name_And_Configured_Message()
    {
        var bot = new RainBot(enabled: true, humidityThreshold: 70, message: "It's pouring!", _reporter.Object);

        bot.OnWeatherUpdate(Weather(temperature: 20, humidity: 90));

        _reporter.Verify(r => r.ReportActivation("RainBot", "It's pouring!"), Times.Once);
    }
}
