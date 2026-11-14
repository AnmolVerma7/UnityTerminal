/// <summary>
/// Snapshot of TMP opening-tag hex values from the last themed apply, used to
/// rewrite history when the theme changes (including duplicate-hex ambiguity).
/// </summary>
public readonly struct TerminalThemeHexCache
{
    public readonly string User;
    public readonly string Host;
    public readonly string Path;
    public readonly string Prompt;
    public readonly string Error;

    public TerminalThemeHexCache(string user, string host, string path, string prompt, string error)
    {
        User = user;
        Host = host;
        Path = path;
        Prompt = prompt;
        Error = error;
    }

    public static TerminalThemeHexCache FromTheme(TerminalTheme t)
    {
        if (t == null) return default;
        return new TerminalThemeHexCache(
            NormalizeHex(TerminalTheme.ToHex(t.user)),
            NormalizeHex(TerminalTheme.ToHex(t.host)),
            NormalizeHex(TerminalTheme.ToHex(t.path)),
            NormalizeHex(TerminalTheme.ToHex(t.prompt)),
            NormalizeHex(TerminalTheme.ToHex(t.error)));
    }

    public bool IsValid =>
        !string.IsNullOrEmpty(User) &&
        !string.IsNullOrEmpty(Host) &&
        !string.IsNullOrEmpty(Path) &&
        !string.IsNullOrEmpty(Prompt) &&
        !string.IsNullOrEmpty(Error);

    private static string NormalizeHex(string hex) =>
        string.IsNullOrEmpty(hex) ? hex : hex.ToUpperInvariant();
}
