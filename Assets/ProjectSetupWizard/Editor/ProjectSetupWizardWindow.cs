using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ProjectSetupWizard
{
    /// <summary>
    /// Editor window for the Project Setup Wizard.
    /// Open via <b>Tools → Project Setup Wizard</b> or <b>Cmd+Shift+P</b>.
    /// </summary>
    public class ProjectSetupWizardWindow : EditorWindow
    {
        // ── Config ────────────────────────────────────────────────────────────
        private ProjectFolderConfig selectedConfig;
        private string customRootName    = "Project";
        private bool   useCustomConfig   = false;
        private bool   deleteBoilerplate  = true;
        private bool   installPackages    = true;

        // ── Quick-setup folder list ───────────────────────────────────────────
        private List<QuickFolderEntry> quickFolders = new List<QuickFolderEntry>();

        // ── UI state ──────────────────────────────────────────────────────────
        private Vector2 scrollPosition;
        private bool    showCustomFolderInput = false;
        private string  newFolderPath         = "";
        private bool    showResults           = false;
        private FolderCreator.CreationResult lastResult;

        // ── Cached styles (null-checked on first use) ─────────────────────────
        private GUIStyle _headerStyle;
        private GUIStyle _boxStyle;
        private GUIStyle HeaderStyle => _headerStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, margin = new RectOffset(0, 0, 10, 5) };
        private GUIStyle BoxStyle    => _boxStyle    ??= new GUIStyle(EditorStyles.helpBox)  { padding = new RectOffset(10, 10, 10, 10) };

        // ─────────────────────────────────────────────────────────────────────

        [MenuItem("Tools/Project Setup Wizard %#p")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectSetupWizardWindow>("Project Setup Wizard");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable() => InitializeQuickFolders();

        // ─────────────────────────────────────────────────────────────────────
        // Folder list
        // ─────────────────────────────────────────────────────────────────────

        private void InitializeQuickFolders()
        {
            quickFolders.Clear();

            Add("Art",            true);
            Add("Art/Animations", true);
            Add("Art/Materials",  true);
            Add("Art/Models",     true);
            Add("Art/Shaders",    false);
            Add("Art/Sprites",    true);
            Add("Art/Textures",   true);
            Add("Art/UI",         true);
            Add("Art/VFX",        false);

            Add("Audio",        true);
            Add("Audio/Music",  true);
            Add("Audio/SFX",    true);

            Add("Data",     true);
            Add("Data/SOs", true);

            Add("Docs",    true);
            Add("Fonts",   true);
            Add("Plugins", false);

            Add("Prefabs",             true);
            Add("Prefabs/Characters",  false);
            Add("Prefabs/Environment", false);
            Add("Prefabs/UI",          true);

            Add("Resources", false);
            Add("Scenes",    true);

            Add("Scripts",         true);
            Add("Scripts/Editor",  true);
            Add("Scripts/Runtime", true);
        }

        private void Add(string path, bool enabled) =>
            quickFolders.Add(new QuickFolderEntry(path, enabled));

        // ─────────────────────────────────────────────────────────────────────
        // GUI
        // ─────────────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawConfigSection();
            EditorGUILayout.Space(10);

            if (useCustomConfig)
                DrawConfigBasedSetup();
            else
                DrawQuickSetup();

            EditorGUILayout.Space(10);
            DrawOptionsSection();

            EditorGUILayout.Space(10);
            DrawPackagesSection();

            EditorGUILayout.Space(20);
            DrawCreateButton();

            if (showResults && lastResult != null)
            {
                EditorGUILayout.Space(10);
                DrawResults();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("🗂️ Project Setup Wizard", HeaderStyle);
            EditorGUILayout.HelpBox(
                "Creates a standardised folder structure under Assets. " +
                "Existing folders are never overwritten.",
                MessageType.Info);
        }

        private void DrawConfigSection()
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            EditorGUILayout.LabelField("Configuration Mode", EditorStyles.boldLabel);

            useCustomConfig = EditorGUILayout.Toggle("Use Saved Config", useCustomConfig);

            if (useCustomConfig)
            {
                EditorGUI.indentLevel++;
                selectedConfig = (ProjectFolderConfig)EditorGUILayout.ObjectField(
                    "Config Asset", selectedConfig, typeof(ProjectFolderConfig), false);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create New Config", GUILayout.Height(25)))
                    CreateNewConfig();
                if (selectedConfig != null && GUILayout.Button("Reset to Defaults", GUILayout.Height(25)))
                {
                    selectedConfig.SetupDefaultFolders();
                    EditorUtility.SetDirty(selectedConfig);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
            else
            {
                customRootName = EditorGUILayout.TextField("Root Folder Name", customRootName);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickSetup()
        {
            EditorGUILayout.BeginVertical(BoxStyle);

            // Header row with select-all / none shortcuts
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quick Setup Folders", EditorStyles.boldLabel);
            if (GUILayout.Button("All",  GUILayout.Width(40))) quickFolders.ForEach(f => f.enabled = true);
            if (GUILayout.Button("None", GUILayout.Width(40))) quickFolders.ForEach(f => f.enabled = false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            string currentCategory = "";
            foreach (var folder in quickFolders)
            {
                string category = folder.path.Contains("/") ? folder.path.Split('/')[0] : folder.path;
                if (category != currentCategory)
                {
                    currentCategory = category;
                    EditorGUILayout.Space(3);
                }

                EditorGUILayout.BeginHorizontal();
                folder.enabled = EditorGUILayout.Toggle(folder.enabled, GUILayout.Width(20));

                int depth = folder.path.Split('/').Length - 1;
                if (depth > 0)
                {
                    GUILayout.Space(depth * 15);
                    EditorGUILayout.LabelField("↳ " + folder.path.Split('/')[depth]);
                }
                else
                {
                    EditorGUILayout.LabelField(folder.path, EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);
            showCustomFolderInput = EditorGUILayout.Foldout(showCustomFolderInput, "Add Custom Folder");
            if (showCustomFolderInput)
                DrawCustomFolderInput();

            EditorGUILayout.EndVertical();
        }

        private void DrawCustomFolderInput()
        {
            EditorGUILayout.BeginHorizontal();
            newFolderPath = EditorGUILayout.TextField(newFolderPath);
            if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrWhiteSpace(newFolderPath))
            {
                quickFolders.Add(new QuickFolderEntry(newFolderPath.Trim(), true));
                newFolderPath = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Path relative to root — e.g. 'Systems/SaveSystem'", MessageType.None);
        }

        private void DrawConfigBasedSetup()
        {
            EditorGUILayout.BeginVertical(BoxStyle);

            if (selectedConfig == null)
            {
                EditorGUILayout.HelpBox("Select or create a config asset to define the folder structure.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField($"Config: {selectedConfig.name}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Root: {selectedConfig.rootFolderName}");
                EditorGUILayout.LabelField($"Folders: {selectedConfig.folders.Count}");
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Edit in Inspector"))
                    Selection.activeObject = selectedConfig;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawOptionsSection()
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            deleteBoilerplate = EditorGUILayout.Toggle(
                new GUIContent(
                    "Remove Unity Defaults",
                    "Deletes 'TutorialInfo' and 'Readme.asset' added by Unity to new projects."),
                deleteBoilerplate);
            EditorGUILayout.EndVertical();
        }

        private void DrawPackagesSection()
        {
            EditorGUILayout.BeginVertical(BoxStyle);

            // Header row with master toggle and select shortcuts
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📦 Package Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("All",  GUILayout.Width(40))) PackageInstaller.Catalog.ForEach(p => p.enabled = true);
            if (GUILayout.Button("None", GUILayout.Width(40))) PackageInstaller.Catalog.ForEach(p => p.enabled = false);
            EditorGUILayout.EndHorizontal();

            installPackages = EditorGUILayout.Toggle(
                new GUIContent("Install Selected", "Installs checked packages via the Package Manager on setup."),
                installPackages);

            EditorGUILayout.Space(4);

            if (PackageInstaller.IsInstalling)
            {
                EditorGUILayout.HelpBox("Installing packages in background — check the console for progress.", MessageType.Info);
            }
            else
            {
                foreach (var pkg in PackageInstaller.Catalog)
                {
                    EditorGUILayout.BeginHorizontal();
                    pkg.enabled = EditorGUILayout.Toggle(pkg.enabled, GUILayout.Width(20));
                    EditorGUILayout.LabelField(pkg.displayName, GUILayout.ExpandWidth(true));
                    if (pkg.essential)
                        EditorGUILayout.LabelField("★ essential", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(70));
                    EditorGUILayout.LabelField(pkg.packageId, EditorStyles.miniLabel, GUILayout.Width(220));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCreateButton()
        {
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("🚀  Create Folder Structure", GUILayout.Height(40)))
                RunSetup();
            GUI.backgroundColor = Color.white;
        }

        private void DrawResults()
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            EditorGUILayout.LabelField("Folder Results", EditorStyles.boldLabel);

            var type = lastResult.HasErrors ? MessageType.Error : MessageType.Info;
            string summary = lastResult.HasErrors
                ? $"Completed with errors:\n{string.Join("\n", lastResult.errors)}"
                : lastResult.GetSummary();
            EditorGUILayout.HelpBox(summary, type);

            if (lastResult.createdPaths.Count > 0)
            {
                EditorGUILayout.LabelField($"✅ Created ({lastResult.foldersCreated}):", EditorStyles.miniLabel);
                foreach (var p in lastResult.createdPaths)
                    EditorGUILayout.LabelField($"  • {p}", EditorStyles.miniLabel);
            }

            if (lastResult.skippedPaths.Count > 0)
            {
                if (lastResult.skippedPaths.Count <= 10)
                {
                    EditorGUILayout.LabelField($"⏭ Skipped ({lastResult.foldersSkipped}):", EditorStyles.miniLabel);
                    foreach (var p in lastResult.skippedPaths)
                        EditorGUILayout.LabelField($"  • {p}", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField($"⏭ {lastResult.foldersSkipped} folders already existed — skipped.", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndVertical();

            if (installPackages && PackageInstaller.LastResult != null)
                DrawPackageResults();
        }

        private void DrawPackageResults()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(BoxStyle);
            EditorGUILayout.LabelField("Package Results", EditorStyles.boldLabel);

            var pkgRes = PackageInstaller.LastResult;

            if (PackageInstaller.IsInstalling)
            {
                EditorGUILayout.HelpBox("Installation in progress...", MessageType.Info);
                Repaint(); // Keep refreshing while the background install is active.
            }
            else
            {
                var pType = pkgRes.HasErrors ? MessageType.Error : MessageType.Info;
                string pSum = pkgRes.HasErrors
                    ? $"Completed with errors:\n{string.Join("\n", pkgRes.errors)}"
                    : $"Installed: {pkgRes.installedPackages.Count}  |  Already existed: {pkgRes.skippedPackages.Count}";

                EditorGUILayout.HelpBox(pSum, pType);

                if (pkgRes.installedPackages.Count > 0)
                {
                    EditorGUILayout.LabelField($"✅ Installed ({pkgRes.installedPackages.Count}):", EditorStyles.miniLabel);
                    foreach (var p in pkgRes.installedPackages)
                        EditorGUILayout.LabelField($"  • {p}", EditorStyles.miniLabel);
                }

                if (pkgRes.skippedPackages.Count > 0)
                {
                    if (pkgRes.skippedPackages.Count <= 10)
                    {
                        EditorGUILayout.LabelField($"⏭ Skipped ({pkgRes.skippedPackages.Count}):", EditorStyles.miniLabel);
                        foreach (var p in pkgRes.skippedPackages)
                            EditorGUILayout.LabelField($"  • {p}", EditorStyles.miniLabel);
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"⏭ {pkgRes.skippedPackages.Count} packages already installed — skipped.", EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Actions
        // ─────────────────────────────────────────────────────────────────────

        private void RunSetup()
        {
            showResults = true;

            if (useCustomConfig && selectedConfig != null)
            {
                lastResult = FolderCreator.CreateFolderStructure(selectedConfig);
            }
            else
            {
                var enabled = new List<string>();
                foreach (var f in quickFolders)
                    if (f.enabled) enabled.Add(f.path);

                lastResult = FolderCreator.CreateFolders($"Assets/{customRootName}", enabled);
            }

            if (deleteBoilerplate)
                FolderCreator.DeleteBoilerplate(lastResult);

            if (installPackages)
            {
                var toInstall = new List<string>();
                foreach (var pkg in PackageInstaller.Catalog)
                    if (pkg.enabled) toInstall.Add(pkg.packageId);
                PackageInstaller.InstallPackages(toInstall);
            }

            Debug.Log($"[ProjectSetup] {lastResult.GetSummary()}");
        }

        private void CreateNewConfig()
        {
            var config = CreateInstance<ProjectFolderConfig>();
            config.SetupDefaultFolders();

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Folder Config", "NewProjectFolderConfig", "asset",
                "Choose where to save the configuration asset.");

            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            selectedConfig = config;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Data
        // ─────────────────────────────────────────────────────────────────────

        private class QuickFolderEntry
        {
            public string path;
            public bool   enabled;

            public QuickFolderEntry(string path, bool enabled)
            {
                this.path    = path;
                this.enabled = enabled;
            }
        }
    }
}
