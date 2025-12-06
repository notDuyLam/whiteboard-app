using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using whiteboard_app.Services;
using whiteboard_app.ViewModels;
using whiteboard_app_data.Models;

namespace whiteboard_app.Views;

/// <summary>
/// Profile management page for creating, editing, and deleting profiles.
/// </summary>
public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; }

    public ProfilePage()
    {
        InitializeComponent();
        ViewModel = new ProfileViewModel(
            App.ServiceProvider!.GetService(typeof(IDataService)) as IDataService ?? throw new InvalidOperationException()
        );
        DataContext = ViewModel;
        Loaded += ProfilePage_Loaded;
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
            // Ensure grid is visible after loading
            ProfilesGridView.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            LoadingProgressRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }

    private async void ProfilePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadProfilesCommand.ExecuteAsync(null);
    }

    private void ProfilesGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Profile profile)
        {
            ViewModel.SelectedProfile = profile;
        }
    }

    private async void CreateProfileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Create New Profile",
            PrimaryButtonText = "Create",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var stackPanel = new StackPanel { Spacing = 16 };
        
        var profileNameTextBox = new TextBox
        {
            Header = "Profile Name *",
            PlaceholderText = "Enter profile name",
            MaxLength = 100
        };
        stackPanel.Children.Add(profileNameTextBox);

        var themeComboBox = new ComboBox
        {
            Header = "Theme",
            SelectedIndex = 2
        };
        themeComboBox.Items.Add(new ComboBoxItem { Content = "Light" });
        themeComboBox.Items.Add(new ComboBoxItem { Content = "Dark" });
        themeComboBox.Items.Add(new ComboBoxItem { Content = "System" });
        stackPanel.Children.Add(themeComboBox);

        var sizePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var canvasWidthTextBox = new TextBox
        {
            Header = "Canvas Width",
            Text = "800",
            Width = 180
        };
        var canvasHeightTextBox = new TextBox
        {
            Header = "Canvas Height",
            Text = "600",
            Width = 180
        };
        sizePanel.Children.Add(canvasWidthTextBox);
        sizePanel.Children.Add(canvasHeightTextBox);
        stackPanel.Children.Add(sizePanel);

        var strokeColorTextBox = new TextBox
        {
            Header = "Stroke Color (Hex)",
            Text = "#000000",
            PlaceholderText = "#000000"
        };
        stackPanel.Children.Add(strokeColorTextBox);

        var thicknessPanel = new StackPanel();
        var thicknessLabel = new TextBlock
        {
            Text = "Stroke Thickness",
            Style = (Microsoft.UI.Xaml.Style)Microsoft.UI.Xaml.Application.Current.Resources["CaptionTextBlockStyle"],
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 8)
        };
        var strokeThicknessSlider = new Slider
        {
            Minimum = 0.5,
            Maximum = 50,
            Value = 2.0,
            TickFrequency = 0.5,
            TickPlacement = Microsoft.UI.Xaml.Controls.Primitives.TickPlacement.BottomRight
        };
        var thicknessValueText = new TextBlock
        {
            Text = "2.0",
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
            Style = (Microsoft.UI.Xaml.Style)Microsoft.UI.Xaml.Application.Current.Resources["CaptionTextBlockStyle"]
        };
        strokeThicknessSlider.ValueChanged += (s, args) =>
        {
            thicknessValueText.Text = args.NewValue.ToString("F1");
        };
        thicknessPanel.Children.Add(thicknessLabel);
        thicknessPanel.Children.Add(strokeThicknessSlider);
        thicknessPanel.Children.Add(thicknessValueText);
        stackPanel.Children.Add(thicknessPanel);

        var fillColorTextBox = new TextBox
        {
            Header = "Fill Color (Hex or Transparent)",
            Text = "Transparent",
            PlaceholderText = "Transparent or #RRGGBB"
        };
        stackPanel.Children.Add(fillColorTextBox);

        var scrollViewer = new ScrollViewer
        {
            MaxHeight = 500,
            Content = stackPanel
        };
        dialog.Content = scrollViewer;

        dialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();
            try
            {
                if (string.IsNullOrWhiteSpace(profileNameTextBox.Text))
                {
                    args.Cancel = true;
                    return;
                }

                ViewModel.NewProfileName = profileNameTextBox.Text.Trim();
                ViewModel.NewProfileTheme = (themeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "System";
                
                if (int.TryParse(canvasWidthTextBox.Text, out int width) && width >= 100 && width <= 10000)
                {
                    ViewModel.NewProfileCanvasWidth = width;
                }
                
                if (int.TryParse(canvasHeightTextBox.Text, out int height) && height >= 100 && height <= 10000)
                {
                    ViewModel.NewProfileCanvasHeight = height;
                }
                
                ViewModel.NewProfileStrokeColor = strokeColorTextBox.Text.Trim();
                ViewModel.NewProfileStrokeThickness = strokeThicknessSlider.Value;
                ViewModel.NewProfileFillColor = fillColorTextBox.Text.Trim();

                // Validate stroke color format
                if (!string.IsNullOrWhiteSpace(ViewModel.NewProfileStrokeColor) && 
                    !ViewModel.NewProfileStrokeColor.StartsWith("#") && 
                    ViewModel.NewProfileStrokeColor != "Transparent")
                {
                    ViewModel.NewProfileStrokeColor = "#" + ViewModel.NewProfileStrokeColor;
                }

                await ViewModel.CreateProfileCommand.ExecuteAsync(null);
            }
            finally
            {
                deferral.Complete();
            }
        };

        await dialog.ShowAsync();
    }

    private async void EditProfileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Profile profile)
        {
            ViewModel.OpenEditDialogCommand.Execute(profile);
            await ShowEditProfileDialogAsync();
        }
    }

    private async Task ShowEditProfileDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Edit Profile",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var stackPanel = new StackPanel { Spacing = 16 };
        
        var profileNameTextBox = new TextBox
        {
            Header = "Profile Name *",
            PlaceholderText = "Enter profile name",
            MaxLength = 100,
            Text = ViewModel.EditProfileName
        };
        stackPanel.Children.Add(profileNameTextBox);

        var themeComboBox = new ComboBox
        {
            Header = "Theme"
        };
        themeComboBox.Items.Add(new ComboBoxItem { Content = "Light" });
        themeComboBox.Items.Add(new ComboBoxItem { Content = "Dark" });
        themeComboBox.Items.Add(new ComboBoxItem { Content = "System" });
        
        // Set selected index based on current theme
        switch (ViewModel.EditProfileTheme)
        {
            case "Light":
                themeComboBox.SelectedIndex = 0;
                break;
            case "Dark":
                themeComboBox.SelectedIndex = 1;
                break;
            case "System":
            default:
                themeComboBox.SelectedIndex = 2;
                break;
        }
        stackPanel.Children.Add(themeComboBox);

        var sizePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var canvasWidthTextBox = new TextBox
        {
            Header = "Canvas Width",
            Text = ViewModel.EditProfileCanvasWidth.ToString(),
            Width = 180
        };
        var canvasHeightTextBox = new TextBox
        {
            Header = "Canvas Height",
            Text = ViewModel.EditProfileCanvasHeight.ToString(),
            Width = 180
        };
        sizePanel.Children.Add(canvasWidthTextBox);
        sizePanel.Children.Add(canvasHeightTextBox);
        stackPanel.Children.Add(sizePanel);

        var strokeColorTextBox = new TextBox
        {
            Header = "Stroke Color (Hex)",
            Text = ViewModel.EditProfileStrokeColor,
            PlaceholderText = "#000000"
        };
        stackPanel.Children.Add(strokeColorTextBox);

        var thicknessPanel = new StackPanel();
        var thicknessLabel = new TextBlock
        {
            Text = "Stroke Thickness",
            Style = (Microsoft.UI.Xaml.Style)Microsoft.UI.Xaml.Application.Current.Resources["CaptionTextBlockStyle"],
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 8)
        };
        var strokeThicknessSlider = new Slider
        {
            Minimum = 0.5,
            Maximum = 50,
            Value = ViewModel.EditProfileStrokeThickness,
            TickFrequency = 0.5,
            TickPlacement = Microsoft.UI.Xaml.Controls.Primitives.TickPlacement.BottomRight
        };
        var thicknessValueText = new TextBlock
        {
            Text = ViewModel.EditProfileStrokeThickness.ToString("F1"),
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
            Style = (Microsoft.UI.Xaml.Style)Microsoft.UI.Xaml.Application.Current.Resources["CaptionTextBlockStyle"]
        };
        strokeThicknessSlider.ValueChanged += (s, args) =>
        {
            thicknessValueText.Text = args.NewValue.ToString("F1");
        };
        thicknessPanel.Children.Add(thicknessLabel);
        thicknessPanel.Children.Add(strokeThicknessSlider);
        thicknessPanel.Children.Add(thicknessValueText);
        stackPanel.Children.Add(thicknessPanel);

        var fillColorTextBox = new TextBox
        {
            Header = "Fill Color (Hex or Transparent)",
            Text = ViewModel.EditProfileFillColor,
            PlaceholderText = "Transparent or #RRGGBB"
        };
        stackPanel.Children.Add(fillColorTextBox);

        var scrollViewer = new ScrollViewer
        {
            MaxHeight = 500,
            Content = stackPanel
        };
        dialog.Content = scrollViewer;

        dialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();
            try
            {
                if (string.IsNullOrWhiteSpace(profileNameTextBox.Text))
                {
                    args.Cancel = true;
                    return;
                }

                ViewModel.EditProfileName = profileNameTextBox.Text.Trim();
                ViewModel.EditProfileTheme = (themeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "System";
                
                if (int.TryParse(canvasWidthTextBox.Text, out int width) && width >= 100 && width <= 10000)
                {
                    ViewModel.EditProfileCanvasWidth = width;
                }
                
                if (int.TryParse(canvasHeightTextBox.Text, out int height) && height >= 100 && height <= 10000)
                {
                    ViewModel.EditProfileCanvasHeight = height;
                }
                
                ViewModel.EditProfileStrokeColor = strokeColorTextBox.Text.Trim();
                ViewModel.EditProfileStrokeThickness = strokeThicknessSlider.Value;
                ViewModel.EditProfileFillColor = fillColorTextBox.Text.Trim();

                // Validate stroke color format
                if (!string.IsNullOrWhiteSpace(ViewModel.EditProfileStrokeColor) && 
                    !ViewModel.EditProfileStrokeColor.StartsWith("#") && 
                    ViewModel.EditProfileStrokeColor != "Transparent")
                {
                    ViewModel.EditProfileStrokeColor = "#" + ViewModel.EditProfileStrokeColor;
                }

                await ViewModel.UpdateProfileCommand.ExecuteAsync(null);
            }
            finally
            {
                deferral.Complete();
            }
        };

        dialog.SecondaryButtonClick += (s, args) =>
        {
            ViewModel.CloseEditDialogCommand.Execute(null);
        };

        await dialog.ShowAsync();
    }
}

