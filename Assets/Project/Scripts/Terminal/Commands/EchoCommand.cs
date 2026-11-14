/// <summary>
/// Prints the text provided after the command name.
/// </summary>
public class EchoCommand : ITerminalCommand
{
    public string Name => "echo";
    public string Usage => "echo <text>";
    public string Description => "Print text";

    public CommandResult Execute(ParsedCommand command)
    {
        return CommandResult.Lines(command.Args);
    }
}
