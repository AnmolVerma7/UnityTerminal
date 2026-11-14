/// <summary>
/// Clears terminal history.
/// </summary>
public class ClearCommand : ITerminalCommand
{
    public string Name => "clear";
    public string Usage => "clear";
    public string Description => "Clear terminal output";

    public CommandResult Execute(ParsedCommand command)
    {
        return CommandResult.Clear();
    }
}
