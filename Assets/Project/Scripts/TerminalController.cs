using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// High-level terminal logic. Subscribes to the input handler, echoes the
/// submitted command into the view, asks the command registry to execute it,
/// and formats the prompt prefix.
/// </summary>
public class TerminalController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerminalInputHandler inputHandler;
    [SerializeField] private TerminalView terminalView;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TerminalThemeApplier themeApplier;
    [SerializeField] private TerminalProfile profile;

    private ShellSession session;
    private CommandRegistry commandRegistry;

    private void Awake()
    {
        EnsureSession();

        commandRegistry = new CommandRegistry();
        commandRegistry.Register(new HelpCommand(commandRegistry));
        commandRegistry.Register(new ClearCommand());
        commandRegistry.Register(new EchoCommand());
        commandRegistry.Register(new WhoAmICommand(session));
        commandRegistry.Register(new DateCommand());
        commandRegistry.Register(new PwdCommand(session));
        commandRegistry.Register(new PrintMsgCommand(profile));
    }

    /// <summary>
    /// Theme applier can refresh the prompt from OnEnable/OnValidate before this
    /// component's Awake runs, so session must exist before BuildPrompt.
    /// </summary>
    private void EnsureSession()
    {
        if (session != null) return;

        string username = profile != null ? profile.username : "operator";
        string host = profile != null ? profile.host : "local";
        string path = profile != null ? profile.path : "~";
        session = new ShellSession(username, host, path);
    }

    private void Start()
    {
        RefreshPrompt();

        if (inputHandler != null)
            inputHandler.CommandSubmitted += HandleSubmit;

        PrintWelcome();
        inputHandler?.Focus();
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
            inputHandler.CommandSubmitted -= HandleSubmit;
    }

    /// <summary>Used by <see cref="TerminalThemeApplier"/> to sync history markup.</summary>
    public TerminalView TerminalView => terminalView;

    /// <summary>Re-apply the current prompt prefix to the label in the scene.</summary>
    public void RefreshPrompt()
    {
        if (promptText != null)
        {
            promptText.text = BuildPrompt();
            SyncInputRowSpacingWithPromptAdvance();
        }
    }

    private void HandleSubmit(string rawText)
    {
        if (terminalView == null || inputHandler == null)
            return;

        string submittedText = rawText ?? string.Empty;
        string commandText = submittedText.Trim();
        if (commandText.Length == 0)
        {
            inputHandler.ClearAndFocus();
            return;
        }

        // Echo exactly what the user typed; no trimming/modification.
        terminalView.AppendLine(BuildPrompt() + submittedText);

        if (commandRegistry.TryExecute(commandText, out CommandResult result))
        {
            ApplyCommandResult(result);
        }
        else
        {
            TerminalTheme theme = themeApplier != null ? themeApplier.Theme : null;
            string message = "Unknown command: " + commandText + "\n" + "Type 'help' for a list of commands.";
            terminalView.AppendLine(theme != null
                ? TerminalRichText.Colorize(theme.error, message)
                : message);
        }

        inputHandler.ClearAndFocus();
    }

    private void ApplyCommandResult(CommandResult result)
    {
        if (terminalView == null || result == null) return;

        if (result.ShouldClearScreen)
            terminalView.Clear();

        terminalView.AppendLines(result.OutputLines);
    }

    private void PrintWelcome()
    {
        if (terminalView == null || profile == null || profile.welcomeLines == null) return;
        foreach (string line in profile.welcomeLines)
            terminalView.AppendLine(line);
    }

    /// <summary>
    /// Keeps the live input row's spacing aligned with the prompt's true text
    /// advance so the typed command and echoed history start at the same X.
    /// </summary>
    private void SyncInputRowSpacingWithPromptAdvance()
    {
        if (promptText == null) return;

        HorizontalLayoutGroup inputRow = promptText.transform.parent != null
            ? promptText.transform.parent.GetComponent<HorizontalLayoutGroup>()
            : null;
        if (inputRow == null) return;

        promptText.ForceMeshUpdate();
        float renderedAdvance = MeasureWidth(promptText, promptText.text);
        float preferredWidth = promptText.preferredWidth;
        float requiredSpacing = Mathf.Max(0f, renderedAdvance - preferredWidth);

        if (!Mathf.Approximately(inputRow.spacing, requiredSpacing))
            inputRow.spacing = requiredSpacing;
    }

    /// <summary>
    /// Measures rendered width while preserving trailing whitespace / bearings.
    /// </summary>
    private static float MeasureWidth(TMP_Text target, string value)
    {
        const string marker = "x";
        float markerWidth = target.GetPreferredValues(marker).x;
        float markerPlus = target.GetPreferredValues(value + marker).x;
        return Mathf.Max(0f, markerPlus - markerWidth);
    }

    // Use a non‑breaking space so TMP/layout treats it as real width at line end.
    private string BuildPrompt()
    {
        EnsureSession();

        TerminalTheme theme = themeApplier != null ? themeApplier.Theme : null;
        if (theme == null)
        {
            return
                $"{TerminalRichText.Escape(session.Username)}@" +
                $"{TerminalRichText.Escape(session.Host)}:" +
                $"{TerminalRichText.Escape(session.Path)}$\u00A0";
        }

        string userHex = TerminalTheme.ToHex(theme.user);
        string hostHex = TerminalTheme.ToHex(theme.host);
        string pathHex = TerminalTheme.ToHex(theme.path);
        string promptHex = TerminalTheme.ToHex(theme.prompt);

        // Keep the base prompt colored too for that Cyberpunk look.
        return
            $"<color=#{userHex}>{TerminalRichText.Escape(session.Username)}</color>" +
            $"<color=#{promptHex}>@</color>" +
            $"<color=#{hostHex}>{TerminalRichText.Escape(session.Host)}</color>" +
            $"<color=#{promptHex}>:</color>" +
            $"<color=#{pathHex}>{TerminalRichText.Escape(session.Path)}</color>" +
            $"<color=#{promptHex}>$</color>\u00A0";
    }
}
