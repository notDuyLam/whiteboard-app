using Microsoft.UI.Xaml.Controls;
using whiteboard_app_data.Enums;

namespace whiteboard_app.Views;

/// <summary>
/// Drawing page with canvas and drawing tools.
/// </summary>
public sealed partial class DrawingPage : Page
{
    public DrawingPage()
    {
        InitializeComponent();
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
        System.Diagnostics.Debug.WriteLine($"TriangleToolButton_Click: Set CurrentShapeType to {DrawingCanvasControl.CurrentShapeType}");
        
        // Update button states (visual feedback)
        UpdateToolButtonStates(TriangleToolButton);
    }

    private void UpdateToolButtonStates(Button? activeButton)
    {
        // Reset all tool buttons
        LineToolButton.Style = null;
        RectangleToolButton.Style = null;
        OvalToolButton.Style = null;
        CircleToolButton.Style = null;
        TriangleToolButton.Style = null;
        
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
}

