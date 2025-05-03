using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms; // For NotifyIcon
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ShortcutKeyboardApp
{
    public partial class MainWindow : Window
    {
        private const string CONFIG_FILE_NAME = "button_config_{0}.json";
        private const string SETTINGS_FILE_NAME = "app_settings.json";
        private List<ButtonConfig> buttonConfigs = new List<ButtonConfig>();
        private int currentProfile = 0;
        private AppSettings appSettings;
        private NotifyIcon notifyIcon = null!;
        private bool shouldMinimizeToTray = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = AppVersion.AppTitleWithVersion;
            try
            {
                this.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/ShortcutKeyboard.ico"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not load icon: {ex.Message}");
            }
            LoadAppSettings();
            ApplyTheme(appSettings.DarkModeEnabled);
            if (ProfileComboBox != null)
            {
                ProfileComboBox.SelectedIndex = 0;
            }
            LoadButtonConfigurations();

            Debug.WriteLine("Setting up keyboard hook");
            KeyboardHook.MacroKeyPressed += OnMacroKeyPressed;
            KeyboardHook.SetHook();
            Debug.WriteLine("Keyboard hook set up completed");

            SetupNotifyIcon();
            ApplyAppSettings();

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SetupNotifyIcon()
        {
            System.Drawing.Icon customIcon = null;

            try
            {
                string[] possiblePaths = new string[]
                {
                    "Resources/ShortcutKeyboard_Tray.ico",
                    "/Resources/ShortcutKeyboard_Tray.ico",
                    "ShortcutKeyboard_Tray.ico",
                    "/ShortcutKeyboard_Tray.ico",
                    "pack://application:,,,/Resources/ShortcutKeyboard_Tray.ico"
                };

                foreach (string path in possiblePaths)
                {
                    try
                    {
                        var uri = new Uri(path, UriKind.Relative);
                        var resourceStream = System.Windows.Application.GetResourceStream(uri)?.Stream;
                        if (resourceStream != null)
                        {
                            customIcon = new System.Drawing.Icon(resourceStream);
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (customIcon == null)
                {
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string exeDir = Path.GetDirectoryName(exePath);
                    string iconPath = Path.Combine(exeDir, "ShortcutKeyboard_Tray.ico");

                    if (File.Exists(iconPath))
                    {
                        customIcon = new System.Drawing.Icon(iconPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading custom icon: {ex.Message}");
            }

            if (customIcon == null)
            {
                customIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }

            notifyIcon = new NotifyIcon
            {
                Icon = customIcon,
                Visible = false,
                Text = "Shortcut Keyboard"
            };
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Open", null, (s, e) => { Show(); WindowState = WindowState.Normal; notifyIcon.Visible = false; });
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => { Close(); });
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void ApplyAppSettings()
        {
            if (appSettings.StartAsTrayIcon)
            {
                Hide();
                notifyIcon.Visible = true;
            }
            else if (appSettings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void LoadAppSettings()
        {
            if (File.Exists(SETTINGS_FILE_NAME))
            {
                string json = File.ReadAllText(SETTINGS_FILE_NAME);
                appSettings = JsonSerializer.Deserialize<AppSettings>(json);
            }
            else
            {
                appSettings = new AppSettings();
            }
        }

        private void SaveAppSettings()
        {
            string json = JsonSerializer.Serialize(appSettings);
            File.WriteAllText(SETTINGS_FILE_NAME, json);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(appSettings)
            {
                Owner = this
            };
            if (settingsWindow.ShowDialog() == true)
            {
                appSettings = settingsWindow.Settings;
                SaveAppSettings();
                ApplyAppSettings();
                ApplyTheme(appSettings.DarkModeEnabled);
            }
        }

        private void ApplyTheme(bool isDarkMode)
        {
            System.Windows.Application app = System.Windows.Application.Current;
            app.Resources.MergedDictionaries.Clear();
            ResourceDictionary resourceDict = new ResourceDictionary();
            resourceDict.Source = new Uri(isDarkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml", UriKind.Relative);
            app.Resources.MergedDictionaries.Add(resourceDict);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (appSettings.StartAsTrayIcon || shouldMinimizeToTray)
                {
                    Hide();
                    notifyIcon.Visible = true;
                    shouldMinimizeToTray = false;
                }

            }
            else if (WindowState == WindowState.Normal)
            {
                Show();
                notifyIcon.Visible = false;
            }
            base.OnStateChanged(e);
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                shouldMinimizeToTray = true;
                WindowState = WindowState.Minimized;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            notifyIcon.Dispose();
            base.OnClosing(e);
        }

        private void MinimizeToTray()
        {
            shouldMinimizeToTray = true;
            WindowState = WindowState.Minimized;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0112 && wParam.ToInt32() == 0xF020) // WM_SYSCOMMAND and SC_MINIMIZE
            {
                shouldMinimizeToTray = appSettings.StartAsTrayIcon;
            }
            return IntPtr.Zero;
        }

        private void LoadButtonConfigurations()
        {
            string fileName = string.Format(CONFIG_FILE_NAME, currentProfile + 1);
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                buttonConfigs = JsonSerializer.Deserialize<List<ButtonConfig>>(json);
            }
            else
            {
                buttonConfigs = new List<ButtonConfig>();
            }

            while (buttonConfigs.Count < 8)
            {
                buttonConfigs.Add(new ButtonConfig("NULL", false));
            }

            for (int i = 0; i < buttonConfigs.Count; i++)
            {
                UpdateButtonText(i + 1, buttonConfigs[i].DisplayText ?? Path.GetFileName(buttonConfigs[i].Action));
            }
        }

        private void SaveButtonConfigurations()
        {
            string fileName = string.Format(CONFIG_FILE_NAME, currentProfile + 1);
            string json = JsonSerializer.Serialize(buttonConfigs);
            File.WriteAllText(fileName, json);
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (buttonConfigs.Count > 0)
            {
                SaveButtonConfigurations();
            }

            currentProfile = ProfileComboBox.SelectedIndex;
            LoadButtonConfigurations();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button clickedButton = sender as System.Windows.Controls.Button;
            int buttonIndex = int.Parse(((TextBlock)((StackPanel)clickedButton.Content).Children[0]).Text) - 1;

            ConfigureButtonAction(buttonIndex);
        }

        private void ConfigureButtonAction(int buttonIndex)
        {
            ConfigurationWindow configWindow = new ConfigurationWindow(
                buttonConfigs[buttonIndex].Action,
                buttonConfigs[buttonIndex].RunAsAdmin,
                buttonConfigs[buttonIndex].DisplayText ?? Path.GetFileName(buttonConfigs[buttonIndex].Action),
                appSettings.DarkModeEnabled)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (configWindow.ShowDialog() == true)
            {
                buttonConfigs[buttonIndex] = new ButtonConfig(
                    configWindow.SelectedPath,
                    configWindow.RunAsAdmin,
                    configWindow.DisplayText);
                UpdateButtonText(buttonIndex + 1, configWindow.DisplayText);
                SaveButtonConfigurations();
            }
        }

        private void UpdateButtonText(int buttonNumber, string text)
        {
            TextBlock textBlock = (TextBlock)this.FindName($"ButtonText{buttonNumber}");
            if (textBlock != null)
            {
                textBlock.Text = text;
            }
        }

        private void OnMacroKeyPressed(object? sender, MacroKeyPressedEventArgs e)
        {
            Debug.WriteLine($"Macro key pressed: Index = {e.KeyIndex}, Corresponding to key {(char)(e.KeyIndex + 65)}");
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine($"Executing action for button {e.KeyIndex + 1} in the UI");
                ExecuteButtonAction(e.KeyIndex);
            });
        }

        public async void ExecuteButtonAction(int buttonIndex)
        {
            Debug.WriteLine($"ExecuteButtonAction called for button {buttonIndex}");
            if (buttonIndex >= 0 && buttonIndex < buttonConfigs.Count)
            {
                ButtonConfig config = buttonConfigs[buttonIndex];
                Debug.WriteLine($"Action for button {buttonIndex}: {config.Action}, Run as Admin: {config.RunAsAdmin}");
                if (!string.IsNullOrEmpty(config.Action) && config.Action != "NULL")
                {
                    try
                    {
                        Debug.WriteLine($"Attempting to execute action: {config.Action}");

                        // Check if it's a web link
                        if (config.Action.StartsWith("http://") || config.Action.StartsWith("https://"))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = config.Action,
                                UseShellExecute = true
                            });
                        }
                        else if (Directory.Exists(config.Action))
                        {
                            Process.Start("explorer.exe", config.Action);
                        }
                        else if (File.Exists(config.Action))
                        {
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = config.Action,
                                UseShellExecute = true
                            };
                            if (config.RunAsAdmin)
                            {
                                startInfo.Verb = "runas";
                            }
                            await Task.Run(() => Process.Start(startInfo));
                        }
                        else
                        {
                            System.Windows.MessageBox.Show($"The file, folder, or URL does not exist: {config.Action}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        Debug.WriteLine("Action executed successfully");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error executing action: {ex.Message}");
                        System.Windows.MessageBox.Show($"Error executing action: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Debug.WriteLine($"No action set for button {buttonIndex}");
                }
            }
            else
            {
                Debug.WriteLine($"Invalid button index: {buttonIndex}");
            }
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to delete the current profile?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                string fileName = string.Format(CONFIG_FILE_NAME, currentProfile + 1);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                buttonConfigs.Clear();
                LoadButtonConfigurations();
            }
        }
    }

    public class AppSettings
    {
        public bool StartMinimized { get; set; }
        public bool StartAsTrayIcon { get; set; }
        public bool DarkModeEnabled { get; set; }
    }
}