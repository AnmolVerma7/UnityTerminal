using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies a <see cref="TerminalTheme"/> to the terminal UI components.
/// Captures baseline colors once so clearing the theme restores scene colors.
/// Optionally syncs <see cref="TerminalController"/> prompt/history markup when
/// the theme changes in the Editor or at runtime.
/// </summary>
public class TerminalThemeApplier : MonoBehaviour
{
    [SerializeField] private TerminalTheme theme;

    [Header("UI Targets")]
    [SerializeField] private Image panelBackground;
    [SerializeField] private TMP_Text historyText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_InputField inputField;

    [Header("Content sync (optional)")]
    [Tooltip("When set, prompt prefix and history TMP markup update when the theme changes or is cleared.")]
    [SerializeField] private TerminalController terminalController;

    public TerminalTheme Theme => theme;

    private bool _baselinesCaptured;
    private Color _panelColor;
    private Color _historyColor;
    private Color _promptColor;
    private Color _inputTextColor;
    private Color _caretColor;
    private Color _selectionColor;
    private bool _inputCustomCaretColor;

    private TerminalThemeHexCache _lastHex;

    private void OnEnable() => Apply();

#if UNITY_EDITOR
    private void OnValidate() => Apply();
#endif

    private void Reset() => _baselinesCaptured = false;

    public void Apply()
    {
        CaptureBaselinesIfNeeded();

        TerminalThemeHexCache previousHex = _lastHex;

        if (theme == null)
        {
            RestoreBaselines();
            _lastHex = default;
            SyncTerminalContentAfterTheme(previousHex, null);
            return;
        }

        if (panelBackground != null)
            panelBackground.color = theme.background;

        if (historyText != null)
            historyText.color = theme.foreground;

        if (promptText != null)
            promptText.color = theme.foreground;

        if (inputField != null)
        {
            if (inputField.textComponent != null)
                inputField.textComponent.color = theme.foreground;

            inputField.caretColor = theme.caret;
            inputField.customCaretColor = true;
            inputField.selectionColor = theme.selection;
        }

        SyncTerminalContentAfterTheme(previousHex, theme);
        _lastHex = TerminalThemeHexCache.FromTheme(theme);
    }

    private void SyncTerminalContentAfterTheme(TerminalThemeHexCache previousHex, TerminalTheme newTheme)
    {
        if (terminalController == null) return;
        terminalController.TerminalView?.RewriteHistoryForTheme(previousHex, newTheme);
        terminalController.RefreshPrompt();
    }

    private void CaptureBaselinesIfNeeded()
    {
        if (_baselinesCaptured) return;

        if (panelBackground != null)
            _panelColor = panelBackground.color;
        if (historyText != null)
            _historyColor = historyText.color;
        if (promptText != null)
            _promptColor = promptText.color;
        if (inputField != null)
        {
            if (inputField.textComponent != null)
                _inputTextColor = inputField.textComponent.color;
            _caretColor = inputField.caretColor;
            _selectionColor = inputField.selectionColor;
            _inputCustomCaretColor = inputField.customCaretColor;
        }

        _baselinesCaptured = true;
    }

    private void RestoreBaselines()
    {
        if (panelBackground != null)
            panelBackground.color = _panelColor;
        if (historyText != null)
            historyText.color = _historyColor;
        if (promptText != null)
            promptText.color = _promptColor;
        if (inputField != null)
        {
            if (inputField.textComponent != null)
                inputField.textComponent.color = _inputTextColor;
            inputField.caretColor = _caretColor;
            inputField.selectionColor = _selectionColor;
            inputField.customCaretColor = _inputCustomCaretColor;
        }
    }
}
