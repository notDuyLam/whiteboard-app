using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using whiteboard_app.Services;
using whiteboard_app_data.Models;

namespace whiteboard_app.ViewModels;

/// <summary>
/// ViewModel for the ProfilePage, managing profile CRUD operations.
/// </summary>
public partial class ProfileViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<Profile> profiles = new();

    [ObservableProperty]
    private Profile? selectedProfile;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isCreateDialogOpen;

    [ObservableProperty]
    private string newProfileName = string.Empty;

    [ObservableProperty]
    private string newProfileTheme = "System";

    [ObservableProperty]
    private int newProfileCanvasWidth = 800;

    [ObservableProperty]
    private int newProfileCanvasHeight = 600;

    [ObservableProperty]
    private string newProfileStrokeColor = "#000000";

    [ObservableProperty]
    private double newProfileStrokeThickness = 2.0;

    [ObservableProperty]
    private string newProfileFillColor = "Transparent";

    public ProfileViewModel(IDataService dataService)
    {
        _dataService = dataService;
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
    private void OpenCreateDialog()
    {
        ResetNewProfileFields();
        IsCreateDialogOpen = true;
    }

    [RelayCommand]
    private void CloseCreateDialog()
    {
        IsCreateDialogOpen = false;
        ResetNewProfileFields();
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProfileName))
        {
            return;
        }

        // Validate profile name uniqueness
        var existingProfile = Profiles.FirstOrDefault(p => p.Name.Equals(NewProfileName.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existingProfile != null)
        {
            // Profile name already exists - validation will be shown in UI
            return;
        }

        var newProfile = new Profile
        {
            Name = NewProfileName.Trim(),
            Theme = NewProfileTheme,
            DefaultCanvasWidth = NewProfileCanvasWidth,
            DefaultCanvasHeight = NewProfileCanvasHeight,
            DefaultStrokeColor = NewProfileStrokeColor,
            DefaultStrokeThickness = NewProfileStrokeThickness,
            DefaultFillColor = NewProfileFillColor,
            IsActive = false
        };

        try
        {
            await _dataService.CreateProfileAsync(newProfile);
            await LoadProfilesAsync();
            CloseCreateDialog();
        }
        catch
        {
            // Error handling - profile creation failed
            // In a real app, we would show an error message to the user
        }
    }

    private void ResetNewProfileFields()
    {
        NewProfileName = string.Empty;
        NewProfileTheme = "System";
        NewProfileCanvasWidth = 800;
        NewProfileCanvasHeight = 600;
        NewProfileStrokeColor = "#000000";
        NewProfileStrokeThickness = 2.0;
        NewProfileFillColor = "Transparent";
    }
}

