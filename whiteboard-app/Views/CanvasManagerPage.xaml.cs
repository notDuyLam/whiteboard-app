using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using whiteboard_app.Services;
using whiteboard_app_data.Models;

namespace whiteboard_app.Views;

/// <summary>
/// Canvas Manager page for viewing and managing all canvases.
/// </summary>
public sealed partial class CanvasManagerPage : Page
{
    private IDataService? _dataService;
    private INavigationService? _navigationService;
    private List<Canvas> _allCanvases = new();

    public CanvasManagerPage()
    {
        InitializeComponent();
        Loaded += CanvasManagerPage_Loaded;
    }

    private void CanvasManagerPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _dataService = App.ServiceProvider?.GetService(typeof(IDataService)) as IDataService;
        _navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
        
        _ = LoadCanvasesAsync();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = LoadCanvasesAsync();
    }

    private async Task LoadCanvasesAsync()
    {
        if (_dataService == null)
            return;

        try
        {
            LoadingProgressRing.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            LoadingProgressRing.IsActive = true;
            CanvasesGridView.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            EmptyStateTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            // Load all canvases with their profiles
            var profiles = await _dataService.GetAllProfilesAsync();
            _allCanvases = new List<Canvas>();
            
            foreach (var profile in profiles)
            {
                var canvases = await _dataService.GetCanvasesByProfileIdAsync(profile.Id);
                _allCanvases.AddRange(canvases);
            }

            // Sort by last modified date (newest first)
            _allCanvases = _allCanvases.OrderByDescending(c => c.LastModifiedDate ?? c.CreatedDate).ToList();

            CanvasesGridView.ItemsSource = _allCanvases;

            if (_allCanvases.Count == 0)
            {
                EmptyStateTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            else
            {
                CanvasesGridView.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to load canvases: {ex.Message}",
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

    private void CanvasesGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Canvas canvas)
        {
            // Navigate to DrawingPage with this canvas
            _navigationService?.NavigateTo(typeof(DrawingPage), canvas);
        }
    }

    private void OpenCanvasButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Canvas canvas)
        {
            _navigationService?.NavigateTo(typeof(DrawingPage), canvas);
        }
    }

    private async void DeleteCanvasButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Canvas canvas && _dataService != null)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Canvas",
                Content = $"Are you sure you want to delete canvas '{canvas.Name}'? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await _dataService.DeleteCanvasAsync(canvas.Id);
                    await LoadCanvasesAsync(); // Refresh list
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to delete canvas: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}

