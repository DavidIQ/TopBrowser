using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;

namespace TopBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            Topmost = false;
#endif
            Go.Visibility = Browser.Visibility = Visibility.Collapsed;
            TopDock.IsEnabled = false;
            Loader.Spin = true;
            Loader.Icon = FontAwesome.WPF.FontAwesomeIcon.CircleOutlineNotch;
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
                    Loader.Spin = false;
                    Loader.Icon = FontAwesome.WPF.FontAwesomeIcon.Adjust;
                }
            }
            catch (EdgeNotFoundException e)
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
            Loader.Spin = true;
            Loader.Icon = FontAwesome.WPF.FontAwesomeIcon.CircleOutlineNotch;

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
            Loader.Spin = false;
            GoBusy.Visibility = Loader.Visibility = Visibility.Collapsed;
            Go.Visibility = Browser.Visibility = Visibility.Visible;
            TopDock.IsEnabled = true;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }

        private void TopDock_Toggle(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(HideTopDock))
            {
                WindowStyle = WindowStyle.None;
                TopDock.Visibility = Visibility.Collapsed;
                ShowTop.IsOpen = true;
                MainDock.IsEnabled = WindowState != WindowState.Maximized; // Allows window to be dragged
            }
            else
            {
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
            // "Pulsing" the HorizontalOffset prop keeps the popup control attached to window
            ShowTop.HorizontalOffset += 1;

            // When window is maximized popup is lost so let's prevent that
            if (WindowState == WindowState.Maximized)
            {
                ShowTop.HorizontalOffset = 0;
                ShowTop.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;
            }
            else
            {
                ShowTop.HorizontalOffset = ShowTopDock.Width;
                ShowTop.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
            }
        }
    }
}
