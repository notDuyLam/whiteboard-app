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
                SetCanvas(canvas);
                await LoadCanvasShapesAsync(canvas);
            }
            else if (e.Parameter is Guid canvasId && _dataService != null)
            {
                var loadedCanvas = await _dataService.GetCanvasByIdAsync(canvasId);
                if (loadedCanvas != null)
                {
                    SetCanvas(loadedCanvas);
                    await LoadCanvasShapesAsync(loadedCanvas);
                }
            }
        }
    }

    /// <summary>
    /// Loads shapes from the database for the given canvas and renders them.
    /// </summary>
    private async Task LoadCanvasShapesAsync(CanvasModel canvas)
    {
        if (_dataService == null || _drawingService == null || DrawingCanvasControl == null)
            return;

        try
        {
            // Load shapes from database
            var shapes = await _dataService.GetShapesByCanvasIdAsync(canvas.Id);
            
            // Clear existing shapes first
            DrawingCanvasControl.ClearAllShapes();
            
            // TODO: Render each shape from database
            // This will be implemented when we add the RenderShapeFromModel method to DrawingCanvas
            // For now, shapes will be loaded when the canvas is set, but not rendered
            // The full implementation will be in Phase 23 (Load Canvas functionality)
        }
        catch (Exception)
        {
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

    private async void DrawingCanvasControl_ShapeDrawingCompleted(object? sender, ShapeDrawingCompletedEventArgs e)
    {
        // Auto-save shape when drawing is completed
        if (_currentCanvas != null && _dataService != null && _drawingService != null)
        {
            await SaveShapeAsync(e);
        }
    }

    /// <summary>
    /// Sets the current canvas for this drawing page.
    /// </summary>
    public void SetCanvas(CanvasModel canvas)
    {
        _currentCanvas = canvas;
        if (DrawingCanvasControl != null)
        {
            DrawingCanvasControl.CanvasModel = canvas;
            if (!string.IsNullOrEmpty(canvas.BackgroundColor))
            {
                DrawingCanvasControl.BackgroundColor = canvas.BackgroundColor;
            }
        }
    }

    /// <summary>
    /// Saves a shape to the database.
    /// </summary>
    private async Task SaveShapeAsync(ShapeDrawingCompletedEventArgs args)
    {
        if (_currentCanvas == null || _dataService == null || _drawingService == null)
            return;

        try
        {
            string serializedData = args.ShapeType switch
            {
                ShapeType.Line => _drawingService.SerializeShapeData(new LineShapeData
                {
                    StartX = args.StartPoint.X,
                    StartY = args.StartPoint.Y,
                    EndX = args.EndPoint.X,
                    EndY = args.EndPoint.Y
                }),
                ShapeType.Rectangle => _drawingService.SerializeShapeData(new RectangleShapeData
                {
                    X = Math.Min(args.StartPoint.X, args.EndPoint.X),
                    Y = Math.Min(args.StartPoint.Y, args.EndPoint.Y),
                    Width = Math.Abs(args.EndPoint.X - args.StartPoint.X),
                    Height = Math.Abs(args.EndPoint.Y - args.StartPoint.Y)
                }),
                ShapeType.Oval => _drawingService.SerializeShapeData(new OvalShapeData
                {
                    CenterX = (args.StartPoint.X + args.EndPoint.X) / 2,
                    CenterY = (args.StartPoint.Y + args.EndPoint.Y) / 2,
                    RadiusX = Math.Abs(args.EndPoint.X - args.StartPoint.X) / 2,
                    RadiusY = Math.Abs(args.EndPoint.Y - args.StartPoint.Y) / 2
                }),
                ShapeType.Circle => _drawingService.SerializeShapeData(new CircleShapeData
                {
                    CenterX = args.StartPoint.X,
                    CenterY = args.StartPoint.Y,
                    Radius = Math.Sqrt(Math.Pow(args.EndPoint.X - args.StartPoint.X, 2) + Math.Pow(args.EndPoint.Y - args.StartPoint.Y, 2))
                }),
                ShapeType.Triangle => args.TrianglePoints != null && args.TrianglePoints.Count == 3
                    ? _drawingService.SerializeShapeData(new TriangleShapeData
                    {
                        Point1X = args.TrianglePoints[0].X,
                        Point1Y = args.TrianglePoints[0].Y,
                        Point2X = args.TrianglePoints[1].X,
                        Point2Y = args.TrianglePoints[1].Y,
                        Point3X = args.TrianglePoints[2].X,
                        Point3Y = args.TrianglePoints[2].Y
                    })
                    : string.Empty,
                ShapeType.Polygon => args.PolygonPoints != null && args.PolygonPoints.Count >= 3
                    ? _drawingService.SerializeShapeData(new PolygonShapeData
                    {
                        Points = args.PolygonPoints.Select(p => new PointData { X = p.X, Y = p.Y }).ToList()
                    })
                    : string.Empty,
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(serializedData))
                return;

            var shape = _drawingService.CreateShape(
                args.ShapeType,
                _currentCanvas.Id,
                args.StrokeColor,
                args.StrokeThickness,
                args.FillColor,
                serializedData
            );

            await _dataService.CreateShapeAsync(shape);
            
            // Update canvas last modified date
            if (_currentCanvas != null)
            {
                _currentCanvas.LastModifiedDate = DateTime.UtcNow;
                await _dataService.UpdateCanvasAsync(_currentCanvas);
            }
            
            // Show save notification
            ShowSaveNotification("Shape saved successfully");
        }
        catch (Exception)
        {
            // Error handling - shape save failed
            ShowSaveNotification("Failed to save shape", isError: true);
        }
    }

    /// <summary>
    /// Shows a save notification to the user.
    /// </summary>
    private async void ShowSaveNotification(string message, bool isError = false)
    {
        if (SaveNotificationInfoBar == null)
            return;

        SaveNotificationInfoBar.Message = message;
        SaveNotificationInfoBar.Severity = isError 
            ? InfoBarSeverity.Error 
            : InfoBarSeverity.Success;
        SaveNotificationInfoBar.IsOpen = true;

        // Auto-hide after 3 seconds
        await Task.Delay(3000);
        if (SaveNotificationInfoBar != null)
        {
            SaveNotificationInfoBar.IsOpen = false;
        }
    }

    /// <summary>
    /// Handles the Save Canvas button click.
    /// </summary>
    private async void SaveCanvasButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_currentCanvas == null || _dataService == null)
        {
            ShowSaveNotification("No canvas selected", isError: true);
            return;
        }

        try
        {
            // Update canvas last modified date
            _currentCanvas.LastModifiedDate = DateTime.UtcNow;
            await _dataService.UpdateCanvasAsync(_currentCanvas);
            
            ShowSaveNotification("Canvas saved successfully");
        }
        catch (Exception)
        {
            ShowSaveNotification("Failed to save canvas", isError: true);
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

                // Get or create serialized data from selected shape
                string serializedData = selectedShapeModel.SerializedData;
                
                // If SerializedData is empty, create it from the selected XamlShape
                if (string.IsNullOrWhiteSpace(serializedData))
                {
                    var selectedXamlShape = DrawingCanvasControl.SelectedShape;
                    if (selectedXamlShape == null)
                    {
                        errorTextBlock.Text = "No shape selected.";
                        errorTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        args.Cancel = true;
                        return;
                    }
                    
                    // Serialize shape data based on type
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

                // Log before saving - add to error message for debugging
                var debugInfo = $"Type={templateShape.ShapeType}, IsTemplate={templateShape.IsTemplate}, IsShapeConcrete={templateShape is whiteboard_app_data.Models.ShapeConcrete}";
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] About to save template: {debugInfo}");
                
                // Save template to database
                await _dataService.CreateShapeAsync(templateShape);
                
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] Template saved successfully");
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
                
                // Also log to debug output
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] ERROR: {errorMessage}");
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] StackTrace: {ex.StackTrace}");
                
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
        System.Diagnostics.Debug.WriteLine("[DrawingPage] LoadTemplateButton_Click called");
        
        if (_dataService == null || _drawingService == null || DrawingCanvasControl == null || _currentCanvas == null)
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] Missing dependencies, cannot load template");
            ShowSaveNotification("Cannot load template: Canvas not available", isError: true);
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine("[DrawingPage] Loading templates...");
            // Load all templates
            var templates = await _dataService.GetAllTemplatesAsync();
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] Loaded {templates.Count} templates");

            if (templates.Count == 0)
            {
                ShowSaveNotification("No templates available", isError: true);
                return;
            }

            // Show dialog with template list
            var xamlRoot = this.XamlRoot;
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] XamlRoot is null: {xamlRoot == null}");
            
            if (xamlRoot == null)
            {
                System.Diagnostics.Debug.WriteLine("[DrawingPage] XamlRoot is null, cannot show dialog");
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

            System.Diagnostics.Debug.WriteLine("[DrawingPage] Dialog created, about to show...");
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
                    var canvasWidth = DrawingCanvasControl.ActualWidth > 0 ? DrawingCanvasControl.ActualWidth : _currentCanvas.Width;
                    var canvasHeight = DrawingCanvasControl.ActualHeight > 0 ? DrawingCanvasControl.ActualHeight : _currentCanvas.Height;
                    var offsetX = Math.Max(0, (canvasWidth - 200) / 2);
                    var offsetY = Math.Max(0, (canvasHeight - 200) / 2);

                    // Render template shape on canvas
                    DrawingCanvasControl.RenderTemplateShape(selectedTemplate, _drawingService, offsetX, offsetY);

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

                    ShowSaveNotification($"Template '{selectedTemplate.TemplateName}' loaded successfully");
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

            System.Diagnostics.Debug.WriteLine("[DrawingPage] Calling dialog.ShowAsync()...");
            var result = await dialog.ShowAsync();
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] Dialog closed with result: {result}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] Exception in LoadTemplateButton_Click: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DrawingPage] InnerException: {ex.InnerException.Message}");
            }
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
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] Loading templates...");
            var templates = await _dataService.GetAllTemplatesAsync();
            System.Diagnostics.Debug.WriteLine($"[DrawingPage] Loaded {templates.Count} templates");

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
}

