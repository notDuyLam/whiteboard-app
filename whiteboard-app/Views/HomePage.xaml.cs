using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using whiteboard_app.ViewModels;
using whiteboard_app.Services;

namespace whiteboard_app.Views;

/// <summary>
/// Home page of the application with profile selection.
/// </summary>
public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }
    private readonly INavigationService? _navigationService;

    public HomePage()
    {
        InitializeComponent();
        ViewModel = new HomeViewModel(
            App.ServiceProvider!.GetService(typeof(IDataService)) as IDataService ?? throw new InvalidOperationException(),
            App.ServiceProvider!.GetService(typeof(INavigationService)) as INavigationService ?? throw new InvalidOperationException()
        );
        _navigationService = App.ServiceProvider!.GetService(typeof(INavigationService)) as INavigationService;
        DataContext = ViewModel;
        Loaded += HomePage_Loaded;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Set initial ItemsSource
        ProfilesGridView.ItemsSource = ViewModel.Profiles;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsLoading))
        {
            LoadingProgressRing.IsActive = ViewModel.IsLoading;
            LoadingProgressRing.Visibility = ViewModel.IsLoading ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            ProfilesGridView.Visibility = ViewModel.IsLoading ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
        }
        if (e.PropertyName == nameof(ViewModel.Profiles))
        {
            ProfilesGridView.ItemsSource = ViewModel.Profiles;
        }
        if (e.PropertyName == nameof(ViewModel.SelectedProfile))
        {
            StartDrawingButton.IsEnabled = ViewModel.SelectedProfile != null;
            CreateCanvasButton.IsEnabled = ViewModel.SelectedProfile != null;
            if (ViewModel.SelectedProfile != null)
            {
                ProfilesGridView.SelectedItem = ViewModel.SelectedProfile;
            }
        }
    }

    private async void HomePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadProfilesCommand.ExecuteAsync(null);
    }

    private void ProfilesGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is whiteboard_app_data.Models.Profile profile)
        {
            ViewModel.SelectedProfile = profile;
        }
    }

    private void ManageProfilesButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _navigationService?.NavigateTo(typeof(Views.ProfilePage));
    }

    private async void StartDrawingButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SelectedProfile == null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Start Drawing",
            Content = $"Do you want to start drawing with profile '{ViewModel.SelectedProfile.Name}'?",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Navigate to DrawingPage with selected profile
            _navigationService?.NavigateTo(typeof(Views.DrawingPage), ViewModel.SelectedProfile);
        }
    }

    private async void CreateCanvasButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SelectedProfile == null)
            return;

        var dialog = new ContentDialog
        {
            Title = "Create New Canvas",
            PrimaryButtonText = "Create",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var stackPanel = new StackPanel { Spacing = 16 };
        
        var canvasNameTextBox = new TextBox
        {
            Header = "Canvas Name *",
            PlaceholderText = "Enter canvas name",
            MaxLength = 200
        };
        stackPanel.Children.Add(canvasNameTextBox);

        var sizePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var widthTextBox = new TextBox
        {
            Header = "Width",
            Text = ViewModel.SelectedProfile.DefaultCanvasWidth.ToString(),
            Width = 180
        };
        var heightTextBox = new TextBox
        {
            Header = "Height",
            Text = ViewModel.SelectedProfile.DefaultCanvasHeight.ToString(),
            Width = 180
        };
        sizePanel.Children.Add(widthTextBox);
        sizePanel.Children.Add(heightTextBox);
        stackPanel.Children.Add(sizePanel);

        var backgroundColorTextBox = new TextBox
        {
            Header = "Background Color (Hex)",
            Text = ViewModel.SelectedProfile.DefaultFillColor != "Transparent" 
                ? ViewModel.SelectedProfile.DefaultFillColor 
                : "#FFFFFF",
            PlaceholderText = "#FFFFFF"
        };
        stackPanel.Children.Add(backgroundColorTextBox);

        var errorTextBlock = new TextBlock
        {
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Visibility = Microsoft.UI.Xaml.Visibility.Collapsed
        };
        stackPanel.Children.Add(errorTextBlock);

        var scrollViewer = new ScrollViewer
        {
            MaxHeight = 400,
            Content = stackPanel
        };
        dialog.Content = scrollViewer;

        dialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();
            try
            {
                errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                errorTextBlock.Text = string.Empty;

                var canvasName = canvasNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(canvasName))
                {
                    errorTextBlock.Text = "Canvas name is required.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (canvasName.Length > 200)
                {
                    errorTextBlock.Text = "Canvas name must be 200 characters or less.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (!int.TryParse(widthTextBox.Text, out int width) || width < 100 || width > 10000)
                {
                    errorTextBlock.Text = "Width must be a number between 100 and 10000.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (!int.TryParse(heightTextBox.Text, out int height) || height < 100 || height > 10000)
                {
                    errorTextBlock.Text = "Height must be a number between 100 and 10000.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                var backgroundColor = backgroundColorTextBox.Text.Trim();
                if (backgroundColor == "Transparent")
                {
                    // Transparent is valid
                }
                else
                {
                    if (!backgroundColor.StartsWith("#"))
                    {
                        backgroundColor = "#" + backgroundColor;
                    }

                    // Validate hex color format: #RRGGBB or #RRGGBBAA
                    if (backgroundColor.Length != 7 && backgroundColor.Length != 9)
                    {
                        errorTextBlock.Text = "Background color must be in hex format (#RRGGBB or #RRGGBBAA) or 'Transparent'.";
                        errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        args.Cancel = true;
                        return;
                    }

                    // Validate hex characters
                    var hexPart = backgroundColor.Substring(1);
                    if (!System.Text.RegularExpressions.Regex.IsMatch(hexPart, @"^[0-9A-Fa-f]+$"))
                    {
                        errorTextBlock.Text = "Background color contains invalid hex characters.";
                        errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        args.Cancel = true;
                        return;
                    }
                }

                try
                {
                    await ViewModel.CreateCanvasCommand.ExecuteAsync((canvasName, width, height, backgroundColor));
                    // Success - dialog will close automatically
                }
                catch (Exception ex)
                {
                    errorTextBlock.Text = $"Failed to create canvas: {ex.Message}";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
            }
            finally
            {
                deferral.Complete();
            }
        };

        var result = await dialog.ShowAsync();
        
        // If dialog was closed with Primary button (Create), navigate to DrawingPage with the canvas
        if (result == ContentDialogResult.Primary && ViewModel.CreatedCanvas != null)
        {
            // Navigate to DrawingPage with the created canvas
            _navigationService?.NavigateTo(typeof(Views.DrawingPage), ViewModel.CreatedCanvas);
        }
    }
}

