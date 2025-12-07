using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using whiteboard_app_data.Models;

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
                // Navigate to initial page
                _navigationService?.NavigateTo(typeof(Views.HomePage));
            }
        }

        private void RootNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (_navigationService?.CanGoBack == true)
            {
                _navigationService.GoBack();
            }
        }

        private async void RootNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer is NavigationViewItem item && item.Tag is string tag)
            {
                switch (tag)
                {
                    case "Home":
                        _navigationService?.NavigateTo(typeof(Views.HomePage));
                        break;
                    case "Drawing":
                        // Always show profile selection dialog
                        await ShowProfileSelectionDialog();
                        break;
                    case "Management":
                        _navigationService?.NavigateTo(typeof(Views.ManagementPage));
                        break;
                }
            }
        }

        private void RootNavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            // Responsive behavior: adjust pane display mode based on window size
            // NavigationView with PaneDisplayMode="Auto" will automatically handle responsive behavior
            // In Minimal mode, pane is hidden and can be toggled with hamburger button
            // In Compact mode, pane shows icons only
            // In Expanded mode, pane shows full content
        }

        private void RootNavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            // Allow pane to close in responsive modes
        }

        private void RootNavigationView_PaneOpening(NavigationView sender, object args)
        {
            // Pane is opening
        }

        /// <summary>
        /// Shows a dialog for the user to select a profile before drawing.
        /// </summary>
        private async Task ShowProfileSelectionDialog()
        {
            var dataService = App.ServiceProvider?.GetService(typeof(IDataService)) as IDataService;
            if (dataService == null)
            {
                // No data service - navigate to Home
                _navigationService?.NavigateTo(typeof(Views.HomePage));
                return;
            }

            // Load all profiles
            var profiles = await dataService.GetAllProfilesAsync();
            if (profiles == null || profiles.Count == 0)
            {
                // No profiles available - show dialog and navigate to Home
                var noProfileDialog = new ContentDialog
                {
                    Title = "No Profiles Available",
                    Content = "You need to create a profile first. Please go to the Home page to create a profile.",
                    PrimaryButtonText = "Go to Home",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = Content.XamlRoot
                };
                var result = await noProfileDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    _navigationService?.NavigateTo(typeof(Views.HomePage));
                }
                return;
            }

            // Show profile selection dialog
            var dialog = new ContentDialog
            {
                Title = "Select Profile",
                Content = "Please select a profile to start drawing:",
                PrimaryButtonText = "Start Drawing",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 16 };
            
            var profileComboBox = new ComboBox
            {
                Header = "Profile *",
                PlaceholderText = "Select a profile",
                ItemsSource = profiles,
                DisplayMemberPath = "Name"
            };
            
            // Select first profile by default
            if (profiles.Count > 0)
            {
                profileComboBox.SelectedIndex = 0;
            }
            
            stackPanel.Children.Add(profileComboBox);
            dialog.Content = stackPanel;

            // Focus on combo box when dialog opens
            dialog.Opened += (s, args) => profileComboBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);

            var dialogResult = await dialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                if (profileComboBox.SelectedItem is Profile selectedProfile)
                {
                    // Navigate to DrawingPage with selected profile
                    _navigationService?.NavigateTo(typeof(Views.DrawingPage), selectedProfile);
                }
                else
                {
                    // No profile selected - show error
                    var errorDialog = new ContentDialog
                    {
                        Title = "Profile Required",
                        Content = "Please select a profile to continue.",
                        CloseButtonText = "OK",
                        XamlRoot = Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}
