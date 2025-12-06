using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;
using whiteboard_app_data.Enums;
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

    /// <summary>
    /// Gets or sets the current shape type being drawn.
    /// </summary>
    public ShapeType? CurrentShapeType
    {
        get => GetValue(CurrentShapeTypeProperty) as ShapeType?;
        set => SetValue(CurrentShapeTypeProperty, value);
    }

    /// <summary>
    /// Identifies the CurrentShapeType dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentShapeTypeProperty =
        DependencyProperty.Register(
            nameof(CurrentShapeType),
            typeof(ShapeType?),
            typeof(DrawingCanvas),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the stroke color for drawing.
    /// </summary>
    public string StrokeColor
    {
        get => GetValue(StrokeColorProperty) as string ?? "#000000";
        set => SetValue(StrokeColorProperty, value);
    }

    /// <summary>
    /// Identifies the StrokeColor dependency property.
    /// </summary>
    public static readonly DependencyProperty StrokeColorProperty =
        DependencyProperty.Register(
            nameof(StrokeColor),
            typeof(string),
            typeof(DrawingCanvas),
            new PropertyMetadata("#000000"));

    /// <summary>
    /// Gets or sets the stroke thickness for drawing.
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Identifies the StrokeThickness dependency property.
    /// </summary>
    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(DrawingCanvas),
            new PropertyMetadata(2.0));

    /// <summary>
    /// Gets or sets the fill color for drawing.
    /// </summary>
    public string FillColor
    {
        get => GetValue(FillColorProperty) as string ?? "Transparent";
        set => SetValue(FillColorProperty, value);
    }

    /// <summary>
    /// Identifies the FillColor dependency property.
    /// </summary>
    public static readonly DependencyProperty FillColorProperty =
        DependencyProperty.Register(
            nameof(FillColor),
            typeof(string),
            typeof(DrawingCanvas),
            new PropertyMetadata("Transparent"));

    /// <summary>
    /// Event raised when a shape drawing is completed.
    /// </summary>
    public event EventHandler<ShapeDrawingCompletedEventArgs>? ShapeDrawingCompleted;

    private Shape? _previewShape;
    private Point _startPoint;
    private bool _isDrawing;

    public DrawingCanvas()
    {
        InitializeComponent();
        PointerPressed += DrawingCanvas_PointerPressed;
        PointerMoved += DrawingCanvas_PointerMoved;
        PointerReleased += DrawingCanvas_PointerReleased;
        PointerCanceled += DrawingCanvas_PointerCanceled;
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

    private void DrawingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (CurrentShapeType == null || _isDrawing)
            return;

        var point = e.GetCurrentPoint(this);
        _startPoint = point.Position;
        _isDrawing = true;
        CapturePointer(e.Pointer);
    }

    private void DrawingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || CurrentShapeType == null)
            return;

        var point = e.GetCurrentPoint(this);
        var currentPoint = point.Position;
        
        // For Circle, always constrain to perfect circle (calculate radius from distance)
        if (CurrentShapeType.Value == ShapeType.Circle)
        {
            var deltaX = currentPoint.X - _startPoint.X;
            var deltaY = currentPoint.Y - _startPoint.Y;
            var radius = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            // Keep the angle but use calculated radius
            if (radius > 0)
            {
                var angle = Math.Atan2(deltaY, deltaX);
                currentPoint = new Point(
                    _startPoint.X + radius * Math.Cos(angle),
                    _startPoint.Y + radius * Math.Sin(angle));
            }
        }
        
        UpdatePreview(_startPoint, currentPoint);
    }

    private void DrawingCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing)
            return;

        var point = e.GetCurrentPoint(this);
        var endPoint = point.Position;
        
        // For Circle, always constrain to perfect circle (calculate radius from distance)
        if (CurrentShapeType == ShapeType.Circle)
        {
            var deltaX = endPoint.X - _startPoint.X;
            var deltaY = endPoint.Y - _startPoint.Y;
            var radius = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            // Keep the angle but use calculated radius
            if (radius > 0)
            {
                var angle = Math.Atan2(deltaY, deltaX);
                endPoint = new Point(
                    _startPoint.X + radius * Math.Cos(angle),
                    _startPoint.Y + radius * Math.Sin(angle));
            }
        }
        
        FinishDrawing(_startPoint, endPoint);
        ReleasePointerCapture(e.Pointer);
    }

    private void DrawingCanvas_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        if (_isDrawing)
        {
            ClearPreview();
            _isDrawing = false;
            ReleasePointerCapture(e.Pointer);
        }
    }

    private void UpdatePreview(Point startPoint, Point currentPoint)
    {
        ClearPreview();

        if (CurrentShapeType == null)
            return;

        _previewShape = CreatePreviewShape(CurrentShapeType.Value, startPoint, currentPoint);
        if (_previewShape != null)
        {
            Children.Add(_previewShape);
        }
    }

    private void FinishDrawing(Point startPoint, Point endPoint)
    {
        if (CurrentShapeType == null)
            return;

        ClearPreview();
        _isDrawing = false;

        // Render final shape based on type
        if (CurrentShapeType.Value == ShapeType.Line)
        {
            RenderFinalLine(startPoint, endPoint);
        }
        else if (CurrentShapeType.Value == ShapeType.Rectangle)
        {
            RenderFinalRectangle(startPoint, endPoint);
        }
        else if (CurrentShapeType.Value == ShapeType.Oval)
        {
            RenderFinalOval(startPoint, endPoint);
        }
        else if (CurrentShapeType.Value == ShapeType.Circle)
        {
            RenderFinalCircle(startPoint, endPoint);
        }

        // Raise event with drawing data
        var args = new ShapeDrawingCompletedEventArgs
        {
            ShapeType = CurrentShapeType.Value,
            StartPoint = startPoint,
            EndPoint = endPoint,
            StrokeColor = StrokeColor,
            StrokeThickness = StrokeThickness,
            FillColor = FillColor
        };

        ShapeDrawingCompleted?.Invoke(this, args);
    }

    /// <summary>
    /// Renders a final Line shape on the canvas.
    /// </summary>
    private void RenderFinalLine(Point startPoint, Point endPoint)
    {
        var strokeBrush = new SolidColorBrush(ParseHexColor(StrokeColor));
        var line = new Line
        {
            X1 = startPoint.X,
            Y1 = startPoint.Y,
            X2 = endPoint.X,
            Y2 = endPoint.Y,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness
        };
        Children.Add(line);
    }

    /// <summary>
    /// Renders a final Rectangle shape on the canvas.
    /// </summary>
    private void RenderFinalRectangle(Point startPoint, Point endPoint)
    {
        var left = Math.Min(startPoint.X, endPoint.X);
        var top = Math.Min(startPoint.Y, endPoint.Y);
        var width = Math.Abs(endPoint.X - startPoint.X);
        var height = Math.Abs(endPoint.Y - startPoint.Y);

        var strokeBrush = new SolidColorBrush(ParseHexColor(StrokeColor));
        var fillBrush = FillColor == "Transparent"
            ? null
            : new SolidColorBrush(ParseHexColor(FillColor));

        var rect = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);
        Children.Add(rect);
    }

    /// <summary>
    /// Renders a final Oval shape on the canvas.
    /// </summary>
    private void RenderFinalOval(Point startPoint, Point endPoint)
    {
        var left = Math.Min(startPoint.X, endPoint.X);
        var top = Math.Min(startPoint.Y, endPoint.Y);
        var width = Math.Abs(endPoint.X - startPoint.X);
        var height = Math.Abs(endPoint.Y - startPoint.Y);

        var strokeBrush = new SolidColorBrush(ParseHexColor(StrokeColor));
        var fillBrush = FillColor == "Transparent"
            ? null
            : new SolidColorBrush(ParseHexColor(FillColor));

        var ellipse = new Ellipse
        {
            Width = width,
            Height = height,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        Children.Add(ellipse);
    }

    /// <summary>
    /// Renders a final Circle shape on the canvas.
    /// </summary>
    private void RenderFinalCircle(Point startPoint, Point endPoint)
    {
        var deltaX = endPoint.X - startPoint.X;
        var deltaY = endPoint.Y - startPoint.Y;
        var radius = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
        var diameter = radius * 2;

        var strokeBrush = new SolidColorBrush(ParseHexColor(StrokeColor));
        var fillBrush = FillColor == "Transparent"
            ? null
            : new SolidColorBrush(ParseHexColor(FillColor));

        var ellipse = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        Canvas.SetLeft(ellipse, startPoint.X - radius);
        Canvas.SetTop(ellipse, startPoint.Y - radius);
        Children.Add(ellipse);
    }

    private void ClearPreview()
    {
        if (_previewShape != null)
        {
            Children.Remove(_previewShape);
            _previewShape = null;
        }
    }

    private Shape? CreatePreviewShape(ShapeType shapeType, Point startPoint, Point endPoint)
    {
        var strokeBrush = new SolidColorBrush(ParseHexColor(StrokeColor));
        var fillBrush = FillColor == "Transparent" 
            ? null 
            : new SolidColorBrush(ParseHexColor(FillColor));

        return shapeType switch
        {
            ShapeType.Line => CreatePreviewLine(startPoint, endPoint, strokeBrush),
            ShapeType.Rectangle => CreatePreviewRectangle(startPoint, endPoint, strokeBrush, fillBrush),
            ShapeType.Oval => CreatePreviewOval(startPoint, endPoint, strokeBrush, fillBrush),
            ShapeType.Circle => CreatePreviewCircle(startPoint, endPoint, strokeBrush, fillBrush),
            _ => null
        };
    }

    private Shape CreatePreviewLine(Point start, Point end, Brush strokeBrush)
    {
        var line = new Line
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness
        };
        return line;
    }

    private Shape CreatePreviewRectangle(Point start, Point end, Brush strokeBrush, Brush? fillBrush)
    {
        var left = Math.Min(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);

        var rect = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);
        return rect;
    }

    private Shape CreatePreviewOval(Point start, Point end, Brush strokeBrush, Brush? fillBrush)
    {
        var left = Math.Min(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);

        var ellipse = new Ellipse
        {
            Width = width,
            Height = height,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        return ellipse;
    }

    private Shape CreatePreviewCircle(Point start, Point end, Brush strokeBrush, Brush? fillBrush)
    {
        var radius = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
        var diameter = radius * 2;

        var ellipse = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        Canvas.SetLeft(ellipse, start.X - radius);
        Canvas.SetTop(ellipse, start.Y - radius);
        return ellipse;
    }
}

/// <summary>
/// Event arguments for shape drawing completion.
/// </summary>
public class ShapeDrawingCompletedEventArgs : EventArgs
{
    public ShapeType ShapeType { get; set; }
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
    public string StrokeColor { get; set; } = "#000000";
    public double StrokeThickness { get; set; } = 2.0;
    public string FillColor { get; set; } = "Transparent";
}

