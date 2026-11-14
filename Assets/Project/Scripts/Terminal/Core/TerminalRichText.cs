using UnityEngine;

/// <summary>
/// Helper for building safe TMP rich-text strings.
/// </summary>
public static class TerminalRichText
{
    public static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // Minimal escaping for TMP rich text.
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    public static string Colorize(Color color, string text)
    {
        string hex = TerminalTheme.ToHex(color);
        return $"<color=#{hex}>{Escape(text)}</color>";
    }
}

