using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using whiteboard_app.Services;

namespace whiteboard_app.Views;

/// <summary>
/// Dashboard page showing summary statistics and quick actions.
/// </summary>
public sealed partial class DashboardPage : Page
{
    private IDataService? _dataService;
    private INavigationService? _navigationService;

    public DashboardPage()
    {
        InitializeComponent();
        Loaded += DashboardPage_Loaded;
    }

    private void DashboardPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _dataService = App.ServiceProvider?.GetService(typeof(IDataService)) as IDataService;
        _navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
        
        _ = LoadStatisticsAsync();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = LoadStatisticsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        if (_dataService == null)
            return;

        try
        {
            LoadingProgressRing.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            LoadingProgressRing.IsActive = true;

            // Load all data
            var profiles = await _dataService.GetAllProfilesAsync();
            var templates = await _dataService.GetAllTemplatesAsync();
            
            // Count canvases and shapes
            int totalCanvases = 0;
            int totalShapes = 0;
            
            foreach (var profile in profiles)
            {
                var canvases = await _dataService.GetCanvasesByProfileIdAsync(profile.Id);
                totalCanvases += canvases.Count;
                
                foreach (var canvas in canvases)
                {
                    var shapes = await _dataService.GetShapesByCanvasIdAsync(canvas.Id);
                    totalShapes += shapes.Count;
                }
            }

            // Update UI
            TotalProfilesTextBlock.Text = profiles.Count.ToString();
            TotalCanvasesTextBlock.Text = totalCanvases.ToString();
            TotalShapesTextBlock.Text = totalShapes.ToString();
            TotalTemplatesTextBlock.Text = templates.Count.ToString();
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to load statistics: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
        finally
        {
            LoadingProgressRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            LoadingProgressRing.IsActive = false;
        }
    }

    private void CanvasManagerButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Navigate to Canvas Manager via parent ManagementPage
        var managementPage = FindParentManagementPage();
        if (managementPage != null)
        {
            managementPage.NavigateToCanvasManager();
        }
    }

    private ManagementPage? FindParentManagementPage()
    {
        var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
        while (parent != null)
        {
            if (parent is ManagementPage managementPage)
            {
                return managementPage;
            }
            parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}

