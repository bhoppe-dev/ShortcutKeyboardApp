using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;

namespace ShortcutKeyboardApp
{
    public partial class ConfigurationWindow : Window
    {
        public string SelectedPath { get; private set; }
        public bool RunAsAdmin { get; private set; }
        public bool IsWebLink { get; private set; }

        public string DisplayText { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ConfigurationWindow class.
        /// </summary>
        /// <param name="currentPath">The current path or URL configured for the button.</param>
        /// <param name="currentRunAsAdmin">Whether the current action runs as administrator.</param>
        /// <param name="currentDisplayText">The current display text for the button.</param>
        /// <param name="isDarkMode">Whether dark mode is currently enabled.</param>
        public ConfigurationWindow(string currentPath, bool currentRunAsAdmin, string currentDisplayText, bool isDarkMode)
        {
            InitializeComponent();
            PathTextBox.Text = currentPath;
            DisplayTextBox.Text = currentDisplayText;
            AdminCheckBox.IsChecked = currentRunAsAdmin;

            // Determine if the current path is a web link
            if (currentPath != null && (currentPath.StartsWith("http://") || currentPath.StartsWith("https://")))
            {
                RadioLink.IsChecked = true;
                BrowseButton.IsEnabled = false;
                AdminCheckBox.IsEnabled = false;
            }

            // Add event handlers for radio buttons
            RadioLink.Checked += RadioButton_Checked;
            RadioFile.Checked += RadioButton_Checked;
            RadioFolder.Checked += RadioButton_Checked;
            RadioExecutable.Checked += RadioButton_Checked;

            // Apply the current theme
            ApplyTheme(isDarkMode);
        }

        /// <summary>
        /// Handles the checked event for action type radio buttons.
        /// Updates UI elements based on the selected action type.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (BrowseButton != null && AdminCheckBox != null) // Check if controls are initialized
            {
                bool isLink = RadioLink.IsChecked ?? false;
                BrowseButton.IsEnabled = !isLink;
                AdminCheckBox.IsEnabled = !isLink;

                if (isLink)
                {
                    AdminCheckBox.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// Applies the specified theme to the window.
        /// </summary>
        /// <param name="isDarkMode">True to apply dark theme, false to apply light theme.</param>
        private void ApplyTheme(bool isDarkMode)
        {
            // Get the current application
            Application app = Application.Current;

            // Clear existing resource dictionaries
            app.Resources.MergedDictionaries.Clear();

            // Create new resource dictionary
            ResourceDictionary resourceDict = new ResourceDictionary();

            // Set the source of the resource dictionary based on the theme
            resourceDict.Source = new Uri(isDarkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml", UriKind.Relative);

            // Add the new dictionary to the application
            app.Resources.MergedDictionaries.Add(resourceDict);
        }

        /// <summary>
        /// Handles the Browse button click event.
        /// Opens appropriate dialog based on selected action type (file, executable, or folder).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadioFile.IsChecked == true || RadioExecutable.IsChecked == true)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (RadioExecutable.IsChecked == true)
                {
                    openFileDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                }
                if (openFileDialog.ShowDialog() == true)
                {
                    PathTextBox.Text = openFileDialog.FileName;
                }
            }
            else if (RadioFolder.IsChecked == true)
            {
                var folderDialog = new CommonOpenFileDialog();
                folderDialog.IsFolderPicker = true;
                if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    PathTextBox.Text = folderDialog.FileName;
                }
            }
        }

        /// <summary>
        /// Handles the OK button click event.
        /// Validates the input and sets the dialog result properties.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadioLink.IsChecked == true)
            {
                string url = PathTextBox.Text.Trim();
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                // Basic URL validation
                if (!IsValidUrl(url))
                {
                    MessageBox.Show("Please enter a valid URL.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SelectedPath = url;
                IsWebLink = true;
                RunAsAdmin = false;
            }
            else
            {
                SelectedPath = PathTextBox.Text;
                IsWebLink = false;
                RunAsAdmin = AdminCheckBox.IsChecked ?? false;
            }
            if (!string.IsNullOrWhiteSpace(DisplayTextBox.Text))
            {
                DisplayText = DisplayTextBox.Text;
            }
            else
            {
                // If it's a web link, use a friendly name
                if (IsWebLink)
                {
                    try
                    {
                        Uri uri = new Uri(SelectedPath);
                        DisplayText = uri.Host.Replace("www.", "").Split('.')[0];  // e.g., "google" from "www.google.com"
                                                                                   // Capitalize first letter
                        if (!string.IsNullOrEmpty(DisplayText))
                        {
                            DisplayText = char.ToUpper(DisplayText[0]) + DisplayText.Substring(1);
                        }
                    }
                    catch
                    {
                        DisplayText = "Web Link";  // Fallback if URL parsing fails
                    }
                }
                else
                {
                    // For files/folders, use filename
                    DisplayText = Path.GetFileName(SelectedPath);
                }
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Validates whether a string is a valid URL.
        /// </summary>
        /// <param name="url">The URL string to validate.</param>
        /// <returns>True if the URL is valid, false otherwise.</returns>
        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Handles the Cancel button click event.
        /// Closes the dialog without saving changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}