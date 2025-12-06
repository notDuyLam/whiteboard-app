using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using whiteboard_app.ViewModels;

namespace whiteboard_app.Views;

/// <summary>
/// Home page of the application with profile selection.
/// </summary>
public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        InitializeComponent();
        ViewModel = new HomeViewModel(
            App.ServiceProvider!.GetService<whiteboard_app.Services.IDataService>()!,
            App.ServiceProvider!.GetService<whiteboard_app.Services.INavigationService>()!
        );
        DataContext = ViewModel;
        Loaded += HomePage_Loaded;
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

    private void CreateProfileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Will be implemented in Profile Management phase
    }
}

