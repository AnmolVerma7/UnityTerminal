using System;
using System.Collections.Generic;

/// <summary>
/// Stores available terminal commands and executes the one matching user input.
/// </summary>
public class CommandRegistry
{
    private readonly Dictionary<string, ITerminalCommand> commands =
        new Dictionary<string, ITerminalCommand>(StringComparer.OrdinalIgnoreCase);
    private readonly List<ITerminalCommand> commandOrder = new List<ITerminalCommand>();

    public IEnumerable<ITerminalCommand> Commands => commandOrder;

    /// <summary>Add a command to the registry.</summary>
    public void Register(ITerminalCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.Name))
            return;

        if (commands.TryGetValue(command.Name, out ITerminalCommand existingCommand))
        {
            int index = commandOrder.IndexOf(existingCommand);
            if (index >= 0)
                commandOrder[index] = command;
        }
        else
        {
            commandOrder.Add(command);
        }

        commands[command.Name] = command;
    }

    /// <summary>Parse input and execute the matching command.</summary>
    public bool TryExecute(string input, out CommandResult result)
    {
        ParsedCommand parsedCommand = ParsedCommand.Parse(input);
        if (parsedCommand.Name.Length == 0)
        {
            result = CommandResult.Empty();
            return true;
        }

        if (commands.TryGetValue(parsedCommand.Name, out ITerminalCommand command))
        {
            result = command.Execute(parsedCommand);
            return true;
        }

        result = null;
        return false;
    }
}
