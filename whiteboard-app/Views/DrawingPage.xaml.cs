using System;
using Microsoft.UI.Xaml.Controls;
using whiteboard_app_data.Enums;
using StrokeStyleEnum = whiteboard_app_data.Enums.StrokeStyle;

namespace whiteboard_app.Views;

/// <summary>
/// Drawing page with canvas and drawing tools.
/// </summary>
public sealed partial class DrawingPage : Page
{
    public DrawingPage()
    {
        InitializeComponent();
        InitializeStrokeSettings();
        
        // Subscribe to shape selection events
        if (DrawingCanvasControl != null)
        {
            DrawingCanvasControl.ShapeSelected += DrawingCanvasControl_ShapeSelected;
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
        
        // Apply initial settings to canvas
        ApplyStrokeSettings();
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

    private void EditShapeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DrawingCanvasControl?.SelectedShape == null)
            return;

        // Get selected shape properties and show edit dialog
        // For now, just show a message - full editing will be implemented later
        var dialog = new ContentDialog
        {
            Title = "Edit Shape",
            Content = "Shape editing feature will be implemented in the next phase.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        _ = dialog.ShowAsync();
    }

    private void DeleteShapeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DrawingCanvasControl?.SelectedShape == null)
            return;

        // Remove the selected shape from canvas
        var selectedShape = DrawingCanvasControl.SelectedShape;
        DrawingCanvasControl.Children.Remove(selectedShape);
        
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
}

