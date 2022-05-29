﻿using System;
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
        }
        public MainWindow()
        {
            InitializeComponent();
            Events();
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            Left = desktopWorkingArea.Left + 26;
            Top = desktopWorkingArea.Bottom - 260;
        }
        public void Events()
        {
            MoreButton.Click += MoreButton_Click;
            CancelButton.Click += CancelButton_Click;
            BrowseButton.Click += BrowseButton_Click;
            OKButton.Click += OKButton_Click;
            PathTextBox.TextChanged += PathTextBox_TextChanged;
            Closing += MainWindow_Closing;
            Activated += MainWindow_Activated;
            KeyDown += MainWindow_KeyDown;
            PathTextBox.KeyDown += PathTextBox_KeyDown;
        }

        private void PathTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if (e.Key == Key.Enter)
            {
                OKButton_Click(null, null);
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
                program = input.Substring(0, input.IndexOf(" "));
            }
            catch
            {

            }
            try
            {
                arguments = input.Substring(input.IndexOf(" "));
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
            openFileDialog.Title = "Choose file";
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
    }
}