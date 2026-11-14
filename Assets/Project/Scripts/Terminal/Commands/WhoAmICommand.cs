/// <summary>
/// Prints the current terminal username from the shell session.
/// </summary>
public class WhoAmICommand : ITerminalCommand
{
    private readonly ShellSession session;

    public string Name => "whoami";
    public string Usage => "whoami";
    public string Description => "Print current user";

    public WhoAmICommand(ShellSession session)
    {
        this.session = session;
    }

    public CommandResult Execute(ParsedCommand command)
    {
        return CommandResult.Lines(session.Username);
    }
}
