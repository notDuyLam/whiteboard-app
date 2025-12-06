using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

    /// <summary>
    /// Gets the profile settings for the selected profile.
    /// Returns null if no profile is selected.
    /// </summary>
    public Profile? GetSelectedProfileSettings()
    {
        return SelectedProfile;
    }

    /// <summary>
    /// Loads profile settings by ID.
    /// </summary>
    public async Task<Profile?> LoadProfileSettingsAsync(Guid profileId)
    {
        return await _dataService.GetProfileByIdAsync(profileId);
    }

    [RelayCommand]
    private async Task CreateCanvasAsync((string Name, int Width, int Height, string BackgroundColor) canvasData)
    {
        if (SelectedProfile == null || string.IsNullOrWhiteSpace(canvasData.Name))
            return;

        // Validate canvas name length
        if (canvasData.Name.Length > 200)
            throw new ArgumentException("Canvas name must be 200 characters or less.");

        // Validate dimensions
        if (canvasData.Width < 100 || canvasData.Width > 10000)
            throw new ArgumentException("Canvas width must be between 100 and 10000.");
        
        if (canvasData.Height < 100 || canvasData.Height > 10000)
            throw new ArgumentException("Canvas height must be between 100 and 10000.");

        var newCanvas = new Canvas
        {
            Name = canvasData.Name.Trim(),
            Width = canvasData.Width,
            Height = canvasData.Height,
            BackgroundColor = canvasData.BackgroundColor,
            ProfileId = SelectedProfile.Id
        };

        try
        {
            await _dataService.CreateCanvasAsync(newCanvas);
        }
        catch (Exception)
        {
            // Error handling - canvas creation failed
            throw;
        }
    }
}

