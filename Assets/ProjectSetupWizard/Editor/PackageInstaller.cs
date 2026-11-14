using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;

namespace ProjectSetupWizard
{
    /// <summary>
    /// Queues and installs Unity packages via the Package Manager Client API.
    /// Packages are installed sequentially because <c>Client.Add</c> only
    /// supports one in-flight request at a time. Progress is logged to the
    /// Unity console and polled through <c>EditorApplication.update</c>.
    /// </summary>
    public static class PackageInstaller
    {
        // ─────────────────────────────────────────────────────────────────────
        // Package catalog
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Represents a single installable package entry.</summary>
        public class PackageEntry
        {
            /// <summary>Human-readable name shown in the wizard UI.</summary>
            public string displayName;

            /// <summary>Official com.* package identifier used by the Package Manager.</summary>
            public string packageId;

            /// <summary>
            /// When true the package is pre-checked as an essential default.
            /// The user can still uncheck it before running setup.
            /// </summary>
            public bool essential;

            /// <summary>Current toggle state in the wizard UI.</summary>
            public bool enabled;

            public PackageEntry(string displayName, string packageId, bool essential)
            {
                this.displayName = displayName;
                this.packageId   = packageId;
                this.essential   = essential;
                this.enabled     = essential; // pre-check essentials
            }
        }

        /// <summary>
        /// The full list of packages the wizard can offer.
        /// Cinemachine and ProBuilder are marked essential and checked by default.
        /// </summary>
        public static readonly List<PackageEntry> Catalog = new List<PackageEntry>
        {
            new PackageEntry("Cinemachine",     "com.unity.cinemachine",                essential: true),
            new PackageEntry("ProBuilder",      "com.unity.probuilder",                 essential: true),
            new PackageEntry("Addressables",    "com.unity.addressables",               essential: false),
            new PackageEntry("Newtonsoft JSON", "com.unity.nuget.newtonsoft-json",      essential: false),
        };

        // ─────────────────────────────────────────────────────────────────────
        // Installation result + state
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Aggregates the outcome of a <see cref="InstallPackages"/> run.</summary>
        public class InstallResult
        {
            /// <summary>Package IDs that were freshly installed this run.</summary>
            public List<string> installedPackages = new List<string>();

            /// <summary>Package IDs that were already present and skipped.</summary>
            public List<string> skippedPackages   = new List<string>();

            /// <summary>Human-readable error messages, if any.</summary>
            public List<string> errors            = new List<string>();

            /// <summary>Total number of packages requested for installation.</summary>
            public int targetCount;

            public bool HasErrors => errors.Count > 0;
        }

        /// <summary>The result of the most recent <see cref="InstallPackages"/> call. Null until first run.</summary>
        public static InstallResult LastResult { get; private set; }

        private static Queue<string> _queue;
        private static ListRequest   _listRequest;
        private static AddRequest    _currentRequest;
        private static string        _currentId;    // package ID of the in-flight request

        /// <summary>
        /// True while a package list check or individual install is active in the background.
        /// The wizard UI polls this to swap between the catalog list and a progress message.
        /// </summary>
        public static bool IsInstalling => _listRequest != null || _currentRequest != null || (_queue != null && _queue.Count > 0);

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Begins installing the supplied package IDs. Safe to call with an
        /// empty collection — returns immediately without logging.
        /// </summary>
        public static void InstallPackages(IEnumerable<string> packageIds)
        {
            _queue = new Queue<string>(packageIds);

            LastResult = new InstallResult { targetCount = _queue.Count };

            if (LastResult.targetCount == 0) return;

            Debug.Log($"[ProjectSetup] Checking {LastResult.targetCount} package(s) against local cache...");
            _listRequest = Client.List(offlineMode: true, includeIndirectDependencies: false);
            EditorApplication.update += Poll;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Kicks off the next install in the queue.</summary>
        private static void InstallNext()
        {
            if (_queue.Count == 0)
            {
                EditorApplication.update -= Poll;
                if (LastResult.skippedPackages.Count > 0 && LastResult.installedPackages.Count == 0)
                    Debug.Log($"[ProjectSetup] Package workflow complete — all packages already installed.");
                else
                    Debug.Log($"[ProjectSetup] Package workflow complete — {LastResult.installedPackages.Count} installed, {LastResult.skippedPackages.Count} skipped.");
                return;
            }

            _currentId      = _queue.Dequeue();
            Debug.Log($"[ProjectSetup] Installing '{_currentId}'...");
            _currentRequest = Client.Add(_currentId);
        }

        /// <summary>
        /// Polled every editor frame. Checks whether the current request has
        /// completed and chains to the next package in the queue.
        /// </summary>
        private static void Poll()
        {
            // Phase 1: Wait for package list and filter out already-installed ones
            if (_listRequest != null)
            {
                if (!_listRequest.IsCompleted) return;

                if (_listRequest.Status == StatusCode.Success)
                {
                    var filteredQueue = new Queue<string>();
                    foreach (string pkgId in _queue)
                    {
                        bool installed = false;
                        foreach (var existing in _listRequest.Result)
                        {
                            if (existing.name == pkgId)
                            {
                                installed = true; 
                                break;
                            }
                        }

                        if (installed)
                        {
                            LastResult.skippedPackages.Add(pkgId);
                            Debug.Log($"[ProjectSetup] ⏭ '{pkgId}' already installed");
                        }
                        else
                        {
                            filteredQueue.Enqueue(pkgId);
                        }
                    }
                    _queue = filteredQueue;
                }
                else
                {
                    LastResult.errors.Add($"Failed to cross-reference packages: {_listRequest.Error?.message}");
                    Debug.LogWarning($"[ProjectSetup] Failed to cross-reference existing packages. Attempting normal installation.");
                }

                _listRequest = null;
                InstallNext();
                return;
            }

            // Phase 2: Process individual package additions
            if (_currentRequest == null || !_currentRequest.IsCompleted) return;

            if (_currentRequest.Status == StatusCode.Success)
            {
                LastResult.installedPackages.Add(_currentRequest.Result.packageId);
                int target = LastResult.targetCount - LastResult.skippedPackages.Count;
                Debug.Log($"[ProjectSetup] ✓ '{_currentRequest.Result.displayName}' installed  ({LastResult.installedPackages.Count}/{target})");
            }
            else if (_currentRequest.Status >= StatusCode.Failure)
            {
                LastResult.errors.Add($"Failed to install '{_currentId}': {_currentRequest.Error?.message}");
                Debug.LogError(
                    $"[ProjectSetup] ✗ Failed to install '{_currentId}': " +
                    $"{_currentRequest.Error?.message}");
            }

            _currentRequest = null;
            InstallNext();
        }
    }
}
