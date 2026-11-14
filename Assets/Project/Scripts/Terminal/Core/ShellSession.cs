/// <summary>
/// Current shell identity and working directory for the terminal prompt and commands.
/// </summary>
public class ShellSession
{
    public string Username { get; set; }
    public string Host { get; set; }
    public string Path { get; set; }

    public ShellSession(string username, string host, string path)
    {
        Username = username;
        Host = host;
        Path = path;
    }

    /// <summary>Plain prompt string, e.g. <c>operator@local:~$ </c></summary>
    public string BuildPrompt()
    {
        return $"{Username}@{Host}:{Path}$ ";
    }
}
