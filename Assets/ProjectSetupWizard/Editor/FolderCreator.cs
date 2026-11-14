using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ProjectSetupWizard
{
    /// <summary>
    /// Core utility for creating and organising Unity project folder structures.
    /// All public entry points funnel into <see cref="ExecuteFolderCreation"/> so
    /// every operation benefits from the same smart-move and duplicate-skip logic.
    /// </summary>
    public static class FolderCreator
    {
        // ─────────────────────────────────────────────────────────────────────
        // Public result type
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Aggregates the outcome of a folder-creation run.</summary>
        public class CreationResult
        {
            public int foldersCreated;
            public int foldersSkipped;
            public List<string> createdPaths = new List<string>();
            public List<string> skippedPaths  = new List<string>();
            public List<string> errors        = new List<string>();

            public bool HasErrors => errors.Count > 0;

            public string GetSummary() =>
                $"Created: {foldersCreated}  |  Already existed: {foldersSkipped}  |  Errors: {errors.Count}";
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a folder hierarchy defined by a <see cref="ProjectFolderConfig"/> asset.
        /// </summary>
        public static CreationResult CreateFolderStructure(ProjectFolderConfig config)
        {
            var result = new CreationResult();

            if (config == null)
            {
                result.errors.Add("ProjectFolderConfig is null.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(config.rootFolderName))
            {
                result.errors.Add("Root folder name is empty.");
                return result;
            }

            var paths = new List<string>();
            foreach (var folder in config.folders)
            {
                if (folder.enabled && !string.IsNullOrWhiteSpace(folder.path))
                    paths.Add(folder.path);
            }

            ExecuteFolderCreation($"Assets/{config.rootFolderName}", paths, result);
            return result;
        }

        /// <summary>
        /// Creates a folder hierarchy from a plain list of relative paths under <paramref name="rootPath"/>.
        /// </summary>
        /// <param name="rootPath">Unity asset path for the root (e.g. <c>Assets/Project</c>).</param>
        /// <param name="folderNames">Paths relative to the root, e.g. <c>Art/Materials</c>.</param>
        public static CreationResult CreateFolders(string rootPath, IEnumerable<string> folderNames)
        {
            var result = new CreationResult();

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                result.errors.Add("Root path is empty.");
                return result;
            }

            ExecuteFolderCreation(rootPath, folderNames, result);
            return result;
        }

        /// <summary>
        /// Removes the default Unity boilerplate: the <c>TutorialInfo</c> folder and
        /// <c>Readme.asset</c>. Silently skips either item if it no longer exists.
        /// </summary>
        public static void DeleteBoilerplate(CreationResult result)
        {
            TryDelete("Assets/TutorialInfo", result);
            TryDelete("Assets/Readme.asset",  result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Core execution pipeline
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Single execution path shared by every public entry point.
        /// Order: Unity Core setup → create root → move Scenes → create remaining folders → refresh.
        /// </summary>
        private static void ExecuteFolderCreation(
            string rootPath,
            IEnumerable<string> folderNames,
            CreationResult result)
        {
            SetupUnityCoreFolders(result);

            CreateSingleFolder(rootPath, result);
            MoveExistingScenesFolder(rootPath, result);

            foreach (var name in folderNames)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    CreateFolderWithParents($"{rootPath}/{name}", result);
            }

            AssetDatabase.Refresh();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Folder helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Walks each segment of <paramref name="path"/> and creates any missing folders
        /// along the way, so nested paths like <c>Assets/Project/Art/Materials</c> always work.
        /// </summary>
        private static void CreateFolderWithParents(string path, CreationResult result)
        {
            string[] parts = path.Split('/');
            string current = "";

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;

                string parent = current;
                current = string.IsNullOrEmpty(current) ? part : $"{current}/{part}";

                if (current == "Assets") continue; // always exists

                CreateSingleFolder(current, result, parent);
            }
        }

        /// <summary>
        /// Creates one folder. Skips silently if it already exists to avoid duplicates.
        /// </summary>
        private static void CreateSingleFolder(string path, CreationResult result, string parentPath = null)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                result.foldersSkipped++;
                result.skippedPaths.Add(path);
                return;
            }

            if (string.IsNullOrEmpty(parentPath))
            {
                int slash = path.LastIndexOf('/');
                parentPath = slash > 0 ? path.Substring(0, slash) : "Assets";
            }

            try
            {
                string guid = AssetDatabase.CreateFolder(parentPath, Path.GetFileName(path));

                if (!string.IsNullOrEmpty(guid))
                {
                    result.foldersCreated++;
                    result.createdPaths.Add(path);
                }
                else
                {
                    result.errors.Add($"Unity returned no GUID for: {path}");
                }
            }
            catch (System.Exception ex)
            {
                result.errors.Add($"Exception creating {path}: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Unity Core setup
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a base 'Unity Core' folder structure and moves the 
        /// Settings folder and InputSystem_Actions asset into it, if they exist.
        /// </summary>
        private static void SetupUnityCoreFolders(CreationResult result)
        {
            string corePath = "Assets/Unity Core";
            CreateSingleFolder(corePath, result);
            CreateSingleFolder($"{corePath}/ImportedAssets", result, corePath);

            // Move Settings folder
            string settingsSrc = "Assets/Settings";
            string settingsDst = $"{corePath}/Settings";
            if (AssetDatabase.IsValidFolder(settingsSrc) && !AssetDatabase.IsValidFolder(settingsDst))
            {
                string error = AssetDatabase.MoveAsset(settingsSrc, settingsDst);
                if (string.IsNullOrEmpty(error))
                    Debug.Log($"[ProjectSetup] Moved '{settingsSrc}' → '{settingsDst}'");
                else
                    result.errors.Add($"Could not move Settings folder: {error}");
            }

            // Move Input Actions asset
            string inputSrc = "Assets/InputSystem_Actions.inputactions";
            string inputDst = $"{corePath}/InputSystem_Actions.inputactions";
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(inputSrc)) && 
                string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(inputDst)))
            {
                string error = AssetDatabase.MoveAsset(inputSrc, inputDst);
                if (string.IsNullOrEmpty(error))
                    Debug.Log($"[ProjectSetup] Moved '{inputSrc}' → '{inputDst}'");
                else
                    result.errors.Add($"Could not move Input Actions: {error}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Scene folder migration
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Moves <c>Assets/Scenes</c> into the project root folder so Unity's default
        /// scene location is preserved rather than duplicated.
        /// <para>
        /// If a placeholder (empty) <c>Scenes</c> folder already exists at the destination
        /// from a previous run it is removed first, making the operation idempotent.
        /// </para>
        /// </summary>
        private static void MoveExistingScenesFolder(string rootPath, CreationResult result)
        {
            const string src = "Assets/Scenes";
            string dst = $"{rootPath}/Scenes";

            if (AssetDatabase.IsValidFolder(src))
            {
                // If destination exists but is empty, clear it so the move can proceed.
                if (AssetDatabase.IsValidFolder(dst) && IsFolderEmpty(dst))
                    AssetDatabase.DeleteAsset(dst);

                if (!AssetDatabase.IsValidFolder(dst))
                {
                    string error = AssetDatabase.MoveAsset(src, dst);
                    if (string.IsNullOrEmpty(error))
                        Debug.Log($"[ProjectSetup] Moved '{src}' → '{dst}'");
                    else
                        result.errors.Add($"Could not move Scenes folder: {error}");
                }
            }

            // Rename SampleScene → DefaultScene if it's present in the destination.
            string sampleScenePath = $"{dst}/SampleScene.unity";
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(sampleScenePath)))
            {
                AssetDatabase.RenameAsset(sampleScenePath, "DefaultScene");
                Debug.Log($"[ProjectSetup] Renamed 'SampleScene' to 'DefaultScene'");
            }
        }

        /// <summary>
        /// Returns <c>true</c> when a folder contains no real files
        /// (ignoring <c>.meta</c> and macOS <c>.DS_Store</c> files).
        /// </summary>
        private static bool IsFolderEmpty(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            foreach (var file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".meta") && !file.EndsWith(".DS_Store"))
                    return false;
            }
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Boilerplate cleanup helper
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to delete a Unity asset or folder at <paramref name="path"/>.
        /// Guards against stale GUIDs (from old <c>.meta</c> files) by
        /// verifying the physical path exists before calling <c>DeleteAsset</c>.
        /// </summary>
        private static void TryDelete(string path, CreationResult result)
        {
            // Guard 1: quick GUID check — returns early if Unity has no record at all.
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path))) return;

            // Guard 2: physical filesystem check — stale .meta files can leave a GUID
            // registered even after the actual asset/folder was deleted on a previous run.
            // If nothing exists on disk we skip silently rather than logging a spurious error.
            string fullPath = Path.GetFullPath(path);
            bool existsOnDisk = File.Exists(fullPath) || Directory.Exists(fullPath);
            if (!existsOnDisk) return;

            if (!AssetDatabase.DeleteAsset(path))
                result.errors.Add($"Could not delete: {path}");
            else
                Debug.Log($"[ProjectSetup] Removed boilerplate: {path}");
        }
    }
}