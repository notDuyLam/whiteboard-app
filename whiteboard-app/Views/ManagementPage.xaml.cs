using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using whiteboard_app.Services;
using whiteboard_app_data.Enums;
using CanvasModel = whiteboard_app_data.Models.Canvas;

namespace whiteboard_app.Views;

/// <summary>
/// Management page - Step 1: BreadcrumbBar and Dashboard UI (no data loading yet).
/// </summary>
/// <summary>
/// Wrapper class for Canvas display.
/// </summary>
public class CanvasDisplayItem
{
    public CanvasModel Canvas { get; set; }
    public string FormattedCreatedDate { get; set; } = string.Empty;
    public string FormattedLastModifiedDate { get; set; } = string.Empty;

    public CanvasDisplayItem(CanvasModel canvas)
    {
        Canvas = canvas;
        FormattedCreatedDate = canvas.CreatedDate.ToString("yyyy-MM-dd HH:mm");
        FormattedLastModifiedDate = canvas.LastModifiedDate.ToString("yyyy-MM-dd HH:mm");
    }
}

public sealed partial class ManagementPage : Page
{
    private IDataService? _dataService;
    private INavigationService? _navigationService;
    private string _currentView = "Dashboard";
    private List<CanvasDisplayItem> _allCanvases = new();
    
    // Chart data
    public ISeries[] ShapeTypeSeries { get; set; } = Array.Empty<ISeries>();
    public ISeries[] TopTemplatesSeries { get; set; } = Array.Empty<ISeries>();
    public Axis[] XAxis { get; set; } = Array.Empty<Axis>();

