/// <summary>
/// Prints the current working directory from the shell session.
/// </summary>
public class PwdCommand : ITerminalCommand
{
    private readonly ShellSession session;

    public string Name => "pwd";
    public string Usage => "pwd";
    public string Description => "Print current working directory";

    public PwdCommand(ShellSession session)
    {
        this.session = session;
    }

    public CommandResult Execute(ParsedCommand command)
    {
        return CommandResult.Lines(session.Path);
    }
}
