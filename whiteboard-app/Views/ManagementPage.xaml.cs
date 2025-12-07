using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using whiteboard_app.Services;

namespace whiteboard_app.Views;

/// <summary>
/// Management page with BreadcrumbBar navigation to Canvas Manager and Dashboard.
/// </summary>
public sealed partial class ManagementPage : Page
{
    private INavigationService? _navigationService;

    public ManagementPage()
    {
        InitializeComponent();
        Loaded += ManagementPage_Loaded;
    }

    private void ManagementPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Get navigation service
        _navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
        
        // Set up frame navigation
        if (_navigationService != null)
        {
            _navigationService.SetNavigationFrame(ManagementContentFrame);
        }

        // Navigate to Dashboard by default
        NavigateToDashboard();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // If parameter is provided, navigate to specific page
        if (e.Parameter is string pageName)
        {
            switch (pageName.ToLower())
            {
                case "dashboard":
                    NavigateToDashboard();
                    break;
                case "canvasmanager":
                case "canvas":
                    NavigateToCanvasManager();
                    break;
                default:
                    NavigateToDashboard();
                    break;
            }
        }
        else
        {
            NavigateToDashboard();
        }
    }

    public void NavigateToDashboard()
    {
        UpdateBreadcrumb("Dashboard");
        ManagementContentFrame.Navigate(typeof(DashboardPage));
    }

    public void NavigateToCanvasManager()
    {
        UpdateBreadcrumb("Canvas Manager");
        ManagementContentFrame.Navigate(typeof(CanvasManagerPage));
    }

    private void UpdateBreadcrumb(string currentPage)
    {
        ManagementBreadcrumbBar.Items.Clear();
        ManagementBreadcrumbBar.Items.Add(new BreadcrumbBarItem { Content = "Management" });
        if (!string.IsNullOrEmpty(currentPage))
        {
            ManagementBreadcrumbBar.Items.Add(new BreadcrumbBarItem { Content = currentPage });
        }
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Item is BreadcrumbBarItem item)
        {
            var content = item.Content?.ToString();
            if (content == "Management")
            {
                NavigateToDashboard();
            }
            else if (content == "Dashboard")
            {
                NavigateToDashboard();
            }
            else if (content == "Canvas Manager")
            {
                NavigateToCanvasManager();
            }
        }
    }
}

