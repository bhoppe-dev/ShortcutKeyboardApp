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
                DarkModeEnabled = currentSettings.DarkModeEnabled
            };

            StartMinimizedCheckBox.IsChecked = Settings.StartMinimized;
            StartAsTrayIconCheckBox.IsChecked = Settings.StartAsTrayIcon;
            DarkModeCheckBox.IsChecked = Settings.DarkModeEnabled;

            UpdateCheckBoxStates();
        }

        private void StartMinimizedCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateCheckBoxStates();
        }

        private void StartAsTrayIconCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateCheckBoxStates();
        }

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

        private void DarkModeCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ApplyTheme(DarkModeCheckBox.IsChecked == true);
        }

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.StartMinimized = StartMinimizedCheckBox.IsChecked ?? false;
            Settings.StartAsTrayIcon = StartAsTrayIconCheckBox.IsChecked ?? false;
            Settings.DarkModeEnabled = DarkModeCheckBox.IsChecked ?? false;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore original settings
            ApplyTheme(originalSettings.DarkModeEnabled);
            DialogResult = false;
            Close();
        }
    }
}