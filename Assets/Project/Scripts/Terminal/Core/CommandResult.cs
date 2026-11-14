using System.Collections.Generic;

/// <summary>
/// Result returned by a terminal command after it runs.
/// </summary>
public class CommandResult
{
    public List<string> OutputLines { get; }
    public bool ShouldClearScreen { get; }

    public CommandResult(List<string> outputLines, bool shouldClearScreen = false)
    {
        OutputLines = outputLines ?? new List<string>();
        ShouldClearScreen = shouldClearScreen;
    }

    /// <summary>Create a result that prints no output.</summary>
    public static CommandResult Empty() => new CommandResult(new List<string>());

    /// <summary>Create a result that prints the provided output lines.</summary>
    public static CommandResult Lines(params string[] lines)
    {
        return new CommandResult(new List<string>(lines ?? new string[0]));
    }

    /// <summary>Create a result that clears the terminal history.</summary>
    public static CommandResult Clear() => new CommandResult(new List<string>(), true);
}
