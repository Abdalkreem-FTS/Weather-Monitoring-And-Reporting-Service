using Spectre.Console;
using WMARS.Models;

namespace WMARS.Reporting;

/// <summary>
/// Spectre.Console implementation of both the application shell (<see cref="IConsoleUi"/>)
/// and the bots' activation reporter (<see cref="IActivationReporter"/>). All
/// styling lives here, so the rest of the application never references Spectre.
/// </summary>
public sealed class SpectreConsoleUi : IConsoleUi, IActivationReporter
{
    private const string Accent = "#d97757";
    private static readonly Color AccentColor = new(217, 119, 87);

    public void ShowWelcome(IReadOnlyCollection<string> supportedFormats)
    {
        var formats = string.Join("  ", supportedFormats.Select(f => $"[{Accent}].{f}[/]"));

        AnsiConsole.Write(
            new Panel(new Markup(
                    "[bold]Weather Monitoring & Reporting Service[/]\n" +
                    "[grey]Real-time weather bots reacting to your data[/]"))
                .Border(BoxBorder.Rounded)
                .BorderColor(AccentColor)
                .Padding(2, 1, 2, 1));

        AnsiConsole.MarkupLine($"  [grey]Supported formats[/]   {formats}");
        AnsiConsole.MarkupLine($"  [grey]Type a file path, or[/] [{Accent}]exit[/] [grey]to quit[/]");
    }

    public string? ReadPath()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Markup($"[{Accent}]❯[/] ");
        return Console.ReadLine();
    }

    public void ShowReceived(WeatherData data)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[grey]Location[/]")
            .AddColumn("[grey]Temperature[/]")
            .AddColumn("[grey]Humidity[/]");

        table.AddRow(
            $"[bold]{Markup.Escape(data.Location)}[/]",
            $"{data.Temperature} [grey]°C[/]",
            $"{data.Humidity} [grey]%[/]");

        AnsiConsole.Write(table);
    }

    public void ReportActivation(string botName, string message)
    {
        var color = botName switch
        {
            "RainBot" => "deepskyblue1",
            "SunBot" => "gold1",
            "SnowBot" => "aqua",
            _ => Accent
        };

        AnsiConsole.MarkupLine($"[{color}]●[/] [bold {color}]{Markup.Escape(botName)} activated![/]");
        AnsiConsole.MarkupLine($"   [italic grey]\"{Markup.Escape(message)}\"[/]");
    }

    public void ShowError(string message) =>
        AnsiConsole.MarkupLine($"[red]✗[/] [red]{Markup.Escape(message)}[/]");

    public void ShowGoodbye()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Accent}]Goodbye![/] [grey]Stay weather-aware.[/]");
    }
}
