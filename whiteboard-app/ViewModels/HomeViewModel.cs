using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using whiteboard_app.Services;
using whiteboard_app_data.Models;

namespace whiteboard_app.ViewModels;

/// <summary>
/// ViewModel for the HomePage, managing profile selection and navigation.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Profile> profiles = new();

    [ObservableProperty]
    private Profile? selectedProfile;

    [ObservableProperty]
    private bool isLoading;

    public HomeViewModel(IDataService dataService, INavigationService navigationService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        IsLoading = true;
        try
        {
            var profileList = await _dataService.GetAllProfilesAsync();
            Profiles.Clear();
            foreach (var profile in profileList)
            {
                Profiles.Add(profile);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToDrawingAsync()
    {
        if (SelectedProfile != null)
        {
            // Confirmation dialog will be shown in the View
            await Task.CompletedTask;
        }
    }
}

