using ModernWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BetterRun
{
    /// <summary>
    /// Logique d'interaction pour ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : Window
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
        }
        public ErrorDialog()
        {
            InitializeComponent();
            OKButton.Click += OKButton_Click;
            KeyDown += ErrorDialog_KeyDown;
            Translate();
        }

        private void ErrorDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Translate()
        {
            if (System.Globalization.CultureInfo.CurrentUICulture.Name.Contains("fr"))
            {
                // French
                WindowTitle.Text = "Erreur";
                Message.Text = "Quelque chose a empêché l'ouverture du programme ou du fichier. Il peut s'agir d'un manque d'autorisations, d'un fichier inexistant ou du fichier qui est actuellement utilisé.";
            }
        }
    }
}
