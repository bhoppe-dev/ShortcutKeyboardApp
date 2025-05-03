using System.IO;
using System.Text.Json.Serialization;

namespace ShortcutKeyboardApp
{
    public class ButtonConfig
    {
        public string Action { get; set; }
        public bool RunAsAdmin { get; set; }
        public string DisplayText { get; set; }

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