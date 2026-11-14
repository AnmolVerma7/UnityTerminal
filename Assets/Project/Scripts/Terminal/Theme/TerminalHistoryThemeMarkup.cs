using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Rewrites TMP history strings when the terminal theme changes or is cleared.
/// </summary>
public static class TerminalHistoryThemeMarkup
{
    private static readonly Regex TmpColorOpenTag = new Regex(
        @"<color=#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})>",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Returns updated markup for <paramref name="raw"/> when the theme changes,
    /// or strips theme color tags when <paramref name="newTheme"/> is null.
    /// Returns <paramref name="raw"/> when nothing changes.
    /// </summary>
    public static string Transform(string raw, TerminalThemeHexCache previous, TerminalTheme newTheme)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        if (newTheme == null)
            return StripTmpColorMarkup(raw);

        if (!previous.IsValid)
            return raw;

        return ReplaceThemeColorOpenings(raw, previous, newTheme);
    }

    private static string StripTmpColorMarkup(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        string stripped = TmpColorOpenTag.Replace(text, string.Empty);
        return stripped.Replace("</color>", string.Empty);
    }

    private static string ReplaceThemeColorOpenings(string text, TerminalThemeHexCache prev, TerminalTheme next)
    {
        string[] olds =
        {
            prev.User, prev.Host, prev.Path, prev.Prompt, prev.Error
        };
        string[] news =
        {
            TerminalTheme.ToHex(next.user).ToUpperInvariant(),
            TerminalTheme.ToHex(next.host).ToUpperInvariant(),
            TerminalTheme.ToHex(next.path).ToUpperInvariant(),
            TerminalTheme.ToHex(next.prompt).ToUpperInvariant(),
            TerminalTheme.ToHex(next.error).ToUpperInvariant(),
        };

        var byOldHex = new Dictionary<string, HashSet<string>>(System.StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < olds.Length; i++)
        {
            if (string.IsNullOrEmpty(olds[i])) continue;
            if (!byOldHex.TryGetValue(olds[i], out HashSet<string> targets))
            {
                targets = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                byOldHex[olds[i]] = targets;
            }
            targets.Add(news[i]);
        }

        string result = text;
        foreach (KeyValuePair<string, HashSet<string>> kv in byOldHex)
        {
            if (kv.Value.Count != 1) continue;

            string soleNew = null;
            foreach (string n in kv.Value)
            {
                soleNew = n;
                break;
            }

            if (soleNew == null || string.Equals(kv.Key, soleNew, System.StringComparison.OrdinalIgnoreCase))
                continue;

            string newOpen = "<color=#" + soleNew + ">";
            string pattern = "(?i)<color=#" + Regex.Escape(kv.Key) + ">";
            result = Regex.Replace(result, pattern, newOpen);
        }

        return result;
    }
}
