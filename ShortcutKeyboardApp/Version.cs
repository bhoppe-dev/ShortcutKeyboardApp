using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortcutKeyboardApp
{
    /// <summary>
    /// Provides version information for the application.
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// Gets the current version of the application.
        /// </summary>
        public const string Version = "1.1.1";

        /// <summary>
        /// Gets the current build type (beta, release, etc.).
        /// </summary>
        public const string BuildType = "beta";

        /// <summary>
        /// Gets the full version string including build type.
        /// </summary>
        public static string FullVersion => $"{Version}-{BuildType}";

        /// <summary>
        /// Gets the application title with version.
        /// </summary>
        public static string AppTitleWithVersion => $"Shortcut Keyboard V{FullVersion}";
    }
}