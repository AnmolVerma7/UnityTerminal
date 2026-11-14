using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A reusable theme definition for the terminal UI.
/// </summary>
[CreateAssetMenu(menuName = "Terminal/Terminal Theme", fileName = "TerminalTheme")]
public class TerminalTheme : ScriptableObject
{
    [Header("Base")]
    public Color background = new Color32(0x0D, 0x0D, 0x12, 0xFF); // #0d0d12
    public Color foreground = new Color32(0xF0, 0xE6, 0xFF, 0xFF); // #f0e6ff
    public Color dim = new Color32(0x4A, 0x3A, 0x5A, 0xFF);        // #4a3a5a

    [Header("Prompt")]
    public Color prompt = new Color32(0x6A, 0x3A, 0x8A, 0xFF);     // #6a3a8a
    public Color user = new Color32(0xFF, 0x2D, 0x78, 0xFF);       // #ff2d78
    public Color host = new Color32(0x00, 0xFF, 0xF5, 0xFF);       // #00fff5
    public Color path = new Color32(0xBD, 0x00, 0xFF, 0xFF);       // #bd00ff

    [Header("System")]
    public Color error = new Color32(0xFF, 0x2D, 0x78, 0xFF);      // #ff2d78
    public Color accent = new Color32(0xFF, 0xE6, 0x00, 0xFF);     // #ffe600

    [Header("Output (file types)")]
    public Color dir = new Color32(0x00, 0xFF, 0xF5, 0xFF);        // cyberpunk dir
    public Color file = new Color32(0xF0, 0xE6, 0xFF, 0xFF);       // cyberpunk file
    public Color script = new Color32(0xFF, 0xE6, 0x00, 0xFF);     // cyberpunk script/exec
    public Color log = new Color32(0x4A, 0x3A, 0x5A, 0xFF);        // cyberpunk log
    public Color data = new Color32(0xFF, 0x6B, 0x00, 0xFF);       // cyberpunk data

    [Header("Selection / Caret")]
    public Color selection = new Color32(0xFF, 0xFF, 0xFF, 0x2E);  // rgba(255,255,255,0.18)
    public Color caret = new Color32(0xF0, 0xE6, 0xFF, 0xFF);      // match fg by default

    public static string ToHex(Color c) => ColorUtility.ToHtmlStringRGB(c);

#if UNITY_EDITOR
    private void OnValidate()
    {
        TerminalTheme self = this;
        EditorApplication.delayCall += () =>
        {
            if (self == null) return;
            TerminalThemeApplier[] appliers =
                Object.FindObjectsByType<TerminalThemeApplier>(FindObjectsInactive.Include);
            for (int i = 0; i < appliers.Length; i++)
            {
                if (appliers[i] != null && appliers[i].Theme == self)
                    appliers[i].Apply();
            }
        };
    }
#endif
}
