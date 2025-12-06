using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using whiteboard_app.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace whiteboard_app
{
    /// <summary>
    /// Main window of the application with custom TitleBar and NavigationView.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private INavigationService? _navigationService;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            InitializeTitleBar();
            InitializeNavigation();
        }

        private void InitializeTitleBar()
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
        }

        private void InitializeNavigation()
        {
        if (App.ServiceProvider != null)
        {
            _navigationService = App.ServiceProvider.GetService(typeof(INavigationService)) as INavigationService;
            _navigationService?.SetNavigationFrame(ContentFrame);
        }
        }

        private void RootNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (_navigationService?.CanGoBack == true)
            {
                _navigationService.GoBack();
            }
        }

        private void RootNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer is NavigationViewItem item && item.Tag is string tag)
            {
                // Navigation will be handled by ViewModels
                // This is just the UI structure
            }
        }

        private void RootNavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            // Responsive behavior: adjust pane display mode based on window size
            if (args.DisplayMode == NavigationViewDisplayMode.Compact || args.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                // Compact mode - pane can be toggled
            }
        }
    }
}
