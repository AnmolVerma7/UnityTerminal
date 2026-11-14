/// <summary>
/// Contract for one executable terminal command.
/// </summary>
public interface ITerminalCommand
{
    string Name { get; }
    string Usage { get; }
    string Description { get; }

    /// <summary>Run the command and return output for the controller to display.</summary>
    CommandResult Execute(ParsedCommand command);
}
