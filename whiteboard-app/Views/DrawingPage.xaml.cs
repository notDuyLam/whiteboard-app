using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using whiteboard_app.Controls;
using whiteboard_app.Services;
using whiteboard_app_data.Enums;
using whiteboard_app_data.Models;
using whiteboard_app_data.Models.ShapeTypes;
using CanvasModel = whiteboard_app_data.Models.Canvas;
using StrokeStyleEnum = whiteboard_app_data.Enums.StrokeStyle;
using XamlCanvas = Microsoft.UI.Xaml.Controls.Canvas;

namespace whiteboard_app.Views;

/// <summary>
/// Drawing page with canvas and drawing tools.
/// </summary>
public sealed partial class DrawingPage : Page
{
    private readonly IDataService? _dataService;
    private readonly IDrawingService? _drawingService;
    private CanvasModel? _currentCanvas;
    private Profile? _currentProfile;

    public DrawingPage()
    {
        InitializeComponent();
        InitializeStrokeSettings();
        
        // Get services from DI
        _dataService = App.ServiceProvider?.GetService(typeof(IDataService)) as IDataService;
        _drawingService = App.ServiceProvider?.GetService(typeof(IDrawingService)) as IDrawingService;
        
        // Subscribe to shape selection events
        if (DrawingCanvasControl != null)
        {
            DrawingCanvasControl.ShapeSelected += DrawingCanvasControl_ShapeSelected;
            DrawingCanvasControl.ShapeDrawingCompleted += DrawingCanvasControl_ShapeDrawingCompleted;
        }
    }

