using System.IO;
using System.Text.Json.Serialization;

namespace ShortcutKeyboardApp
{
    public class ButtonConfig
    {
        public string Action { get; set; }
        public bool RunAsAdmin { get; set; }
        public string DisplayText { get; set; }

        /// <summary>
        /// Initializes a new instance of the ButtonConfig class with the specified parameters.
        /// </summary>
        /// <param name="action">The action associated with the button (path or URL).</param>
        /// <param name="runAsAdmin">Whether to run the action with administrator privileges.</param>
        /// <param name="displayText">Optional custom display text. Defaults to filename if null.</param>
        public ButtonConfig(string action, bool runAsAdmin, string displayText = null)
        {
            Action = action;
            RunAsAdmin = runAsAdmin;
            DisplayText = displayText ?? Path.GetFileName(action);
        }

        [JsonConstructor]
        public ButtonConfig() { }
    }
}