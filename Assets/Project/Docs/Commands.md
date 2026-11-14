# Terminal commands

Commands are registered in `TerminalController.Awake()` and listed at runtime via `help`.

Shell prompt fields (`username`, `host`, `path`) and welcome text live on a **Terminal Profile** asset (`Assets/Project/Data/TerminalProfile_Default.asset` or **Create → Terminal → Terminal Profile**). Edit that asset instead of the controller.

| Command | Usage | Description |
|---------|--------|-------------|
| `help` | `help` | Lists every registered command with usage and description. |
| `clear` | `clear` | Clears terminal history output. |
| `echo` | `echo <text>` | Prints the text after the command name (arguments are preserved as a single string). |
| `whoami` | `whoami` | Prints the current username from the active shell session (from the terminal profile). |
| `pwd` | `pwd` | Prints the current working directory path from the shell session. |
| `date` | `date` | Prints the local date and time (e.g. weekday, month day year, 12-hour clock with am/pm). |
| `printmsg` | `printmsg` | Re-prints all **welcome lines** from the terminal profile (same text shown at startup). |

## Adding a command

1. Create a class implementing `ITerminalCommand` under `Assets/Project/Scripts/Terminal/Commands/`.
2. Implement `Name`, `Usage`, `Description`, and `Execute(ParsedCommand)`.
3. Register it in `TerminalController.Awake()` with `commandRegistry.Register(new YourCommand(...))`.
4. Add a row to this file.

`help` picks up new commands automatically from the registry.

## Planned

- **`printmsg <index>`** — print a single welcome line by index (e.g. `printmsg 0`).
