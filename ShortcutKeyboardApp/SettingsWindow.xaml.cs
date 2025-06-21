using System.Windows;

namespace ShortcutKeyboardApp
{
    public partial class SettingsWindow : Window
    {
        public AppSettings Settings { get; private set; }
        private AppSettings originalSettings;

        public SettingsWindow(AppSettings currentSettings)
        {
            InitializeComponent();
            originalSettings = currentSettings;
            Settings = new AppSettings
            {
                StartMinimized = currentSettings.StartMinimized,
                StartAsTrayIcon = currentSettings.StartAsTrayIcon,
                DarkModeEnabled = currentSettings.DarkModeEnabled,
                MacroButtonCount = currentSettings.MacroButtonCount
            };

            StartMinimizedCheckBox.IsChecked = Settings.StartMinimized;
            StartAsTrayIconCheckBox.IsChecked = Settings.StartAsTrayIcon;
            DarkModeCheckBox.IsChecked = Settings.DarkModeEnabled;
            MacroButtonCountSlider.Value = Settings.MacroButtonCount;
            MacroButtonCountLabel.Text = Settings.MacroButtonCount.ToString();

            UpdateCheckBoxStates();
        }

        /// <summary>
        /// Handles the value changed event for the Macro Button Count slider.
        /// Updates the label to display the current value.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MacroButtonCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MacroButtonCountLabel != null)
            {
                MacroButtonCountLabel.Text = ((int)e.NewValue).ToString();
            }
        }
        /// <summary>
        /// Handles the check/uncheck event for the Start Minimized checkbox.
        /// Updates checkbox states to ensure mutual exclusivity with Start as Tray Icon option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void StartMinimizedCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateCheckBoxStates();
        }

        /// <summary>
        /// Handles the check/uncheck event for the Start as Tray Icon checkbox.
        /// Updates checkbox states to ensure mutual exclusivity with Start Minimized option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void StartAsTrayIconCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateCheckBoxStates();
        }

        /// <summary>
        /// Updates the enabled/disabled states of startup option checkboxes.
        /// Ensures that Start Minimized and Start as Tray Icon options are mutually exclusive.
        /// </summary>
        private void UpdateCheckBoxStates()
        {
            if (StartMinimizedCheckBox.IsChecked == true)
            {
                StartAsTrayIconCheckBox.IsEnabled = false;
                StartAsTrayIconCheckBox.IsChecked = false;
            }
            else if (StartAsTrayIconCheckBox.IsChecked == true)
            {
                StartMinimizedCheckBox.IsEnabled = false;
                StartMinimizedCheckBox.IsChecked = false;
            }
            else
            {
                StartMinimizedCheckBox.IsEnabled = true;
                StartAsTrayIconCheckBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the check/uncheck event for the Dark Mode checkbox.
        /// Immediately applies the selected theme to the application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void DarkModeCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ApplyTheme(DarkModeCheckBox.IsChecked == true);
        }

        /// <summary>
        /// Applies the specified theme to the application.
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
        /// Handles the Save button click event.
        /// Saves the current settings and closes the dialog with a positive result.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.StartMinimized = StartMinimizedCheckBox.IsChecked ?? false;
            Settings.StartAsTrayIcon = StartAsTrayIconCheckBox.IsChecked ?? false;
            Settings.DarkModeEnabled = DarkModeCheckBox.IsChecked ?? false;
            Settings.MacroButtonCount = (int)MacroButtonCountSlider.Value;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles the Cancel button click event.
        /// Restores the original theme and closes the dialog with a negative result.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore original settings
            ApplyTheme(originalSettings.DarkModeEnabled);
            DialogResult = false;
            Close();
        }
    }
}