using System.Collections.Generic;

/// <summary>
/// Lists all commands currently registered in the terminal.
/// </summary>
public class HelpCommand : ITerminalCommand
{
    private readonly CommandRegistry registry;

    public string Name => "help";
    public string Usage => "help";
    public string Description => "Show available commands";

    public HelpCommand(CommandRegistry registry)
    {
        this.registry = registry;
    }

    public CommandResult Execute(ParsedCommand command)
    {
        List<string> lines = new List<string> { "Available commands:" };

        foreach (ITerminalCommand registeredCommand in registry.Commands)
            lines.Add($"  {registeredCommand.Usage,-11} {registeredCommand.Description}");

        return new CommandResult(lines);
    }
}
