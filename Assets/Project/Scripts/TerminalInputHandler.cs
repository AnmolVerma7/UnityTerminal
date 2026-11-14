using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Thin wrapper around a <see cref="TMP_InputField"/> that exposes a clean
/// Focus/Clear API and a <see cref="CommandSubmitted"/> event. The rest of
/// the terminal talks to this class instead of touching TMP_InputField
/// directly, which keeps input concerns in one place.
/// </summary>
public class TerminalInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField commandInputField;

    /// <summary>Raised when the user presses Enter in the input field.</summary>
    public event Action<string> CommandSubmitted;

    private void OnEnable()
    {
        if (commandInputField != null)
            commandInputField.onSubmit.AddListener(OnInputSubmit);
    }

    private void OnDisable()
    {
        if (commandInputField != null)
            commandInputField.onSubmit.RemoveListener(OnInputSubmit);
    }

    public void Focus()
    {
        if (commandInputField != null)
            commandInputField.ActivateInputField();
    }

    public void Clear()
    {
        if (commandInputField != null)
            commandInputField.text = string.Empty;
    }

    public void ClearAndFocus()
    {
        Clear();
        Focus();
    }

    private void OnInputSubmit(string text) => CommandSubmitted?.Invoke(text);
}
