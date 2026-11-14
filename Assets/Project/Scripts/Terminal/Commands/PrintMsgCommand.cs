/// <summary>
/// Prints all welcome messages from the terminal profile.
/// </summary>
public class PrintMsgCommand : ITerminalCommand
{
    private readonly TerminalProfile profile;

    public string Name => "printmsg";
    public string Usage => "printmsg";
    public string Description => "Print welcome messages";

    public PrintMsgCommand(TerminalProfile profile)
    {
        this.profile = profile;
    }

    public CommandResult Execute(ParsedCommand command)
    {
        if (profile == null || profile.welcomeLines == null || profile.welcomeLines.Length == 0)
            return CommandResult.Empty();

        return CommandResult.Lines(profile.welcomeLines);
    }
}
