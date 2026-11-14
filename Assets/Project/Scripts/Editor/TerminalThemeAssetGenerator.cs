#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TerminalThemeAssetGenerator
{
    private const string OutputDir = "Assets/Project/Data/Themes";
    private const string CyberpunkAssetPath = OutputDir + "/TerminalTheme_Cyberpunk.asset";
    private const string HackerAssetPath = OutputDir + "/TerminalTheme_Hacker.asset";
    private const string MonokaiAssetPath = OutputDir + "/TerminalTheme_Monokai.asset";
    private const string ClaudeAssetPath = OutputDir + "/TerminalTheme_Claude.asset";

    [MenuItem("Tools/Terminal/Generate Cyberpunk Theme Asset")]
    public static void GenerateCyberpunkTheme()
    {
        EnsureFolders();

        // Exact values from Assets/Command Prompt Shell/src/tweaks.jsx (cyberpunk).
        TerminalTheme theme = ScriptableObject.CreateInstance<TerminalTheme>();
        theme.background = FromHex("#0d0d12");
        theme.foreground = FromHex("#f0e6ff");
        theme.dim = FromHex("#4a3a5a");
        theme.prompt = FromHex("#6a3a8a");
        theme.user = FromHex("#ff2d78");
        theme.host = FromHex("#00fff5");
        theme.path = FromHex("#bd00ff");
        theme.accent = FromHex("#ffe600");
        theme.error = FromHex("#ff2d78");
        theme.dir = FromHex("#00fff5");
        theme.file = FromHex("#f0e6ff");
        theme.script = FromHex("#ffe600");
        theme.log = FromHex("#4a3a5a");
        theme.data = FromHex("#ff6b00");
        theme.caret = theme.foreground;
        theme.selection = new Color(1f, 1f, 1f, 0.18f);

        CreateOrReplace(theme, CyberpunkAssetPath);
    }

    [MenuItem("Tools/Terminal/Generate Hacker Theme Asset")]
    public static void GenerateHackerTheme()
    {
        EnsureFolders();

        // Exact values from Assets/Command Prompt Shell/src/tweaks.jsx (hacker).
        TerminalTheme theme = ScriptableObject.CreateInstance<TerminalTheme>();
        theme.background = FromHex("#000000");
        theme.foreground = FromHex("#00ff41");
        theme.dim = FromHex("#005c17");
        theme.prompt = FromHex("#007a1f");
        theme.user = FromHex("#00ff41");
        theme.host = FromHex("#00cc33");
        theme.path = FromHex("#00ff41");
        theme.accent = FromHex("#ccff00");
        theme.error = FromHex("#ff2222");
        theme.dir = FromHex("#00ffff");
        theme.file = FromHex("#00cc33");
        theme.script = FromHex("#ccff00");
        theme.log = FromHex("#007a1f");
        theme.data = FromHex("#ff9900");
        theme.caret = theme.foreground;
        theme.selection = new Color(1f, 1f, 1f, 0.18f);

        CreateOrReplace(theme, HackerAssetPath);
    }

    [MenuItem("Tools/Terminal/Generate Monokai Theme Asset")]
    public static void GenerateMonokaiTheme()
    {
        EnsureFolders();

        // Exact values from Assets/Command Prompt Shell/src/tweaks.jsx (monokai).
        TerminalTheme theme = ScriptableObject.CreateInstance<TerminalTheme>();
        theme.background = FromHex("#272822");
        theme.foreground = FromHex("#f8f8f2");
        theme.dim = FromHex("#75715e");
        theme.prompt = FromHex("#75715e");
        theme.user = FromHex("#a6e22e");
        theme.host = FromHex("#66d9e8");
        theme.path = FromHex("#e6db74");
        theme.accent = FromHex("#fd971f");
        theme.error = FromHex("#f92672");
        theme.dir = FromHex("#66d9e8");
        theme.file = FromHex("#f8f8f2");
        theme.script = FromHex("#a6e22e");
        theme.log = FromHex("#75715e");
        theme.data = FromHex("#e6db74");
        theme.caret = theme.foreground;
        theme.selection = new Color(1f, 1f, 1f, 0.18f);

        CreateOrReplace(theme, MonokaiAssetPath);
    }

    [MenuItem("Tools/Terminal/Generate Claude Theme Asset")]
    public static void GenerateClaudeTheme()
    {
        EnsureFolders();

        // Exact values from Assets/Command Prompt Shell/src/tweaks.jsx (claude).
        TerminalTheme theme = ScriptableObject.CreateInstance<TerminalTheme>();
        theme.background = FromOklch(0.14f, 0.02f, 260f);
        theme.foreground = FromOklch(0.92f, 0.01f, 80f);
        theme.dim = FromOklch(0.52f, 0.02f, 260f);
        theme.prompt = FromOklch(0.55f, 0.04f, 260f);
        theme.user = FromOklch(0.78f, 0.14f, 60f);
        theme.host = FromOklch(0.72f, 0.12f, 280f);
        theme.path = FromOklch(0.75f, 0.10f, 220f);
        theme.accent = FromOklch(0.80f, 0.16f, 50f);
        theme.error = FromOklch(0.68f, 0.18f, 25f);
        theme.dir = FromOklch(0.75f, 0.10f, 220f);
        theme.file = theme.foreground;
        theme.script = theme.accent;
        theme.log = theme.dim;
        theme.data = theme.user;
        theme.caret = theme.foreground;
        theme.selection = new Color(1f, 1f, 1f, 0.18f);

        CreateOrReplace(theme, ClaudeAssetPath);
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Project/Data"))
            AssetDatabase.CreateFolder("Assets/Project", "Data");

        if (!AssetDatabase.IsValidFolder(OutputDir))
            AssetDatabase.CreateFolder("Assets/Project/Data", "Themes");
    }

    private static void CreateOrReplace(TerminalTheme theme, string assetPath)
    {
        TerminalTheme existing = AssetDatabase.LoadAssetAtPath<TerminalTheme>(assetPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(assetPath);

        AssetDatabase.CreateAsset(theme, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = theme;
        EditorGUIUtility.PingObject(theme);
        Debug.Log($"[TerminalThemeAssetGenerator] Created {assetPath}");
    }

    private static Color FromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color c))
            return c;
        return Color.magenta;
    }

    // Minimal OKLCH->RGB conversion for the theme generator.
    // Good enough for matching the web palette closely in Unity.
    private static Color FromOklch(float l, float c, float hDeg)
    {
        float hRad = hDeg * Mathf.Deg2Rad;
        float a = c * Mathf.Cos(hRad);
        float b = c * Mathf.Sin(hRad);
        return OklabToSrgb(l, a, b);
    }

    private static Color OklabToSrgb(float L, float a, float b)
    {
        // OKLab -> LMS
        float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
        float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
        float s_ = L - 0.0894841775f * a - 1.2914855480f * b;

        float l3 = l_ * l_ * l_;
        float m3 = m_ * m_ * m_;
        float s3 = s_ * s_ * s_;

        // LMS -> linear sRGB
        float rLin = +4.0767416621f * l3 - 3.3077115913f * m3 + 0.2309699292f * s3;
        float gLin = -1.2684380046f * l3 + 2.6097574011f * m3 - 0.3413193965f * s3;
        float bLin = -0.0041960863f * l3 - 0.7034186147f * m3 + 1.7076147010f * s3;

        return new Color(LinearToSrgb01(rLin), LinearToSrgb01(gLin), LinearToSrgb01(bLin), 1f);
    }

    private static float LinearToSrgb01(float x)
    {
        x = Mathf.Clamp01(x);
        return x <= 0.0031308f ? 12.92f * x : 1.055f * Mathf.Pow(x, 1f / 2.4f) - 0.055f;
    }
}
#endif
