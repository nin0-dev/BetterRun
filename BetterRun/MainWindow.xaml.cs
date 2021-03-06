using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ModernWpf;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Net.Http;
using System.Net;

namespace BetterRun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string KeyName = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);
        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029
        }
        public static void EnableMica(HwndSource source, bool darkThemeEnabled)
        {
            int trueValue = 0x01;
            int falseValue = 0x00;

            // Set dark mode before applying the material, otherwise you'll get an ugly flash when displaying the window.
            if (darkThemeEnabled)
                DwmSetWindowAttribute(source.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            else
                DwmSetWindowAttribute(source.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseValue, Marshal.SizeOf(typeof(int)));

            DwmSetWindowAttribute(source.Handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
        }
        public static void UpdateStyleAttributes(HwndSource hwnd)
        {
            int lightThemeEnabled = (int)Microsoft.Win32.Registry.GetValue(KeyName, "AppsUseLightTheme", 1);
            if (lightThemeEnabled == 1)
            {
                EnableMica(hwnd, false);
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            if (lightThemeEnabled == 0)
            {
                EnableMica(hwnd, true);
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }           
            ThemeManager.Current.AccentColor = SystemParameters.WindowGlassColor;
        }
        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            // Apply Mica brush and ImmersiveDarkMode if needed
            UpdateStyleAttributes((HwndSource)sender);

            // Hook to Windows theme change to reapply the brushes when needed
            ModernWpf.ThemeManager.Current.ActualApplicationThemeChanged += (s, ev) => UpdateStyleAttributes((HwndSource)sender);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get PresentationSource
            PresentationSource presentationSource = PresentationSource.FromVisual((Visual)sender);
            
            
            // Subscribe to PresentationSource's ContentRendered event
            presentationSource.ContentRendered += Window_ContentRendered;
            PathTextBox.Focus();
            CancelButton_Click(null, null);
            UpdateCheck();
        }
        public MainWindow()
        {
            InitializeComponent();
            Events();
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            Left = desktopWorkingArea.Left + 26;
            Top = desktopWorkingArea.Bottom - 260;
            UpdateCheck();
            Translate();
        }
        public void Events()
        {
            MoreButton.Click += MoreButton_Click;
            CancelButton.Click += CancelButton_Click;
            BrowseButton.Click += BrowseButton_Click;
            AboutContextItem.Click += AboutContextItem_Click;
            UpdateContextItem.Click += UpdateContextItem_Click;
            OKButton.Click += OKButton_Click;
            PathTextBox.TextChanged += PathTextBox_TextChanged;
            Closing += MainWindow_Closing;
            Activated += MainWindow_Activated;
            KeyDown += MainWindow_KeyDown;
            PathTextBox.KeyDown += PathTextBox_KeyDown;
        }

        private void UpdateContextItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(new Uri(@"https://github.com/nin0-dev/BetterRun/raw/master/delete_this.exe"), Environment.GetEnvironmentVariable("USERPROFILE") + @"\delete_this.exe");
                Process.Start(Environment.GetEnvironmentVariable("USERPROFILE") + @"\delete_this.exe");
            }
            catch (Exception ex)
            {

            }
        }

        private void AboutContextItem_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

        private void PathTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if (e.Key == Key.Enter)
            {
                if(PathTextBox.Text != "")
                {
                    OKButton_Click(null, null);
                }
            }
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string input = PathTextBox.Text;
            string program = "";
            string arguments = "";
            try
            {
                if (input.Contains(" "))
                {
                    program = input.Substring(0, input.IndexOf(" "));
                }
                else
                {
                    program = input;
                }
            }
            catch
            {

            }
            try
            {
                if(input.Contains(" "))
                {
                    arguments = input.Substring(input.IndexOf(" "));
                }
            }
            catch
            {
                 
            }
            try
            {
                if ((bool)!AdminCheckbox.IsChecked)
                {
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(program)
                        {
                            UseShellExecute = true,
                            Arguments = arguments,
                            WorkingDirectory = Environment.GetEnvironmentVariable("USERPROFILE")
                        }
                    }.Start();
                    Visibility = Visibility.Hidden;
                    PathTextBox.Text = "";
                }
                if ((bool)AdminCheckbox.IsChecked)
                {
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(program)
                        {
                            UseShellExecute = true,
                            Arguments = arguments,
                            WorkingDirectory = Environment.GetEnvironmentVariable("USERPROFILE"),
                            Verb = "runas"
                        }
                    }.Start();
                    Visibility = Visibility.Hidden;
                    PathTextBox.Text = "";
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                ErrorDialog ed = new ErrorDialog();
                ed.ShowDialog();
            }
        
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            if(openFileDialog.FileName != "")
            {
                PathTextBox.Focus();
                PathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
            PathTextBox.Text = "";
            AdminCheckbox.IsChecked = false;
            PathTextBox.Focus();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }
        
    
        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(PathTextBox.Text == "")
            {
                OKButton.IsEnabled = false;
            }
            if (PathTextBox.Text != "")
            {
                OKButton.IsEnabled = true;
            }
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            MoreButton.ContextMenu.IsOpen = true;
        }
        private void UpdateCheck()
        {
            MoreContextMenu.Items.Remove(UpdateContextItem);
            MoreContextMenu.Items.Insert(0, UpdateContextItem);
            try
            {
                HttpClient client = new HttpClient();
                var stringTask = client.GetStringAsync(@"https://raw.githubusercontent.com/nin0-dev/BetterRun/master/ota_version");
                var msg = stringTask.Result;
                if (msg == "1.0\n")
                {
                    MoreContextMenu.Items.Remove(UpdateContextItem);
                }
            }
            catch (Exception ex)
            {
                MoreContextMenu.Items.Remove(UpdateContextItem);
            }
        }
        private void Translate()
        {
            if(System.Globalization.CultureInfo.CurrentUICulture.Name.Contains("fr"))
            {
                // French
                Title.Text = "Exécuter";
                GreetingText.Text = "Entrez le nom d'un programme, site Web ou fichier, et BetterRun l'ouvrira pour vous.";
                OpenText.Text = "Ouvrir:";
                AdminCheckbox.Content = "Lancer en tant qu'admin";
                BrowseButton.Content = "Parcourir...";
                CancelButton.Content = "Annuler";
                UpdateContextItem.Header = "\ue896  Mettre à jour";
                AboutContextItem.Header = "\uea1f  À propos";

            }
        }
    }
}
