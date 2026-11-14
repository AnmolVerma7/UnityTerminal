/// <summary>
/// Parsed representation of one submitted terminal command.
/// </summary>
public class ParsedCommand
{
    public string Name { get; }
    public string Args { get; }

    public ParsedCommand(string name, string args)
    {
        Name = name;
        Args = args;
    }

    /// <summary>Split raw input into a command name and argument string.</summary>
    public static ParsedCommand Parse(string input)
    {
        string safeInput = input ?? string.Empty;
        int space = safeInput.IndexOf(' ');

        return space < 0
            ? new ParsedCommand(safeInput, string.Empty)
            : new ParsedCommand(safeInput.Substring(0, space), safeInput.Substring(space + 1));
    }
}