    /// <summary>
    /// Called when the page is navigated to.
    /// </summary>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // If a canvas parameter is passed, load it
        if (e.Parameter != null)
        {
            if (e.Parameter is CanvasModel canvas)
            {
                // Load canvas with its profile
                if (canvas.Profile != null)
                {
                    _currentProfile = canvas.Profile;
                    ApplyProfileSettings(_currentProfile);
                }
                SetCanvas(canvas);
                await LoadCanvasShapesAsync(canvas);
            }
            else if (e.Parameter is Guid canvasId && _dataService != null)
            {
                var loadedCanvas = await _dataService.GetCanvasByIdAsync(canvasId);
                if (loadedCanvas != null)
                {
                    if (loadedCanvas.Profile != null)
                    {
                        _currentProfile = loadedCanvas.Profile;
                        ApplyProfileSettings(_currentProfile);
                    }
                    SetCanvas(loadedCanvas);
                    await LoadCanvasShapesAsync(loadedCanvas);
                }
            }
            else if (e.Parameter is Profile profile)
            {
                // Profile passed from HomePage - create new canvas (not saved yet)
                _currentProfile = profile;
                ApplyProfileSettings(_currentProfile);
                
                // Create a temporary canvas (not saved to DB yet)
                _currentCanvas = null;
                if (DrawingCanvasControl != null)
                {
                    DrawingCanvasControl.ClearAllShapes();
                    DrawingCanvasControl.CanvasModel = null;
                    // Set canvas size from profile
                    DrawingCanvasControl.Width = profile.DefaultCanvasWidth;
                    DrawingCanvasControl.Height = profile.DefaultCanvasHeight;
                }
            }
        }
        else
        {
            // No parameter - require profile selection
            await ShowProfileRequiredDialog();
        }
    }

    /// <summary>
    /// Loads shapes from the database for the given canvas and renders them.
    /// </summary>
    private async Task LoadCanvasShapesAsync(CanvasModel canvas)
    {
        System.Diagnostics.Debug.WriteLine($"[DrawingPage] LoadCanvasShapesAsync - START: Canvas={canvas.Name} (Id: {canvas.Id})");
        
        if (_dataService == null || _drawingService == null || DrawingCanvasControl == null)
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] LoadCanvasShapesAsync - ERROR: Required services or control is null");
            return;
        }

        try
        {
            // Load shapes from database
            System.Diagnostics.Debug.WriteLine("[DrawingPage] LoadCanvasShapesAsync - Loading shapes from database...");
            var shapes = await _dataService.GetShapesByCanvasIdAsync(canvas.Id);
            
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] LoadCanvasShapesAsync - Loaded {shapes.Count} shapes from database");
            
            // Clear existing shapes first
            DrawingCanvasControl.ClearAllShapes();
            
            // Render each shape from database
            int renderedCount = 0;
            foreach (var shape in shapes)
            {
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] LoadCanvasShapesAsync - Rendering shape: Type={shape.ShapeType}, Id={shape.Id}");
                DrawingCanvasControl.RenderShapeFromDatabase(shape, _drawingService);
                renderedCount++;
            }
            
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] LoadCanvasShapesAsync - Rendered {renderedCount} shapes");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] LoadCanvasShapesAsync - ERROR: {ex.Message}\n{ex.StackTrace}");
            // Error loading shapes - silently fail for now
        }
    }

    private void InitializeStrokeSettings()
    {
        // Set default stroke style to Solid
        if (StrokeStyleComboBox != null)
        {
            StrokeStyleComboBox.SelectedIndex = 0;
        }
        
        // Set default stroke color
        if (StrokeColorTextBox != null)
        {
            StrokeColorTextBox.Text = "#000000";
        }
        
        // Set default stroke thickness
        if (StrokeThicknessSlider != null)
        {
            StrokeThicknessSlider.Value = 2;
        }
        if (StrokeThicknessTextBlock != null)
        {
            StrokeThicknessTextBlock.Text = "2";
        }
        
        // Set default fill color
        if (FillColorTextBox != null)
        {
            FillColorTextBox.Text = "Transparent";
        }
        
        // Apply initial settings to canvas
        ApplyStrokeSettings();
        ApplyFillColor();
    }

    /// <summary>
    /// Applies profile settings to the drawing page (stroke color, thickness, canvas size, theme).
    /// </summary>
    private void ApplyProfileSettings(Profile profile)
    {
        if (profile == null)
            return;

        // Apply stroke color
        if (StrokeColorTextBox != null)
        {
            StrokeColorTextBox.Text = profile.DefaultStrokeColor;
        }

        // Apply stroke thickness
        if (StrokeThicknessSlider != null)
        {
            StrokeThicknessSlider.Value = profile.DefaultStrokeThickness;
        }
        if (StrokeThicknessTextBlock != null)
        {
            StrokeThicknessTextBlock.Text = profile.DefaultStrokeThickness.ToString("F1");
        }

        // Apply fill color
        if (FillColorTextBox != null)
        {
            FillColorTextBox.Text = profile.DefaultFillColor;
        }

        // Apply canvas size
        if (DrawingCanvasControl != null)
        {
            DrawingCanvasControl.Width = profile.DefaultCanvasWidth;
            DrawingCanvasControl.Height = profile.DefaultCanvasHeight;
        }

        // Apply theme (if supported)
        // Note: Theme application might need to be handled at application level
        
        // Apply settings to canvas
        ApplyStrokeSettings();
        ApplyFillColor();
    }

    /// <summary>
    /// Shows a dialog requiring the user to select a profile before drawing.
    /// </summary>
    private async Task ShowProfileRequiredDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "Profile Required",
            Content = "You must select a profile before drawing. Please go to the Home page and select a profile first.",
            PrimaryButtonText = "Go to Home",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Navigate to HomePage
            var navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
            navigationService?.NavigateTo(typeof(Views.HomePage));
        }
        else
        {
            // Navigate back if user cancels
            var navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
            if (navigationService?.CanGoBack == true)
            {
                navigationService.GoBack();
            }
            else
            {
                navigationService?.NavigateTo(typeof(Views.HomePage));
            }
        }
    }

    private void ApplyStrokeSettings()
    {
        if (DrawingCanvasControl == null)
            return;

        // Apply stroke style
        if (StrokeStyleComboBox?.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string styleTag)
        {
            DrawingCanvasControl.StrokeStyle = styleTag switch
            {
                "Solid" => StrokeStyleEnum.Solid,
                "Dash" => StrokeStyleEnum.Dash,
                "Dot" => StrokeStyleEnum.Dot,
                _ => StrokeStyleEnum.Solid
            };
        }
        
        // Apply stroke color
        if (StrokeColorTextBox != null)
        {
            DrawingCanvasControl.StrokeColor = StrokeColorTextBox.Text;
        }
        
        // Apply stroke thickness
        if (StrokeThicknessSlider != null)
        {
            DrawingCanvasControl.StrokeThickness = StrokeThicknessSlider.Value;
        }
    }

    private void ApplyFillColor()
    {
        if (DrawingCanvasControl == null)
            return;
        
        // Apply fill color
        if (FillColorTextBox != null)
        {
            DrawingCanvasControl.FillColor = FillColorTextBox.Text;
        }
    }

    private void FillColorTextBox_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
    {
        ApplyFillColor();
    }

    private void LineToolButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Set Line as the current drawing tool
        DrawingCanvasControl.CurrentShapeType = ShapeType.Line;
        
        // Update button states (visual feedback)
        UpdateToolButtonStates(LineToolButton);
    }

    private void RectangleToolButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Set Rectangle as the current drawing tool
        DrawingCanvasControl.CurrentShapeType = ShapeType.Rectangle;
        
        // Update button states (visual feedback)
        UpdateToolButtonStates(RectangleToolButton);
    }

    private void OvalToolButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Set Oval as the current drawing tool
        DrawingCanvasControl.CurrentShapeType = ShapeType.Oval;
        
        // Update button states (visual feedback)
        UpdateToolButtonStates(OvalToolButton);
    }

    private void CircleToolButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Set Circle as the current drawing tool
        DrawingCanvasControl.CurrentShapeType = ShapeType.Circle;
        
        // Update button states (visual feedback)
        UpdateToolButtonStates(CircleToolButton);
    }

    private void TriangleToolButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Set Triangle as the current drawing tool
        DrawingCanvasControl.CurrentShapeType = ShapeType.Triangle;
        
        // Update button states (visual feedback)
        UpdateToolButtonStates(TriangleToolButton);
    }

    private void PolygonToolButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Set Polygon as the current drawing tool
        DrawingCanvasControl.CurrentShapeType = ShapeType.Polygon;
        
        // Update button states (visual feedback)
        UpdateToolButtonStates(PolygonToolButton);
    }

    private void UpdateToolButtonStates(Button? activeButton)
    {
        // Reset all tool buttons
        LineToolButton.Style = null;
        RectangleToolButton.Style = null;
        OvalToolButton.Style = null;
        CircleToolButton.Style = null;
        TriangleToolButton.Style = null;
        PolygonToolButton.Style = null;
        
        // Set active button style
        if (activeButton != null && activeButton.Resources.TryGetValue("AccentButtonStyle", out var accentStyle))
        {
            activeButton.Style = accentStyle as Microsoft.UI.Xaml.Style;
        }
        else if (activeButton != null)
        {
            // Fallback: try Application resources
            var appResources = Microsoft.UI.Xaml.Application.Current.Resources;
            if (appResources.TryGetValue("AccentButtonStyle", out var appStyle))
            {
                activeButton.Style = appStyle as Microsoft.UI.Xaml.Style;
            }
        }
    }

    private void StrokeStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyStrokeSettings();
    }

    private void StrokeColorTextBox_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
    {
        ApplyStrokeSettings();
    }

    private void StrokeThicknessSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (StrokeThicknessTextBlock != null)
        {
            StrokeThicknessTextBlock.Text = ((int)e.NewValue).ToString();
        }
        ApplyStrokeSettings();
    }

    private void SelectionModeToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (SelectionModeToggle != null && DrawingCanvasControl != null)
        {
            DrawingCanvasControl.IsSelectionMode = SelectionModeToggle.IsOn;
            
            // Update tool buttons state
            if (SelectionModeToggle.IsOn)
            {
                // Disable drawing tools when in selection mode
                LineToolButton.IsEnabled = false;
                RectangleToolButton.IsEnabled = false;
                OvalToolButton.IsEnabled = false;
                CircleToolButton.IsEnabled = false;
                TriangleToolButton.IsEnabled = false;
                PolygonToolButton.IsEnabled = false;
            }
            else
            {
                // Enable drawing tools when in drawing mode
                LineToolButton.IsEnabled = true;
                RectangleToolButton.IsEnabled = true;
                OvalToolButton.IsEnabled = true;
                CircleToolButton.IsEnabled = true;
                TriangleToolButton.IsEnabled = true;
                PolygonToolButton.IsEnabled = true;
                
                // Clear selection when switching back to drawing mode
                UpdateEditButtonsState();
            }
        }
    }

    private async void EditShapeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DrawingCanvasControl?.SelectedShape == null)
            return;

        // Get selected shape properties
        var (strokeColor, strokeThickness, fillColor, strokeStyle) = DrawingCanvasControl.GetSelectedShapeProperties();

        var dialog = new ContentDialog
        {
            Title = "Edit Shape Properties",
            PrimaryButtonText = "Apply",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var stackPanel = new StackPanel { Spacing = 16 };

        // Stroke Style
        var strokeStyleComboBox = new ComboBox
        {
            Header = "Stroke Style",
            SelectedIndex = strokeStyle switch
            {
                StrokeStyleEnum.Solid => 0,
                StrokeStyleEnum.Dash => 1,
                StrokeStyleEnum.Dot => 2,
                _ => 0
            }
        };
        strokeStyleComboBox.Items.Add(new ComboBoxItem { Content = "Solid", Tag = "Solid" });
        strokeStyleComboBox.Items.Add(new ComboBoxItem { Content = "Dash", Tag = "Dash" });
        strokeStyleComboBox.Items.Add(new ComboBoxItem { Content = "Dot", Tag = "Dot" });
        stackPanel.Children.Add(strokeStyleComboBox);

        // Stroke Color
        var strokeColorTextBox = new TextBox
        {
            Header = "Stroke Color (Hex)",
            Text = strokeColor,
            PlaceholderText = "#000000"
        };
        stackPanel.Children.Add(strokeColorTextBox);

        // Stroke Thickness
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
            Value = strokeThickness,
            TickFrequency = 0.5,
            TickPlacement = Microsoft.UI.Xaml.Controls.Primitives.TickPlacement.BottomRight
        };
        var thicknessValueText = new TextBlock
        {
            Text = strokeThickness.ToString("F1"),
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

        // Fill Color
        var fillColorTextBox = new TextBox
        {
            Header = "Fill Color (Hex or Transparent)",
            Text = fillColor,
            PlaceholderText = "Transparent or #RRGGBB"
        };
        stackPanel.Children.Add(fillColorTextBox);

        var scrollViewer = new ScrollViewer
        {
            MaxHeight = 400,
            Content = stackPanel
        };
        dialog.Content = scrollViewer;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // Get selected stroke style
            var selectedStrokeStyle = StrokeStyleEnum.Solid;
            if (strokeStyleComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string styleTag)
            {
                selectedStrokeStyle = styleTag switch
                {
                    "Solid" => StrokeStyleEnum.Solid,
                    "Dash" => StrokeStyleEnum.Dash,
                    "Dot" => StrokeStyleEnum.Dot,
                    _ => StrokeStyleEnum.Solid
                };
            }

            // Update shape properties
            DrawingCanvasControl.UpdateSelectedShapeProperties(
                strokeColorTextBox.Text.Trim(),
                strokeThicknessSlider.Value,
                fillColorTextBox.Text.Trim(),
                selectedStrokeStyle
            );
        }
    }

    /// <summary>
    /// Handles the Fill Selected Shape button click event.
    /// </summary>
    private async void FillShapeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DrawingCanvasControl?.SelectedShape == null)
        {
            ShowSaveNotification("No shape selected", isError: true);
            return;
        }

        if (FillColorTextBox == null)
        {
            ShowSaveNotification("Fill color field not found", isError: true);
            return;
        }

        var fillColor = FillColorTextBox.Text?.Trim() ?? "Transparent";
        
        // Get current shape properties
        var (strokeColor, strokeThickness, currentFillColor, strokeStyle) = DrawingCanvasControl.GetSelectedShapeProperties();
        
        // Update only the fill color
        DrawingCanvasControl.UpdateSelectedShapeProperties(
            strokeColor,
            strokeThickness,
            fillColor,
            strokeStyle
        );
        
        // Update the shape in database only if it's already saved and exists in DB
        var selectedShapeModel = DrawingCanvasControl.GetSelectedShapeModel();
        if (selectedShapeModel != null && _dataService != null && _currentCanvas != null)
        {
            // Check if shape exists in database
            bool shapeExistsInDb = false;
            if (selectedShapeModel.Id != Guid.Empty)
            {
                try
                {
                    var existingShape = await _dataService.GetShapeByIdAsync(selectedShapeModel.Id);
                    shapeExistsInDb = existingShape != null;
                }
                catch
                {
                    shapeExistsInDb = false;
                }
            }
            
            if (shapeExistsInDb)
            {
                // Shape exists in DB - update it
                try
                {
                    selectedShapeModel.FillColor = fillColor;
                    await _dataService.UpdateShapeAsync(selectedShapeModel);
                    
                    // Update canvas last modified date
                    _currentCanvas.LastModifiedDate = DateTime.UtcNow;
                    await _dataService.UpdateCanvasAsync(_currentCanvas);
                    
                    ShowSaveNotification($"Shape filled with {fillColor} (saved)");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DrawingPage] FillShapeButton_Click - ERROR: {ex.Message}\n{ex.StackTrace}");
                    ShowSaveNotification("Failed to save fill color to database", isError: true);
                }
            }
            else
            {
                // Shape is still in draft (not saved to DB yet) - just update UI, will be saved when user clicks Save
                ShowSaveNotification($"Shape filled with {fillColor} (will be saved when you click Save)");
            }
        }
        else
        {
            ShowSaveNotification($"Shape filled with {fillColor}");
        }
    }

    private void DeleteShapeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DrawingCanvasControl?.SelectedShape == null)
            return;

        // Remove the selected shape using the proper method
        var selectedShape = DrawingCanvasControl.SelectedShape;
        DrawingCanvasControl.RemoveShape(selectedShape);
        
        // Clear selection
        DrawingCanvasControl.IsSelectionMode = false;
        SelectionModeToggle.IsOn = false;
        
        // Update button states
        UpdateEditButtonsState();
    }

    private void UpdateEditButtonsState()
    {
        var hasSelection = DrawingCanvasControl?.SelectedShape != null;
        
        if (EditShapeButton != null)
        {
            EditShapeButton.IsEnabled = hasSelection;
        }
        
        if (FillShapeButton != null)
        {
            FillShapeButton.IsEnabled = hasSelection;
        }
        
        if (DeleteShapeButton != null)
        {
            DeleteShapeButton.IsEnabled = hasSelection;
        }
        
        if (SaveAsTemplateButton != null)
        {
            SaveAsTemplateButton.IsEnabled = hasSelection;
        }
    }

    private void DrawingCanvasControl_ShapeSelected(object? sender, EventArgs e)
    {
        UpdateEditButtonsState();
    }

    private void DrawingCanvasControl_ShapeDrawingCompleted(object? sender, ShapeDrawingCompletedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[DrawingPage] ShapeDrawingCompleted - START: Type={e.ShapeType}");
        
        // Only serialize shape data and update _shapeMap - DO NOT save to database yet
        // Shapes will only be saved when user clicks Save button
        if (_drawingService == null || DrawingCanvasControl == null)
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] ShapeDrawingCompleted - WARNING: _drawingService or DrawingCanvasControl is null");
            return;
        }
        
        // Serialize shape data
        string serializedData = e.ShapeType switch
        {
            ShapeType.Line => _drawingService.SerializeShapeData(new LineShapeData
            {
                StartX = e.StartPoint.X,
                StartY = e.StartPoint.Y,
                EndX = e.EndPoint.X,
                EndY = e.EndPoint.Y
            }),
            ShapeType.Rectangle => _drawingService.SerializeShapeData(new RectangleShapeData
            {
                X = Math.Min(e.StartPoint.X, e.EndPoint.X),
                Y = Math.Min(e.StartPoint.Y, e.EndPoint.Y),
                Width = Math.Abs(e.EndPoint.X - e.StartPoint.X),
                Height = Math.Abs(e.EndPoint.Y - e.StartPoint.Y)
            }),
            ShapeType.Oval => _drawingService.SerializeShapeData(new OvalShapeData
            {
                CenterX = (e.StartPoint.X + e.EndPoint.X) / 2,
                CenterY = (e.StartPoint.Y + e.EndPoint.Y) / 2,
                RadiusX = Math.Abs(e.EndPoint.X - e.StartPoint.X) / 2,
                RadiusY = Math.Abs(e.EndPoint.Y - e.StartPoint.Y) / 2
            }),
            ShapeType.Circle => _drawingService.SerializeShapeData(new CircleShapeData
            {
                CenterX = e.StartPoint.X,
                CenterY = e.StartPoint.Y,
                Radius = Math.Sqrt(Math.Pow(e.EndPoint.X - e.StartPoint.X, 2) + Math.Pow(e.EndPoint.Y - e.StartPoint.Y, 2))
            }),
            ShapeType.Triangle => e.TrianglePoints != null && e.TrianglePoints.Count == 3
                ? _drawingService.SerializeShapeData(new TriangleShapeData
                {
                    Point1X = e.TrianglePoints[0].X,
                    Point1Y = e.TrianglePoints[0].Y,
                    Point2X = e.TrianglePoints[1].X,
                    Point2Y = e.TrianglePoints[1].Y,
                    Point3X = e.TrianglePoints[2].X,
                    Point3Y = e.TrianglePoints[2].Y
                })
                : string.Empty,
            ShapeType.Polygon => e.PolygonPoints != null && e.PolygonPoints.Count >= 3
                ? _drawingService.SerializeShapeData(new PolygonShapeData
                {
                    Points = e.PolygonPoints.Select(p => new PointData { X = p.X, Y = p.Y }).ToList()
                })
                : string.Empty,
            _ => string.Empty
        };
        
        // Update the shape in _shapeMap with SerializedData
        var allShapes = DrawingCanvasControl.GetAllShapeModels();
        if (allShapes.Count > 0)
        {
            // Get the last shape (most recently added)
            var lastShape = allShapes.Last();
            if (lastShape.ShapeType == e.ShapeType && string.IsNullOrWhiteSpace(lastShape.SerializedData))
            {
                lastShape.SerializedData = serializedData;
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] ShapeDrawingCompleted - Updated SerializedData for shape: Type={e.ShapeType} (draft, not saved to DB yet)");
            }
        }
        
        // DO NOT save to database - shapes are only saved when user clicks Save button
        System.Diagnostics.Debug.WriteLine("[DrawingPage] ShapeDrawingCompleted - Shape is in draft state, will be saved when user clicks Save");
    }

    /// <summary>
    /// Sets the current canvas for this drawing page.
    /// </summary>
    public void SetCanvas(CanvasModel canvas)
    {
        System.Diagnostics.Debug.WriteLine($"[DrawingPage] SetCanvas - START: Canvas={canvas?.Name ?? "null"} (Id: {canvas?.Id})");
        _currentCanvas = canvas;
        if (DrawingCanvasControl != null)
        {
            try
            {
                DrawingCanvasControl.CanvasModel = canvas;
                if (!string.IsNullOrEmpty(canvas?.BackgroundColor))
                {
                    DrawingCanvasControl.BackgroundColor = canvas.BackgroundColor;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] SetCanvas - ERROR setting canvas properties: {ex.Message}");
                // Continue anyway - canvas is still set
            }
        }
        System.Diagnostics.Debug.WriteLine($"[DrawingPage] SetCanvas - END: _currentCanvas is now {(_currentCanvas != null ? $"set (Name: {_currentCanvas.Name}, Id: {_currentCanvas.Id})" : "null")}");
    }


    /// <summary>
    /// Shows a save notification to the user.
    /// </summary>
    private async void ShowSaveNotification(string message, bool isError = false)
    {
        System.Diagnostics.Debug.WriteLine($"[DrawingPage] ShowSaveNotification - START: message={message}, isError={isError}");
        
        if (SaveNotificationInfoBar == null)
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] ShowSaveNotification - SaveNotificationInfoBar is null!");
            // Fallback: Use ContentDialog if InfoBar is not available
            var dialog = new ContentDialog
            {
                Title = isError ? "Error" : "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        System.Diagnostics.Debug.WriteLine("[DrawingPage] ShowSaveNotification - Setting InfoBar properties");
        SaveNotificationInfoBar.Message = message;
        SaveNotificationInfoBar.Severity = isError 
            ? InfoBarSeverity.Error 
            : InfoBarSeverity.Success;
        
        System.Diagnostics.Debug.WriteLine("[DrawingPage] ShowSaveNotification - Opening InfoBar");
        SaveNotificationInfoBar.IsOpen = true;
        System.Diagnostics.Debug.WriteLine($"[DrawingPage] ShowSaveNotification - InfoBar.IsOpen = {SaveNotificationInfoBar.IsOpen}");

        // Auto-hide after 3 seconds
        await Task.Delay(3000);
        if (SaveNotificationInfoBar != null)
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] ShowSaveNotification - Closing InfoBar");
            SaveNotificationInfoBar.IsOpen = false;
        }
    }

    /// <summary>
    /// Handles the Save Canvas button click.
    /// </summary>
    private async void SaveCanvasButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DrawingPage] SaveCanvasButton_Click - START");
        
        if (_dataService == null)
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] SaveCanvasButton_Click - DataService is null");
            ShowSaveNotification("Data service unavailable", isError: true);
            return;
        }

        if (_currentProfile == null)
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] SaveCanvasButton_Click - No profile selected");
            ShowSaveNotification("No profile selected. Please select a profile first.", isError: true);
            await ShowProfileRequiredDialog();
            return;
        }

        try
        {
            // If canvas doesn't exist yet, create it with a name
            if (_currentCanvas == null)
            {
                // Show dialog to get canvas name
                var nameDialog = new ContentDialog
                {
                    Title = "Save Canvas",
                    PrimaryButtonText = "Save",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };

                var stackPanel = new StackPanel { Spacing = 16 };
                var nameTextBox = new TextBox
                {
                    Header = "Canvas Name *",
                    PlaceholderText = "Enter canvas name",
                    MaxLength = 200
                };
                stackPanel.Children.Add(nameTextBox);
                nameDialog.Content = stackPanel;

                // Focus on text box when dialog opens
                nameDialog.Opened += (s, args) => nameTextBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);

                var result = await nameDialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    return; // User cancelled
                }

                var canvasName = nameTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(canvasName))
                {
                    ShowSaveNotification("Canvas name cannot be empty", isError: true);
                    return;
                }

                // Create new canvas
                _currentCanvas = new CanvasModel
                {
                    Name = canvasName,
                    ProfileId = _currentProfile.Id,
                    Width = DrawingCanvasControl?.Width != null ? (int)DrawingCanvasControl.Width : _currentProfile.DefaultCanvasWidth,
                    Height = DrawingCanvasControl?.Height != null ? (int)DrawingCanvasControl.Height : _currentProfile.DefaultCanvasHeight,
                    BackgroundColor = DrawingCanvasControl?.Background?.ToString() ?? "#FFFFFF"
                };

                _currentCanvas = await _dataService.CreateCanvasAsync(_currentCanvas);
                SetCanvas(_currentCanvas);
                
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - Created new canvas: {_currentCanvas.Name} (Id: {_currentCanvas.Id})");
            }
            else
            {
                // Update existing canvas
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - Updating canvas: {_currentCanvas.Name} (Id: {_currentCanvas.Id})");
                _currentCanvas.LastModifiedDate = DateTime.UtcNow;
                await _dataService.UpdateCanvasAsync(_currentCanvas);
            }
            
            // Sync shapes: Save all shapes currently on canvas, delete shapes that are no longer on canvas
            if (DrawingCanvasControl != null && _drawingService != null)
            {
                // Get all shapes currently on canvas (draft + saved)
                var shapesOnCanvas = DrawingCanvasControl.GetAllShapeModels();
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - Found {shapesOnCanvas.Count} shapes on canvas");
                
                // Get all shapes currently in database for this canvas
                var shapesInDb = await _dataService.GetShapesByCanvasIdAsync(_currentCanvas.Id);
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - Found {shapesInDb.Count} shapes in database");
                
                // Create a set of shape IDs currently on canvas (for quick lookup)
                var shapesOnCanvasIds = new HashSet<Guid>();
                foreach (var shape in shapesOnCanvas)
                {
                    if (shape.Id != Guid.Empty)
                    {
                        shapesOnCanvasIds.Add(shape.Id);
                    }
                }
                
                // Delete shapes that are in DB but no longer on canvas
                foreach (var dbShape in shapesInDb)
                {
                    if (!shapesOnCanvasIds.Contains(dbShape.Id))
                    {
                        System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - Deleting shape from DB: Id={dbShape.Id}, Type={dbShape.ShapeType}");
                        await _dataService.DeleteShapeAsync(dbShape.Id);
                    }
                }
                
                // Save/Update all shapes currently on canvas
                foreach (var shape in shapesOnCanvas)
                {
                    if (string.IsNullOrWhiteSpace(shape.SerializedData))
                    {
                        System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - WARNING: Shape has no SerializedData, skipping");
                        continue;
                    }
                    
                    shape.CanvasId = _currentCanvas.Id;
                    
                    try
                    {
                        // Check if shape exists in database
                        bool shapeExistsInDb = false;
                        if (shape.Id != Guid.Empty)
                        {
                            var existingShape = await _dataService.GetShapeByIdAsync(shape.Id);
                            shapeExistsInDb = existingShape != null;
                        }
                        
                        if (!shapeExistsInDb)
                        {
                            // Shape doesn't exist in DB (new draft or Id was generated but not saved) - create it
                            // Reset Id to Guid.Empty to let CreateShapeAsync generate a new one
                            var originalId = shape.Id;
                            shape.Id = Guid.Empty;
                            var savedShape = await _dataService.CreateShapeAsync(shape);
                            System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - Created new shape: Type={savedShape.ShapeType}, Id={savedShape.Id} (original draft Id was {originalId})");
                            
                            // Update the shape in _shapeMap with the saved entity
                            DrawingCanvasControl.UpdateLastShapeEntity(savedShape);
                        }
                        else
                        {
                            // Existing shape in DB - update it
                            await _dataService.UpdateShapeAsync(shape);
                            System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - Updated shape: Type={shape.ShapeType}, Id={shape.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - ERROR saving shape: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine("[DrawingPage] SaveCanvasButton_Click - Canvas saved successfully");
            ShowSaveNotification("Canvas saved successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] SaveCanvasButton_Click - ERROR: {ex.Message}\n{ex.StackTrace}");
            ShowSaveNotification($"Failed to save canvas: {ex.Message}", isError: true);
        }
    }

    /// <summary>
    /// Handles the Delete Canvas button click event.
    /// </summary>
    private async void DeleteCanvasButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_currentCanvas == null || _dataService == null)
        {
            ShowSaveNotification("No canvas selected", isError: true);
            return;
        }

        // Show confirmation dialog
        var confirmDialog = new ContentDialog
        {
            Title = "Delete Canvas",
            Content = $"Are you sure you want to delete canvas '{_currentCanvas.Name}'? This action cannot be undone and all shapes on this canvas will be deleted.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = XamlRoot
        };

        var result = await confirmDialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return; // User cancelled
        }

        try
        {
            // Delete canvas from database (cascade delete will handle shapes)
            bool deleted = await _dataService.DeleteCanvasAsync(_currentCanvas.Id);
            
            if (deleted)
            {
                // Clear all shapes from the drawing canvas
                if (DrawingCanvasControl != null)
                {
                    DrawingCanvasControl.ClearAllShapes();
                }

                // Clear current canvas reference
                _currentCanvas = null;
                if (DrawingCanvasControl != null)
                {
                    DrawingCanvasControl.CanvasModel = null;
                }

                ShowSaveNotification("Canvas deleted successfully");
            }
            else
            {
                ShowSaveNotification("Failed to delete canvas", isError: true);
            }
        }
        catch (Exception)
        {
            ShowSaveNotification("Failed to delete canvas", isError: true);
        }
    }

    /// <summary>
    /// Handles the Save as Template button click event.
    /// </summary>
    private async void SaveAsTemplateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DrawingCanvasControl == null || _dataService == null || _drawingService == null)
        {
            ShowSaveNotification("No shape selected", isError: true);
            return;
        }

        var selectedShapeModel = DrawingCanvasControl.GetSelectedShapeModel();
        if (selectedShapeModel == null)
        {
            ShowSaveNotification("No shape selected", isError: true);
            return;
        }

        // Show dialog to enter template name
        var dialog = new ContentDialog
        {
            Title = "Save Shape as Template",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var stackPanel = new StackPanel { Spacing = 16 };
        
        var templateNameTextBox = new TextBox
        {
            Header = "Template Name *",
            PlaceholderText = "Enter template name",
            MaxLength = 200
        };
        stackPanel.Children.Add(templateNameTextBox);

        var errorTextBlock = new TextBlock
        {
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Visibility = Microsoft.UI.Xaml.Visibility.Collapsed
        };
        stackPanel.Children.Add(errorTextBlock);

        dialog.Content = stackPanel;

        dialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();
            try
            {
                errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                errorTextBlock.Text = string.Empty;

                var templateName = templateNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(templateName))
                {
                    errorTextBlock.Text = "Template name is required.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (templateName.Length > 200)
                {
                    errorTextBlock.Text = "Template name must be 200 characters or less.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                // Check if template name already exists
                var existingTemplates = await _dataService.GetAllTemplatesAsync();
                if (existingTemplates.Any(t => t.TemplateName == templateName))
                {
                    errorTextBlock.Text = "A template with this name already exists.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                // Get selected shape model
                var selectedShapeModel = DrawingCanvasControl.GetSelectedShapeModel();
                if (selectedShapeModel == null)
                {
                    errorTextBlock.Text = "No shape selected.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
                
                // Get selected shape properties including stroke style
                var selectedXamlShape = DrawingCanvasControl.SelectedShape;
                if (selectedXamlShape == null)
                {
                    errorTextBlock.Text = "No shape selected.";
                    errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
                
                // Get stroke style from selected shape model (preferred) or from properties
                var strokeStyle = selectedShapeModel.StrokeStyle;
                if (strokeStyle == whiteboard_app_data.Enums.StrokeStyle.Solid && selectedShapeModel.StrokeStyle == 0)
                {
                    // Fallback to getting from shape properties
                    var shapeProperties = DrawingCanvasControl.GetSelectedShapeProperties();
                    strokeStyle = shapeProperties.StrokeStyle ?? whiteboard_app_data.Enums.StrokeStyle.Solid;
                }
                
                // Get or create serialized data from selected shape
                string serializedData = selectedShapeModel.SerializedData;
                
                // If SerializedData is empty, create it from the selected XamlShape
                if (string.IsNullOrWhiteSpace(serializedData))
                {
                    // Serialize shape data based on type - use selectedShapeModel.ShapeType, not XamlShape type
                    serializedData = selectedShapeModel.ShapeType switch
                    {
                        ShapeType.Line when selectedXamlShape is Microsoft.UI.Xaml.Shapes.Line line =>
                            _drawingService.SerializeShapeData(new LineShapeData
                            {
                                StartX = line.X1,
                                StartY = line.Y1,
                                EndX = line.X2,
                                EndY = line.Y2
                            }),
                        ShapeType.Rectangle when selectedXamlShape is Microsoft.UI.Xaml.Shapes.Rectangle rect =>
                            _drawingService.SerializeShapeData(new RectangleShapeData
                            {
                                X = XamlCanvas.GetLeft(rect),
                                Y = XamlCanvas.GetTop(rect),
                                Width = rect.Width,
                                Height = rect.Height
                            }),
                        ShapeType.Oval when selectedXamlShape is Microsoft.UI.Xaml.Shapes.Ellipse ellipse =>
                            _drawingService.SerializeShapeData(new OvalShapeData
                            {
                                CenterX = XamlCanvas.GetLeft(ellipse) + ellipse.Width / 2,
                                CenterY = XamlCanvas.GetTop(ellipse) + ellipse.Height / 2,
                                RadiusX = ellipse.Width / 2,
                                RadiusY = ellipse.Height / 2
                            }),
                        ShapeType.Circle when selectedXamlShape is Microsoft.UI.Xaml.Shapes.Ellipse ellipse =>
                            _drawingService.SerializeShapeData(new CircleShapeData
                            {
                                CenterX = XamlCanvas.GetLeft(ellipse) + ellipse.Width / 2,
                                CenterY = XamlCanvas.GetTop(ellipse) + ellipse.Height / 2,
                                Radius = Math.Min(ellipse.Width, ellipse.Height) / 2
                            }),
                        ShapeType.Triangle when selectedXamlShape is Microsoft.UI.Xaml.Shapes.Polygon polygon && polygon.Points.Count == 3 =>
                            _drawingService.SerializeShapeData(new TriangleShapeData
                            {
                                Point1X = polygon.Points[0].X,
                                Point1Y = polygon.Points[0].Y,
                                Point2X = polygon.Points[1].X,
                                Point2Y = polygon.Points[1].Y,
                                Point3X = polygon.Points[2].X,
                                Point3Y = polygon.Points[2].Y
                            }),
                        ShapeType.Polygon when selectedXamlShape is Microsoft.UI.Xaml.Shapes.Polygon polygon =>
                            _drawingService.SerializeShapeData(new PolygonShapeData
                            {
                                Points = polygon.Points.Select(p => new PointData { X = p.X, Y = p.Y }).ToList()
                            }),
                        _ => string.Empty
                    };
                    
                    if (string.IsNullOrWhiteSpace(serializedData))
                    {
                        errorTextBlock.Text = "Selected shape has invalid data and cannot be saved as template.";
                        errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        args.Cancel = true;
                        return;
                    }
                }

                // Create template shape (copy of selected shape)
                // Template shapes have CanvasId = null and IsTemplate = true
                var templateShape = _drawingService.CreateShape(
                    selectedShapeModel.ShapeType,
                    null, // CanvasId is null for templates
                    selectedShapeModel.StrokeColor,
                    selectedShapeModel.StrokeThickness,
                    selectedShapeModel.FillColor,
                    serializedData
                );

                // Set template properties
                templateShape.IsTemplate = true;
                templateShape.TemplateName = templateName.Trim();
                templateShape.CanvasId = null; // Templates are not associated with any canvas
                templateShape.StrokeStyle = strokeStyle; // Save stroke style
                
                // Save template to database
                await _dataService.CreateShapeAsync(templateShape);
            }
            catch (Exception ex)
            {
                // Build detailed error message for debugging
                var errorMessage = $"Failed to save template: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\n\nInner-Inner: {ex.InnerException.InnerException.Message}";
                    }
                }
                errorMessage += $"\n\nException Type: {ex.GetType().Name}";
                if (ex is System.InvalidOperationException invalidOpEx)
                {
                    errorMessage += $"\n\nFull Exception Details:\n{ex}";
                }
                
                errorTextBlock.Text = errorMessage;
                errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ShowSaveNotification("Shape saved as template successfully");
        }
    }

    /// <summary>
    /// Handles the Load Template button click event.
    /// </summary>
    private async void LoadTemplateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_dataService == null)
        {
            ShowSaveNotification("Cannot load template: Data service not available", isError: true);
            return;
        }
        
        if (_drawingService == null)
        {
            ShowSaveNotification("Cannot load template: Drawing service not available", isError: true);
            return;
        }
        
        if (DrawingCanvasControl == null)
        {
            ShowSaveNotification("Cannot load template: Drawing canvas not available", isError: true);
            return;
        }

        try
        {
            // Load all templates
            var templates = await _dataService.GetAllTemplatesAsync();

            if (templates.Count == 0)
            {
                ShowSaveNotification("No templates available", isError: true);
                return;
            }

            // Show dialog with template list
            var xamlRoot = this.XamlRoot;
            
            if (xamlRoot == null)
            {
                ShowSaveNotification("Cannot show dialog: XamlRoot is not available", isError: true);
                return;
            }
            
            var dialog = new ContentDialog
            {
                Title = "Load Template to Canvas",
                PrimaryButtonText = "Load",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot,
                MaxWidth = 600
            };

            var scrollViewer = new ScrollViewer
            {
                MaxHeight = 400
            };

            var stackPanel = new StackPanel { Spacing = 12 };

            // Template selection
            Shape? selectedTemplate = null;
            
            // Create simple list of template items
            foreach (var template in templates)
            {
                var templateBorder = new Border
                {
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4),
                    Padding = new Microsoft.UI.Xaml.Thickness(12),
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 8),
                    Tag = template
                };

                var templatePanel = new StackPanel();
                var templateNameText = new TextBlock
                {
                    Text = template.TemplateName ?? "Unnamed Template",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    FontSize = 14
                };
                var templateTypeText = new TextBlock
                {
                    Text = $"Type: {template.ShapeType}",
                    FontSize = 12,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0)
                };
                templatePanel.Children.Add(templateNameText);
                templatePanel.Children.Add(templateTypeText);
                templateBorder.Child = templatePanel;

                // Add click handler to select template
                templateBorder.PointerPressed += (s, args) =>
                {
                    // Reset all borders
                    foreach (var child in stackPanel.Children)
                    {
                        if (child is Border b)
                        {
                            b.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                        }
                    }
                    
                    // Highlight selected
                    templateBorder.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightBlue);
                    selectedTemplate = template;
                };

                stackPanel.Children.Add(templateBorder);
            }

            scrollViewer.Content = stackPanel;
            dialog.Content = scrollViewer;

            dialog.PrimaryButtonClick += async (s, args) =>
            {
                var deferral = args.GetDeferral();
                try
                {
                    if (selectedTemplate == null)
                    {
                        ShowSaveNotification("Please select a template", isError: true);
                        args.Cancel = true;
                        return;
                    }

                    // Render template shape on canvas at center position
                    // For Line and Polygon, we need to calculate offset based on the shape's bounding box
                    var canvasWidth = DrawingCanvasControl.ActualWidth > 0 ? DrawingCanvasControl.ActualWidth : (_currentCanvas?.Width ?? 800);
                    var canvasHeight = DrawingCanvasControl.ActualHeight > 0 ? DrawingCanvasControl.ActualHeight : (_currentCanvas?.Height ?? 600);
                    
                    // Calculate offset to center the shape
                    // For Line and Polygon, we'll use a default offset to center them
                    double offsetX = 0;
                    double offsetY = 0;
                    
                    // Try to deserialize to get shape bounds
                    try
                    {
                        if (selectedTemplate.ShapeType == ShapeType.Line)
                        {
                            var lineData = _drawingService.DeserializeShapeData<LineShapeData>(selectedTemplate.SerializedData);
                            if (lineData != null)
                            {
                                // Calculate bounding box
                                var minX = Math.Min(lineData.StartX, lineData.EndX);
                                var minY = Math.Min(lineData.StartY, lineData.EndY);
                                var maxX = Math.Max(lineData.StartX, lineData.EndX);
                                var maxY = Math.Max(lineData.StartY, lineData.EndY);
                                var shapeWidth = maxX - minX;
                                var shapeHeight = maxY - minY;
                                
                                // Center the shape - ensure offset is reasonable
                                offsetX = Math.Max(0, (canvasWidth - shapeWidth) / 2 - minX);
                                offsetY = Math.Max(0, (canvasHeight - shapeHeight) / 2 - minY);
                                
                                // If line is too small or coordinates are negative, use default center
                                if (shapeWidth < 10 || shapeHeight < 10 || minX < 0 || minY < 0)
                                {
                                    offsetX = Math.Max(0, (canvasWidth - 200) / 2);
                                    offsetY = Math.Max(0, (canvasHeight - 200) / 2);
                                }
                            }
                        }
                        else if (selectedTemplate.ShapeType == ShapeType.Polygon)
                        {
                            var polygonData = _drawingService.DeserializeShapeData<PolygonShapeData>(selectedTemplate.SerializedData);
                            if (polygonData != null && polygonData.Points != null && polygonData.Points.Count > 0)
                            {
                                // Calculate bounding box
                                var minX = polygonData.Points.Min(p => p.X);
                                var minY = polygonData.Points.Min(p => p.Y);
                                var maxX = polygonData.Points.Max(p => p.X);
                                var maxY = polygonData.Points.Max(p => p.Y);
                                var shapeWidth = maxX - minX;
                                var shapeHeight = maxY - minY;
                                
                                // Center the shape
                                offsetX = (canvasWidth - shapeWidth) / 2 - minX;
                                offsetY = (canvasHeight - shapeHeight) / 2 - minY;
                            }
                        }
                        else if (selectedTemplate.ShapeType == ShapeType.Oval)
                        {
                            var ovalData = _drawingService.DeserializeShapeData<OvalShapeData>(selectedTemplate.SerializedData);
                            if (ovalData != null)
                            {
                                // Calculate bounding box for oval
                                var minX = ovalData.CenterX - ovalData.RadiusX;
                                var minY = ovalData.CenterY - ovalData.RadiusY;
                                var maxX = ovalData.CenterX + ovalData.RadiusX;
                                var maxY = ovalData.CenterY + ovalData.RadiusY;
                                var shapeWidth = maxX - minX;
                                var shapeHeight = maxY - minY;
                                
                                // Center the shape
                                offsetX = (canvasWidth - shapeWidth) / 2 - minX;
                                offsetY = (canvasHeight - shapeHeight) / 2 - minY;
                            }
                        }
                        else
                        {
                            // For other shapes, use default center offset
                            offsetX = Math.Max(0, (canvasWidth - 200) / 2);
                            offsetY = Math.Max(0, (canvasHeight - 200) / 2);
                        }
                    }
                    catch
                    {
                        // Fallback to default offset
                        offsetX = Math.Max(0, (canvasWidth - 200) / 2);
                        offsetY = Math.Max(0, (canvasHeight - 200) / 2);
                    }

                    // Render template shape on canvas
                    DrawingCanvasControl.RenderTemplateShape(selectedTemplate, _drawingService, offsetX, offsetY);

                    // Only save to database if we have a canvas
                    if (_currentCanvas != null)
                    {
                        // Create shape entity to save to database
                        // The RenderTemplateShape method renders the shape visually,
                        // but we need to save it to database separately
                        var shapeEntity = _drawingService.CreateShape(
                            selectedTemplate.ShapeType,
                            _currentCanvas.Id,
                            selectedTemplate.StrokeColor,
                            selectedTemplate.StrokeThickness,
                            selectedTemplate.FillColor,
                            selectedTemplate.SerializedData
                        );

                        // Save to database
                        await _dataService.CreateShapeAsync(shapeEntity);

                        // Update canvas last modified date
                        _currentCanvas.LastModifiedDate = DateTime.UtcNow;
                        await _dataService.UpdateCanvasAsync(_currentCanvas);
                        
                        ShowSaveNotification($"Template '{selectedTemplate.TemplateName}' loaded and saved successfully");
                    }
                    else
                    {
                        ShowSaveNotification($"Template '{selectedTemplate.TemplateName}' loaded (not saved - no canvas)");
                    }

                }
                catch (Exception ex)
                {
                    ShowSaveNotification($"Failed to load template: {ex.Message}", isError: true);
                    args.Cancel = true;
                }
                finally
                {
                    deferral.Complete();
                }
            };

            var result = await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            ShowSaveNotification($"Failed to load templates: {ex.Message}", isError: true);
        }
    }

    /// <summary>
    /// Handles the Manage Templates button click event.
    /// </summary>
    private async void ManageTemplatesButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_dataService == null)
        {
            ShowSaveNotification("Data service not available", isError: true);
            return;
        }

        try
        {
            // Load all templates
            var templates = await _dataService.GetAllTemplatesAsync();

            // Show dialog with template list
            var dialog = new ContentDialog
            {
                Title = "Manage Templates",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot,
                MaxWidth = 600
            };

            var scrollViewer = new ScrollViewer
            {
                MaxHeight = 400
            };

            var stackPanel = new StackPanel { Spacing = 12 };

            if (templates.Count == 0)
            {
                var noTemplatesText = new TextBlock
                {
                    Text = "No templates available.",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 20, 0, 20)
                };
                stackPanel.Children.Add(noTemplatesText);
            }
            else
            {
                foreach (var template in templates)
                {
                    var templateBorder = new Border
                    {
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
                        BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                        BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                        CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4),
                        Padding = new Microsoft.UI.Xaml.Thickness(12),
                        Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 8)
                    };

                    var templatePanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
                    
                    var templateInfo = new StackPanel { VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center };
                    var templateNameText = new TextBlock
                    {
                        Text = template.TemplateName ?? "Unnamed Template",
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        FontSize = 14
                    };
                    var templateTypeText = new TextBlock
                    {
                        Text = $"Type: {template.ShapeType}",
                        FontSize = 12,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                        Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0)
                    };
                    templateInfo.Children.Add(templateNameText);
                    templateInfo.Children.Add(templateTypeText);
                    templatePanel.Children.Add(templateInfo);

                    var deleteButton = new Button
                    {
                        Content = "Delete",
                        Tag = template,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right,
                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                    };
                    deleteButton.Click += async (s, args) =>
                    {
                        if (s is Button btn && btn.Tag is Shape templateToDelete)
                        {
                            await DeleteTemplateAsync(templateToDelete);
                            // Refresh template list
                            ManageTemplatesButton_Click(sender, e);
                        }
                    };
                    templatePanel.Children.Add(deleteButton);

                    templateBorder.Child = templatePanel;
                    stackPanel.Children.Add(templateBorder);
                }
            }

            scrollViewer.Content = stackPanel;
            dialog.Content = scrollViewer;

            await dialog.ShowAsync();
        }
        catch (Exception)
        {
            ShowSaveNotification("Failed to load templates", isError: true);
        }
    }

    /// <summary>
    /// Deletes a template from the database.
    /// </summary>
    private async Task DeleteTemplateAsync(Shape template)
    {
        if (_dataService == null)
            return;

        // Show confirmation dialog
        var confirmDialog = new ContentDialog
        {
            Title = "Delete Template",
            Content = $"Are you sure you want to delete template '{template.TemplateName}'? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = XamlRoot
        };

        var result = await confirmDialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return; // User cancelled
        }

        try
        {
            bool deleted = await _dataService.DeleteShapeAsync(template.Id);
            if (deleted)
            {
                ShowSaveNotification($"Template '{template.TemplateName}' deleted successfully");
            }
            else
            {
                ShowSaveNotification("Failed to delete template", isError: true);
            }
        }
        catch (Exception)
        {
            ShowSaveNotification("Failed to delete template", isError: true);
        }
    }

    /// <summary>
    /// Toggles the visibility of the drawing tools sidebar.
    /// </summary>
    private void ToggleSidebarButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ToolsPanelColumn != null)
        {
            // Toggle sidebar visibility
            if (ToolsPanelColumn.Width.Value == 0)
            {
                // Show sidebar
                ToolsPanelColumn.Width = new Microsoft.UI.Xaml.GridLength(200);
            }
            else
            {
                // Hide sidebar
                ToolsPanelColumn.Width = new Microsoft.UI.Xaml.GridLength(0);
            }
        }
    }
}


