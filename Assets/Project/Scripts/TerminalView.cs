using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders terminal history into a single TMP_Text. Positioning of the output,
/// prompt, and input field is handled by Unity's layout groups in the scene,
/// while an optional ScrollRect keeps the latest terminal line in view.
/// </summary>
public class TerminalView : MonoBehaviour
{
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private ScrollRect scrollRect;

    private readonly StringBuilder history = new StringBuilder();

    /// <summary>
    /// Rewrites stored history so TMP color openings match <paramref name="newTheme"/>,
    /// or strips theme color tags when <paramref name="newTheme"/> is null.
    /// </summary>
    public void RewriteHistoryForTheme(TerminalThemeHexCache previous, TerminalTheme newTheme)
    {
        if (history.Length == 0) return;

        string raw = history.ToString();
        string next = TerminalHistoryThemeMarkup.Transform(raw, previous, newTheme);
        if (next == raw) return;

        history.Clear();
        history.Append(next);
        if (outputText != null)
            outputText.text = history.ToString();
    }

    /// <summary>Append a line of output to the terminal history.</summary>
    public void AppendLine(string line)
    {
        if (outputText == null)
        {
            Debug.LogError("[TerminalView] outputText is not assigned.");
            return;
        }

        AppendToHistory(line);
        outputText.text = history.ToString();
        ScrollToBottom();
    }

    /// <summary>
    /// Append multiple lines of output to the terminal history in order.
    /// </summary>
    /// <param name="lines">Lines to append. Null is ignored.</param>
    public void AppendLines(IEnumerable<string> lines)
    {
        if (lines == null) return;

        if (outputText == null)
        {
            Debug.LogError("[TerminalView] outputText is not assigned.");
            return;
        }

        foreach (string line in lines)
            AppendToHistory(line);

        outputText.text = history.ToString();
        ScrollToBottom();
    }

    /// <summary>Clear all history output.</summary>
    public void Clear()
    {
        history.Clear();
        if (outputText != null)
            outputText.text = string.Empty;
        ScrollToBottom();
    }

    private void AppendToHistory(string line)
    {
        if (history.Length > 0)
            history.Append('\n');

        history.Append(line);
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null) return;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
