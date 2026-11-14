using System;
/// <summary>
/// Prints the current date and time
/// </summary>
public class DateCommand : ITerminalCommand
{
    public string Name => "date";
    public string Usage => "date";
    public string Description => "Print current date and time";

    public CommandResult Execute(ParsedCommand command)
    {
        string dayOfWeek = DateTime.Now.DayOfWeek.ToString();
        string formattedTime = DateTime.Now.ToString("MMMM d yyyy h:mm:ss");
        string amPm = DateTime.Now.ToString("tt").ToLower();

        return CommandResult.Lines($"{dayOfWeek} {formattedTime}{amPm}");
    }
}
