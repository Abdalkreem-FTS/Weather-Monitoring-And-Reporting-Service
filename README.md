# Weather Monitoring And Reporting Service

A console-based real-time weather monitoring service with a clean and interactive terminal UI powered by Spectre.Console. It reads raw weather data in multiple formats (JSON and XML) and triggers configurable "weather bots" that react to each reading.

## Features

* Read weather data from JSON and XML files
* Automatic format detection by file extension — extensible: add a new parser to support more formats
* Configurable weather bots that react to each reading:
  * **RainBot** — activates when humidity exceeds a threshold
  * **SunBot** — activates when temperature rises above a threshold
  * **SnowBot** — activates when temperature drops below a threshold
* Per-bot settings (enabled, threshold, message) controlled from a JSON config file
* Clean, interactive terminal UI powered by Spectre.Console

## Prerequisites

* Git
* .NET 10 SDK

## Run

```bash
git clone https://github.com/Abdalkreem-FTS/Weather-Monitoring-And-Reporting-Service
cd Weather-Monitoring-And-Reporting-Service
dotnet run --project WMARS/WMARS.Console
```

## Usage

When prompted, enter the path to a weather data file (`.json` or `.xml`), or type `exit` to quit. Sample files are included:

```
samples/nablus.json
samples/london.xml
```

## Configuration

Bot behavior is controlled by `WMARS/WMARS.Console/bots.json`:

```json
{
  "RainBot": { "enabled": true, "humidityThreshold": 70, "message": "It looks like it's about to pour down!" },
  "SunBot": { "enabled": true, "temperatureThreshold": 30, "message": "Wow, it's a scorcher out there!" },
  "SnowBot": { "enabled": false, "temperatureThreshold": 0, "message": "Brrr, it's getting chilly!" }
}
```

## Tests

```bash
dotnet test
```
