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
            App.ServiceProvider!.GetService<IDataService>()!,
            App.ServiceProvider!.GetService<INavigationService>()!
        );
        _navigationService = App.ServiceProvider!.GetService<INavigationService>();
        DataContext = ViewModel;
        Loaded += HomePage_Loaded;
        ViewModel.NavigateToDrawingCommand.CanExecuteChanged += (s, e) => { };
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

    private async void CreateProfileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Will be implemented in Profile Management phase
        await Task.CompletedTask;
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

