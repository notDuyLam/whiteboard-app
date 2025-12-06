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
            // Navigate to Drawing page will be implemented when DrawingPage is created
            // _navigationService?.NavigateTo<DrawingPage>(ViewModel.SelectedProfile);
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
            Text = "#FFFFFF",
            PlaceholderText = "#FFFFFF"
        };
        stackPanel.Children.Add(backgroundColorTextBox);

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
                if (string.IsNullOrWhiteSpace(canvasNameTextBox.Text))
                {
                    args.Cancel = true;
                    return;
                }

                if (!int.TryParse(widthTextBox.Text, out int width) || width < 100 || width > 10000)
                {
                    args.Cancel = true;
                    return;
                }

                if (!int.TryParse(heightTextBox.Text, out int height) || height < 100 || height > 10000)
                {
                    args.Cancel = true;
                    return;
                }

                var backgroundColor = backgroundColorTextBox.Text.Trim();
                if (!backgroundColor.StartsWith("#") && backgroundColor != "Transparent")
                {
                    backgroundColor = "#" + backgroundColor;
                }

                await ViewModel.CreateCanvasCommand.ExecuteAsync((canvasNameTextBox.Text.Trim(), width, height, backgroundColor));
            }
            finally
            {
                deferral.Complete();
            }
        };

        await dialog.ShowAsync();
    }
}

