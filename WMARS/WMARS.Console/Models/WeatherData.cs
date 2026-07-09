namespace WMARS.Models;

/// <summary>
/// Immutable weather reading for a single location, produced by a parser and broadcast to the weather bots.
/// </summary>
public record WeatherData(string Location, double Temperature, double Humidity);
