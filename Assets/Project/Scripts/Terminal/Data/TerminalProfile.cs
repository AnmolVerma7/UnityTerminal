using UnityEngine;

/// <summary>
/// Shell identity, prompt path, and startup / printmsg welcome text.
/// Assign on <see cref="TerminalController"/> instead of editing those fields on the component.
/// </summary>
[CreateAssetMenu(menuName = "Terminal/Terminal Profile", fileName = "TerminalProfile")]
public class TerminalProfile : ScriptableObject
{
    [Header("Prompt")]
    public string username = "operator";
    public string host = "local";
    public string path = "~";

    [Header("Welcome")]
    [TextArea(1, 8)]
    public string[] welcomeLines =
    {
        "Terminal initialized.",
        "Type 'help' for a list of commands."
    };
}
