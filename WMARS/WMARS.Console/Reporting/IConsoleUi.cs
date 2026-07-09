using WMARS.Models;

namespace WMARS.Reporting;

/// <summary>
/// Console surface used by the application shell to greet the user, read input
/// and present results. Implementations own all presentation concerns.
/// </summary>
public interface IConsoleUi
{
    void ShowWelcome(IReadOnlyCollection<string> supportedFormats);

    /// <summary>Prompts for and reads a line of input; null signals end-of-input.</summary>
    string? ReadPath();

    void ShowReceived(WeatherData data);
    void ShowError(string message);
    void ShowGoodbye();
}
