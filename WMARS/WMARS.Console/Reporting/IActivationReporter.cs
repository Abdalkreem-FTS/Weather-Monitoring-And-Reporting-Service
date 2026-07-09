namespace WMARS.Reporting;

/// <summary>
/// Narrow surface a bot uses to announce that it activated. Keeping this
/// separate from the wider console UI means bots depend only on what they use
/// (ISP) and stay decoupled from any specific console library (DIP).
/// </summary>
public interface IActivationReporter
{
    void ReportActivation(string botName, string message);
}
