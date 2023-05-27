using Microsoft.UI.Xaml;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT;

namespace App1
{
    public sealed partial class MainWindow : Window
    {
        private enum BackdropType
        {
            Micainfo,
            DesktopAcrylic,
            FallbackColor
        }

        private WindowsSystemDispatcherQueueHelper WSDQHelper { get; init; }
        private SystemBackdropConfiguration ConfigurationSource { get; set; }
        private DesktopAcrylicController AcrylicController { get; set; }
        private MicaController MicainfoController { get; set; }
        private BackdropType CurrentBackdropType { get; set; } = BackdropType.DesktopAcrylic; //コメントテスト

        public MainWindow()
        {
            InitializeComponent();

            ((FrameworkElement)this.Content).RequestedTheme = ElementTheme.Default;

            WSDQHelper = new WindowsSystemDispatcherQueueHelper();
            WSDQHelper.EnsureWindowsSystemDispatcherQueueController();

            SetBackdrop(CurrentBackdropType);
        }

        private void SetBackdrop(BackdropType type)
        {
            DisposeBackdropController();
            switch (type)
            {
                case BackdropType.Micainfo:
                    if (TrySetMicainfoBackdrop())
                    {
                        myButton.Content = "Mica";
                    }
                    else if(TrySetAcrylicBackdrop())
                    {
                        myButton.Content = "DesktopAcrylic (Fallback from Mica)";
                    }
                    else
                    {
                        // サンプルではマイカ→アクリル→単色の順にフォールバックしてるけど、そもそも論を考えるとそれで良いのかという気もする。
                        myButton.Content = "FallbackColor (Fallback from Mica, Acrylic)";
                    }
                    break;
                case BackdropType.DesktopAcrylic:
                    if (TrySetAcrylicBackdrop())
                    {
                        myButton.Content = "DesktopAcrylic";
                    }
                    else
                    {
                        myButton.Content = "FallbackColor (Fallback from Acrylic)";
                    }
                    break;
                case BackdropType.FallbackColor:
                    myButton.Content = "FallbackColor";
                    break;
            }
        }

        bool TrySetMicainfoBackdrop()
        {
            if (MicaController.IsSupported())
            {
                // Hooking up the policy object
                ConfigurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                ConfigurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                MicainfoController = new MicaController();

                // Enable the system backdrop.
                MicainfoController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                MicainfoController.SetSystemBackdropConfiguration(ConfigurationSource);
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }

        bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                // Hooking up the policy object
                ConfigurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                ConfigurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                AcrylicController = new DesktopAcrylicController();

                // Enable the system backdrop.
                AcrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                AcrylicController.SetSystemBackdropConfiguration(ConfigurationSource);
                return true; // succeeded
            }

            return false; // Acrylic is not supported on this system
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark:
                    ConfigurationSource.Theme = SystemBackdropTheme.Dark;
                    myTextBlock.Text = "Dark";
                    break;
                case ElementTheme.Light:
                    ConfigurationSource.Theme = SystemBackdropTheme.Light;
                    myTextBlock.Text = "Light";
                    break;
                case ElementTheme.Default:
                    ConfigurationSource.Theme = SystemBackdropTheme.Default;
                    myTextBlock.Text = "Default";
                    break;
            }
        }

        private void DisposeBackdropController()
        {
            if (AcrylicController != null)
            {
                AcrylicController.Dispose();
                AcrylicController = null;
            }
            if (MicainfoController != null)
            {
                MicainfoController.Dispose();
                MicainfoController = null;
            }
            this.Activated -= Window_Activated;
            this.Closed -= Window_Closed;
            ((FrameworkElement)this.Content).ActualThemeChanged -= Window_ThemeChanged;
            if (ConfigurationSource != null)
            {
                ConfigurationSource = null;
            }
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentBackdropType)
            {
                case BackdropType.Micainfo:
                    CurrentBackdropType = BackdropType.DesktopAcrylic;
                    break;
                case BackdropType.DesktopAcrylic:
                    CurrentBackdropType = BackdropType.FallbackColor;
                    break;
                case BackdropType.FallbackColor:
                    CurrentBackdropType = BackdropType.Micainfo;
                    break;
            }
            SetBackdrop(CurrentBackdropType);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            ConfigurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            DisposeBackdropController();
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (ConfigurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }
    }
}
