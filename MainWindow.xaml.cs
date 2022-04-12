using System;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace TopBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool SpinLoader { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            Topmost = false;
#endif
            Go.Visibility = Browser.Visibility = Visibility.Collapsed;
            TopDock.IsEnabled = false;
            SpinLoader = true;
            Loader.Icon = FontAwesome.Sharp.IconChar.CircleNotch;
            InitializeAsync();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            ShowTop_Opened(null, e);
            base.OnLocationChanged(e);
        }

        async void InitializeAsync()
        {
            try
            {
                await Browser.EnsureCoreWebView2Async();
                if (!string.IsNullOrWhiteSpace(Url.Text))
                {
                    Go_Click(null, null);
                }
                else
                {
                    GoBusy.Visibility = Visibility.Collapsed;
                    Go.Visibility = Visibility.Visible;
                    TopDock.IsEnabled = true;
                    SpinLoader = false;
                    Loader.Icon = FontAwesome.Sharp.IconChar.Adjust;
                }
                DockToggle(Properties.Settings.Default.DockCollapsed);
            }
            catch (WebView2RuntimeNotFoundException e)
            {
                MessageBox.Show("You need Edge Chromium Dev or Canary channel to use this application.", "WebView2 requirement check", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(e.HResult);
            }
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Url.Text))
            {
                return;
            }

            if (!Url.Text.StartsWith("http"))
            {
                Url.Text = "http://" + Url.Text;
            }

            bool result = Uri.TryCreate(Url.Text, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
            {
                MessageBox.Show("Invalid web URL entered.", "URL invalid", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Browser.Visibility = Visibility.Collapsed;
            TopDock.IsEnabled = false;
            Loader.Visibility = Visibility.Visible;
            SpinLoader = true;
            Loader.Icon = FontAwesome.Sharp.IconChar.CircleNotch;

            Browser.CoreWebView2.Navigate(uriResult.ToString());
        }

        private void Browser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Url.Text = e.Uri.ToString();
            GoBusy.Visibility = Visibility.Visible;
            Go.Visibility = Visibility.Collapsed;
        }

        private void Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            SpinLoader = false;
            GoBusy.Visibility = Loader.Visibility = Visibility.Collapsed;
            Go.Visibility = Browser.Visibility = Visibility.Visible;
            TopDock.IsEnabled = true;
        }

        /// <summary>
        /// Allows window to be dragged by controls and not just by title bar
        /// </summary>
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }

        private void TopDock_Toggle(object sender, RoutedEventArgs e)
        {
            DockToggle(sender.Equals(HideTopDock));
        }

        private void DockToggle(bool collapseDock)
        {
            if (collapseDock)
            {
                Properties.Settings.Default.DockCollapsed = true;
                WindowStyle = WindowStyle.None;
                TopDock.Visibility = Visibility.Collapsed;
                ShowTop.IsOpen = true;
                MainDock.IsEnabled = WindowState == WindowState.Maximized; // Allows window to be dragged
            }
            else
            {
                Properties.Settings.Default.DockCollapsed = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
                TopDock.Visibility = Visibility.Visible;
                ShowTop.IsOpen = false;
                MainDock.IsEnabled = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void ShowTop_Opened(object sender, EventArgs e)
        {
            // "Pulsing" the HorizontalOffset prop keeps the popup control where it needs to be in the window
            ShowTop.HorizontalOffset += 1;
            ShowTop.HorizontalOffset -= 1;

            // Button acts strangely when window is partially offscreen so just close it momentarily
            ShowTop.IsOpen = TopDock.Visibility == Visibility.Collapsed && Left > 0 && Top > 0;
        }
    }
}
