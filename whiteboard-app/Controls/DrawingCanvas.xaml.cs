using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using CanvasModel = whiteboard_app_data.Models.Canvas;

namespace whiteboard_app.Controls;

/// <summary>
/// Custom Canvas control for drawing shapes on a whiteboard.
/// Extends the base Canvas control with drawing-specific functionality.
/// </summary>
public sealed partial class DrawingCanvas : Canvas
{
    /// <summary>
    /// Gets or sets the background color of the drawing canvas.
    /// </summary>
    public string BackgroundColor
    {
        get => GetValue(BackgroundColorProperty) as string ?? "#FFFFFF";
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>
    /// Identifies the BackgroundColor dependency property.
    /// </summary>
    public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register(
            nameof(BackgroundColor),
            typeof(string),
            typeof(DrawingCanvas),
            new PropertyMetadata("#FFFFFF", OnBackgroundColorChanged));

    /// <summary>
    /// Gets or sets the canvas model associated with this drawing canvas.
    /// </summary>
    public CanvasModel? CanvasModel
    {
        get => GetValue(CanvasModelProperty) as CanvasModel;
        set => SetValue(CanvasModelProperty, value);
    }

    /// <summary>
    /// Identifies the CanvasModel dependency property.
    /// </summary>
    public static readonly DependencyProperty CanvasModelProperty =
        DependencyProperty.Register(
            nameof(CanvasModel),
            typeof(CanvasModel),
            typeof(DrawingCanvas),
            new PropertyMetadata(null, OnCanvasModelChanged));

    public DrawingCanvas()
    {
        InitializeComponent();
    }

    private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DrawingCanvas canvas && e.NewValue is string colorString)
        {
            canvas.UpdateBackgroundColor(colorString);
        }
    }

    private static void OnCanvasModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DrawingCanvas canvas && e.NewValue is CanvasModel canvasModel)
        {
            canvas.UpdateFromCanvasModel(canvasModel);
        }
    }

    private void UpdateBackgroundColor(string colorString)
    {
        if (BackgroundBrush == null)
            return;

        if (colorString == "Transparent")
        {
            BackgroundBrush.Color = Colors.Transparent;
        }
        else
        {
            try
            {
                var color = ParseHexColor(colorString);
                BackgroundBrush.Color = color;
            }
            catch
            {
                // Default to white if parsing fails
                BackgroundBrush.Color = Colors.White;
            }
        }
    }

    private void UpdateFromCanvasModel(CanvasModel canvasModel)
    {
        if (canvasModel == null)
            return;

        // Update canvas size
        Width = canvasModel.Width;
        Height = canvasModel.Height;

        // Update background color
        BackgroundColor = canvasModel.BackgroundColor;
    }

    private static Color ParseHexColor(string hex)
    {
        hex = hex.Trim().TrimStart('#');

        if (hex.Length == 6)
        {
            // RRGGBB format
            var r = Convert.ToByte(hex.Substring(0, 2), 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromArgb(255, r, g, b);
        }
        else if (hex.Length == 8)
        {
            // AARRGGBB format
            var a = Convert.ToByte(hex.Substring(0, 2), 16);
            var r = Convert.ToByte(hex.Substring(2, 2), 16);
            var g = Convert.ToByte(hex.Substring(4, 2), 16);
            var b = Convert.ToByte(hex.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        throw new ArgumentException("Invalid hex color format");
    }
}

