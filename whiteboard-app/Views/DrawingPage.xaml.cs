using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using whiteboard_app.Controls;
using whiteboard_app.Services;
using whiteboard_app_data.Enums;
using whiteboard_app_data.Models;
using whiteboard_app_data.Models.ShapeTypes;
using CanvasModel = whiteboard_app_data.Models.Canvas;
using StrokeStyleEnum = whiteboard_app_data.Enums.StrokeStyle;

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
}

