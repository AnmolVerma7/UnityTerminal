using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectSetupWizard
{
    /// <summary>
    /// ScriptableObject that defines the folder structure for a Unity project.
    /// Create different configs for different project types (2D, 3D, VR, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectFolderConfig", menuName = "Project Setup/Folder Config")]
    public class ProjectFolderConfig : ScriptableObject
    {
        [Header("Root Settings")]
        [Tooltip("The root folder name inside Assets (e.g., 'Project' creates Assets/Project)")]
        public string rootFolderName = "Project";

        [Header("Folder Structure")]
        [Tooltip("Define all folders to create. Supports nested paths like 'Art/Materials'")]
        public List<FolderDefinition> folders = new List<FolderDefinition>();

        /// <summary>
        /// Creates a default configuration with common Unity project folders.
        /// </summary>
        public void SetupDefaultFolders()
        {
            folders.Clear();
            
            // Art & Visual Assets
            folders.Add(new FolderDefinition("Art", "All visual assets"));
            folders.Add(new FolderDefinition("Art/Materials", "Material assets"));
            folders.Add(new FolderDefinition("Art/Textures", "Texture files (PNG, JPG, PSD, etc.)"));
            folders.Add(new FolderDefinition("Art/Models", "3D models (FBX, OBJ, etc.)"));
            folders.Add(new FolderDefinition("Art/Sprites", "2D sprite assets"));
            folders.Add(new FolderDefinition("Art/Animations", "Animation clips and controllers"));
            folders.Add(new FolderDefinition("Art/Shaders", "Custom shaders and shader graphs"));
            folders.Add(new FolderDefinition("Art/UI", "UI sprites, icons, and textures"));
            folders.Add(new FolderDefinition("Art/Fonts", "Font assets"));
            folders.Add(new FolderDefinition("Art/VFX", "Visual effects and particle systems"));
            
            // Audio
            folders.Add(new FolderDefinition("Audio", "All audio assets"));
            folders.Add(new FolderDefinition("Audio/Music", "Background music tracks"));
            folders.Add(new FolderDefinition("Audio/SFX", "Sound effects"));
            folders.Add(new FolderDefinition("Audio/Ambience", "Ambient and environmental sounds"));
            folders.Add(new FolderDefinition("Audio/Voice", "Voice over and dialogue"));
            
            // Code
            folders.Add(new FolderDefinition("Scripts", "All C# scripts"));
            folders.Add(new FolderDefinition("Scripts/Runtime", "Runtime game scripts"));
            folders.Add(new FolderDefinition("Scripts/Runtime/Core", "Core systems and managers"));
            folders.Add(new FolderDefinition("Scripts/Runtime/Gameplay", "Gameplay-specific scripts"));
            folders.Add(new FolderDefinition("Scripts/Runtime/UI", "UI scripts and controllers"));
            folders.Add(new FolderDefinition("Scripts/Runtime/Utilities", "Helper and utility scripts"));
            folders.Add(new FolderDefinition("Scripts/Editor", "Editor-only scripts"));
            
            // Prefabs
            folders.Add(new FolderDefinition("Prefabs", "All prefab assets"));
            folders.Add(new FolderDefinition("Prefabs/Characters", "Character prefabs"));
            folders.Add(new FolderDefinition("Prefabs/Environment", "Environment and level prefabs"));
            folders.Add(new FolderDefinition("Prefabs/UI", "UI prefabs"));
            folders.Add(new FolderDefinition("Prefabs/Props", "Props and interactive objects"));
            folders.Add(new FolderDefinition("Prefabs/VFX", "VFX prefabs"));
            
            // Scenes
            folders.Add(new FolderDefinition("Scenes", "Scene files"));
            folders.Add(new FolderDefinition("Scenes/Levels", "Game levels"));
            folders.Add(new FolderDefinition("Scenes/Testing", "Test and debug scenes"));
            
            // Data & Configuration
            folders.Add(new FolderDefinition("Data", "ScriptableObjects and data assets"));
            folders.Add(new FolderDefinition("Data/Configs", "Configuration assets"));
            folders.Add(new FolderDefinition("Data/Localization", "Localization data"));
            
            // Resources (use sparingly)
            folders.Add(new FolderDefinition("Resources", "Assets loaded via Resources.Load"));
            
            // Plugins & Third-Party
            folders.Add(new FolderDefinition("Plugins", "Third-party plugins and packages"));
            
            // Documentation
            folders.Add(new FolderDefinition("Documentation", "Project documentation"));
        }
    }

    /// <summary>
    /// Defines a single folder with optional description.
    /// </summary>
    [Serializable]
    public class FolderDefinition
    {
        [Tooltip("Folder path relative to root (e.g., 'Art/Materials')")]
        public string path;
        
        [Tooltip("Optional description for this folder's purpose")]
        public string description;
        
        [Tooltip("Whether this folder should be created")]
        public bool enabled = true;

        public FolderDefinition() { }

        public FolderDefinition(string path, string description = "")
        {
            this.path = path;
            this.description = description;
            this.enabled = true;
        }
    }
}