    public ManagementPage()
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] Constructor - START");
        
        try
        {
            InitializeComponent();
            
            // Get services
            _dataService = App.ServiceProvider?.GetService(typeof(IDataService)) as IDataService;
            _navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
            
            // Subscribe to Loaded event - update BreadcrumbBar AFTER page is fully rendered
            Loaded += ManagementPage_Loaded;
            
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Constructor - DataService null: {_dataService == null}");
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Constructor - NavigationService null: {_navigationService == null}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Constructor error: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
        
        System.Diagnostics.Debug.WriteLine("[ManagementPage] Constructor - END");
    }

    private void ManagementPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] Loaded event - START");
        
        try
        {
            // Update BreadcrumbBar AFTER page is fully rendered
            UpdateBreadcrumb("Dashboard");
            
            // Load dashboard data asynchronously (fire-and-forget)
            _ = LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Loaded event error: {ex.Message}\n{ex.StackTrace}");
        }
        
        System.Diagnostics.Debug.WriteLine("[ManagementPage] Loaded event - END");
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] OnNavigatedTo - START");
        base.OnNavigatedTo(e);
        
        // DON'T update BreadcrumbBar here - wait for Loaded event
        // This prevents freeze/crash during navigation
        
        System.Diagnostics.Debug.WriteLine("[ManagementPage] OnNavigatedTo - END");
    }

    private void UpdateBreadcrumb(string currentPage)
    {
        System.Diagnostics.Debug.WriteLine($"[ManagementPage] UpdateBreadcrumb - currentPage={currentPage}");
        
        try
        {
            _currentView = currentPage;
            
            // Check if BreadcrumbBar is initialized
            if (ManagementBreadcrumbBar == null)
            {
                System.Diagnostics.Debug.WriteLine("[ManagementPage] UpdateBreadcrumb - BreadcrumbBar is null!");
                return;
            }
            
            // Use simple strings instead of BreadcrumbBarItem to avoid XamlRoot context issues
            // BreadcrumbBar in WinUI 3 can accept IEnumerable<string> directly
            var items = new List<string> { "Management" };
            
            if (!string.IsNullOrEmpty(currentPage))
            {
                items.Add(currentPage);
            }
            
            ManagementBreadcrumbBar.ItemsSource = items;
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] UpdateBreadcrumb - ItemsSource set successfully (count: {items.Count})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] UpdateBreadcrumb error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        try
        {
            if (args.Item is string item)
            {
                System.Diagnostics.Debug.WriteLine($"[ManagementPage] Breadcrumb clicked: {item}");
                
                if (item == "Management" || item == "Dashboard")
                {
                    ShowDashboard();
                }
                else if (item == "Canvas Manager")
                {
                    ShowCanvasManager();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] BreadcrumbBar_ItemClicked error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void ShowDashboard()
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] ShowDashboard - START");
        
        _currentView = "Dashboard";
        UpdateBreadcrumb("Dashboard");
        
        // Simple visibility switching
        DashboardView.Visibility = Visibility.Visible;
        CanvasManagerView.Visibility = Visibility.Collapsed;
        
        System.Diagnostics.Debug.WriteLine("[ManagementPage] ShowDashboard - END");
    }

    private void ShowCanvasManager()
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] ShowCanvasManager - START");
        
        _currentView = "Canvas Manager";
        UpdateBreadcrumb("Canvas Manager");
        
        // Simple visibility switching
        DashboardView.Visibility = Visibility.Collapsed;
        CanvasManagerView.Visibility = Visibility.Visible;
        
        // Load canvas manager data
        _ = LoadCanvasesAsync();
        
        System.Diagnostics.Debug.WriteLine("[ManagementPage] ShowCanvasManager - END");
    }

    private void CanvasManagerButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] CanvasManagerButton clicked");
        ShowCanvasManager();
    }

    // ==================== DASHBOARD DATA LOADING ====================
    
    private async Task LoadDashboardDataAsync()
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] LoadDashboardDataAsync - START");
        
        if (_dataService == null)
        {
            System.Diagnostics.Debug.WriteLine("[ManagementPage] DataService is null");
            return;
        }

        try
        {
            // Show loading indicator
            if (DashboardLoadingProgressRing != null)
            {
                DashboardLoadingProgressRing.Visibility = Visibility.Visible;
                DashboardLoadingProgressRing.IsActive = true;
            }

            // Load data
            System.Diagnostics.Debug.WriteLine("[ManagementPage] Loading profiles...");
            var profiles = await _dataService.GetAllProfilesAsync();
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Loaded {profiles.Count} profiles");
            
            System.Diagnostics.Debug.WriteLine("[ManagementPage] Loading templates...");
            var templates = await _dataService.GetAllTemplatesAsync();
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Loaded {templates.Count} templates");
            
            int totalCanvases = 0;
            System.Diagnostics.Debug.WriteLine("[ManagementPage] Counting canvases...");
            foreach (var profile in profiles)
            {
                var canvases = await _dataService.GetCanvasesByProfileIdAsync(profile.Id);
                totalCanvases += canvases.Count;
            }
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Total canvases: {totalCanvases}");
            
            int totalShapes = 0;
            try
            {
                System.Diagnostics.Debug.WriteLine("[ManagementPage] Counting shapes...");
                totalShapes = await _dataService.GetTotalShapesCountAsync();
                System.Diagnostics.Debug.WriteLine($"[ManagementPage] Total shapes: {totalShapes}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ManagementPage] Error counting shapes: {ex.Message}");
            }

            // Load statistics for charts
            Dictionary<ShapeType, int> shapeTypeStats = new();
            List<(string TemplateName, int UsageCount)> topTemplates = new();
            
            try
            {
                System.Diagnostics.Debug.WriteLine("[ManagementPage] Loading shape type statistics...");
                shapeTypeStats = await _dataService.GetShapeTypeStatisticsAsync();
                System.Diagnostics.Debug.WriteLine($"[ManagementPage] Loaded shape type statistics: {shapeTypeStats.Count} types");
                
                System.Diagnostics.Debug.WriteLine("[ManagementPage] Loading top templates...");
                topTemplates = await _dataService.GetTopTemplatesAsync(10);
                System.Diagnostics.Debug.WriteLine($"[ManagementPage] Loaded {topTemplates.Count} top templates");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ManagementPage] Error loading statistics: {ex.Message}");
            }

            // Update UI on UI thread
            if (TotalProfilesTextBlock != null)
                TotalProfilesTextBlock.Text = profiles.Count.ToString();
            if (TotalCanvasesTextBlock != null)
                TotalCanvasesTextBlock.Text = totalCanvases.ToString();
            if (TotalShapesTextBlock != null)
                TotalShapesTextBlock.Text = totalShapes.ToString();
            if (TotalTemplatesTextBlock != null)
                TotalTemplatesTextBlock.Text = templates.Count.ToString();
            
            // Update charts on UI thread
            if (DispatcherQueue != null)
            {
                DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                {
                    UpdateShapeTypeChart(shapeTypeStats);
                    UpdateTopTemplatesChart(topTemplates);
                });
            }
            else
            {
                // Fallback: update directly if DispatcherQueue is not available
                UpdateShapeTypeChart(shapeTypeStats);
                UpdateTopTemplatesChart(topTemplates);
            }
            
            System.Diagnostics.Debug.WriteLine("[ManagementPage] UI updated successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Error loading dashboard: {ex.Message}\n{ex.StackTrace}");
            
            // Show error values
            if (TotalProfilesTextBlock != null)
                TotalProfilesTextBlock.Text = "?";
            if (TotalCanvasesTextBlock != null)
                TotalCanvasesTextBlock.Text = "?";
            if (TotalShapesTextBlock != null)
                TotalShapesTextBlock.Text = "?";
            if (TotalTemplatesTextBlock != null)
                TotalTemplatesTextBlock.Text = "?";
        }
        finally
        {
            // Hide loading indicator
            if (DashboardLoadingProgressRing != null)
            {
                DashboardLoadingProgressRing.Visibility = Visibility.Collapsed;
                DashboardLoadingProgressRing.IsActive = false;
            }
        }
        
        System.Diagnostics.Debug.WriteLine("[ManagementPage] LoadDashboardDataAsync - END");
    }

    private void UpdateShapeTypeChart(Dictionary<ShapeType, int> statistics)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ManagementPage] UpdateShapeTypeChart - START");
            
            var total = statistics.Values.Sum();
            if (total == 0)
            {
                ShapeTypeSeries = Array.Empty<ISeries>();
                return;
            }

            var colors = new[]
            {
                new SolidColorPaint(SKColors.Blue),
                new SolidColorPaint(SKColors.Green),
                new SolidColorPaint(SKColors.Orange),
                new SolidColorPaint(SKColors.Red),
                new SolidColorPaint(SKColors.Purple),
                new SolidColorPaint(SKColors.Teal)
            };

            var series = new List<ISeries>();
            int colorIndex = 0;
            
            foreach (var kvp in statistics.OrderByDescending(x => x.Value))
            {
                if (kvp.Value > 0)
                {
                    var shapeTypeName = kvp.Key.ToString();
                    var percentage = (double)kvp.Value / total * 100;
                    
                    series.Add(new PieSeries<double>
                    {
                        Name = $"{shapeTypeName} ({kvp.Value})",
                        Values = new[] { (double)kvp.Value },
                        Fill = colors[colorIndex % colors.Length]
                    });
                    
                    colorIndex++;
                }
            }
            
            ShapeTypeSeries = series.ToArray();
            
            // Update chart control
            if (ShapeTypePieChart != null)
            {
                ShapeTypePieChart.Series = ShapeTypeSeries;
            }
            
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] UpdateShapeTypeChart - SUCCESS: {series.Count} series");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] UpdateShapeTypeChart - ERROR: {ex.Message}\n{ex.StackTrace}");
            ShapeTypeSeries = Array.Empty<ISeries>();
        }
    }

    private void UpdateTopTemplatesChart(List<(string TemplateName, int UsageCount)> templates)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ManagementPage] UpdateTopTemplatesChart - START");
            
            if (templates == null || templates.Count == 0)
            {
                TopTemplatesSeries = Array.Empty<ISeries>();
                XAxis = Array.Empty<Axis>();
                return;
            }

            var values = templates.Select(t => (double)t.UsageCount).ToArray();
            var labels = templates.Select(t => t.TemplateName).ToArray();

            TopTemplatesSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    Name = "Usage Count"
                }
            };

            XAxis = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 45,
                    TextSize = 12
                }
            };
            
            // Update chart control
            if (TopTemplatesChart != null)
            {
                TopTemplatesChart.Series = TopTemplatesSeries;
                TopTemplatesChart.XAxes = XAxis;
            }
            
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] UpdateTopTemplatesChart - SUCCESS: {templates.Count} templates");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] UpdateTopTemplatesChart - ERROR: {ex.Message}\n{ex.StackTrace}");
            TopTemplatesSeries = Array.Empty<ISeries>();
            XAxis = Array.Empty<Axis>();
        }
    }

    // ==================== CANVAS MANAGER DATA LOADING ====================
    
    private async Task LoadCanvasesAsync()
    {
        System.Diagnostics.Debug.WriteLine("[ManagementPage] LoadCanvasesAsync - START");
        
        if (_dataService == null)
        {
            System.Diagnostics.Debug.WriteLine("[ManagementPage] DataService is null");
            return;
        }

        try
        {
            CanvasManagerLoadingProgressRing.Visibility = Visibility.Visible;
            CanvasManagerLoadingProgressRing.IsActive = true;
            CanvasesGridView.Visibility = Visibility.Collapsed;
            EmptyStateTextBlock.Visibility = Visibility.Collapsed;

            // Load all canvases
            System.Diagnostics.Debug.WriteLine("[ManagementPage] Loading all profiles...");
            var profiles = await _dataService.GetAllProfilesAsync();
            var allCanvasesRaw = new List<CanvasModel>();
            
            System.Diagnostics.Debug.WriteLine("[ManagementPage] Loading canvases for each profile...");
            foreach (var profile in profiles)
            {
                var canvases = await _dataService.GetCanvasesByProfileIdAsync(profile.Id);
                allCanvasesRaw.AddRange(canvases);
            }

            // Sort and create display items
            var sortedCanvases = allCanvasesRaw.OrderByDescending(c => c.LastModifiedDate).ToList();
            _allCanvases = sortedCanvases.Select(c => new CanvasDisplayItem(c)).ToList();

            CanvasesGridView.ItemsSource = _allCanvases;

            if (_allCanvases.Count == 0)
            {
                EmptyStateTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                CanvasesGridView.Visibility = Visibility.Visible;
            }
            
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Loaded {_allCanvases.Count} canvases");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Error loading canvases: {ex.Message}");
            
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
            CanvasManagerLoadingProgressRing.Visibility = Visibility.Collapsed;
            CanvasManagerLoadingProgressRing.IsActive = false;
        }
        
        System.Diagnostics.Debug.WriteLine("[ManagementPage] LoadCanvasesAsync - END");
    }

    private void CanvasesGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CanvasDisplayItem item)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Canvas clicked: {item.Canvas.Name}");
            _navigationService?.NavigateTo(typeof(DrawingPage), item.Canvas);
        }
    }

    private void OpenCanvasButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CanvasDisplayItem item)
        {
            System.Diagnostics.Debug.WriteLine($"[ManagementPage] Open canvas: {item.Canvas.Name}");
            _navigationService?.NavigateTo(typeof(DrawingPage), item.Canvas);
        }
    }

    private async void DeleteCanvasButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CanvasDisplayItem item && _dataService != null)
        {
            var canvas = item.Canvas;
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
                    System.Diagnostics.Debug.WriteLine($"[ManagementPage] Deleting canvas: {canvas.Name}");
                    await _dataService.DeleteCanvasAsync(canvas.Id);
                    await LoadCanvasesAsync(); // Refresh list
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ManagementPage] Delete error: {ex.Message}");
                    
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
