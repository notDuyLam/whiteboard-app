using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;
using whiteboard_app_data.Enums;
using whiteboard_app_data.Models;
using CanvasModel = whiteboard_app_data.Models.Canvas;
using XamlCanvas = Microsoft.UI.Xaml.Controls.Canvas;
using ShapeModel = whiteboard_app_data.Models.Shape;
using XamlShape = Microsoft.UI.Xaml.Shapes.Shape;

namespace whiteboard_app.Controls;

/// <summary>
/// Custom Canvas control for drawing shapes on a whiteboard.
/// Extends the base Canvas control with drawing-specific functionality.
/// </summary>
public sealed partial class DrawingCanvas : XamlCanvas
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
            new PropertyMetadata(null, OnCurrentShapeTypeChanged));

    private static void OnCurrentShapeTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DrawingCanvas canvas)
        {
            // Reset drawing state when tool changes
            canvas.ResetDrawingState();
        }
    }

    /// <summary>
    /// Resets the drawing state when tool changes.
    /// </summary>
    private void ResetDrawingState()
    {
        if (_isDrawing)
        {
            ClearPreview();
            _isDrawing = false;
        }
        _trianglePoints?.Clear();
        _polygonPoints?.Clear();
        _startPoint = new Point(0, 0);
    }

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
    /// Gets or sets the stroke style for drawing.
    /// </summary>
    public StrokeStyle? StrokeStyle
    {
        get => (StrokeStyle?)GetValue(StrokeStyleProperty);
        set => SetValue(StrokeStyleProperty, value);
    }

    /// <summary>
    /// Identifies the StrokeStyle dependency property.
    /// </summary>
    public static readonly DependencyProperty StrokeStyleProperty =
        DependencyProperty.Register(
            nameof(StrokeStyle),
            typeof(StrokeStyle?),
            typeof(DrawingCanvas),
            new PropertyMetadata(whiteboard_app_data.Enums.StrokeStyle.Solid));

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

    /// <summary>
    /// Event raised when a shape is selected.
    /// </summary>
    public event EventHandler? ShapeSelected;

    private XamlShape? _previewShape;
    private Point _startPoint;
    private bool _isDrawing;
    private System.Collections.Generic.List<Point> _trianglePoints = new();
    private System.Collections.Generic.List<Point> _polygonPoints = new();
    
    // Shape selection and editing
    private Dictionary<XamlShape, ShapeModel> _shapeMap = new();
    private XamlShape? _selectedShape;
    private Border? _selectionBorder;
    private bool _isSelectionMode = false;

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

    /// <summary>
    /// Creates a stroke brush with the specified color and applies stroke style.
    /// </summary>
    private Brush CreateStrokeBrush(string color)
    {
        var brush = new SolidColorBrush(ParseHexColor(color));
        
        // Stroke style is applied via ApplyStrokeStyle method
        
        return brush;
    }

    /// <summary>
    /// Applies stroke style to a shape.
    /// </summary>
    private void ApplyStrokeStyle(XamlShape shape)
    {
        if (StrokeStyle.HasValue && StrokeStyle.Value != whiteboard_app_data.Enums.StrokeStyle.Solid)
        {
            var strokeCollection = new DoubleCollection();
            if (StrokeStyle.Value == whiteboard_app_data.Enums.StrokeStyle.Dash)
            {
                // Dash pattern: 4 units on, 2 units off
                strokeCollection.Add(4);
                strokeCollection.Add(2);
            }
            else if (StrokeStyle.Value == whiteboard_app_data.Enums.StrokeStyle.Dot)
            {
                // Dot pattern: 1 unit on, 2 units off
                strokeCollection.Add(1);
                strokeCollection.Add(2);
            }
            shape.StrokeDashArray = strokeCollection;
        }
        else
        {
            shape.StrokeDashArray = null;
        }
    }

    private void DrawingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        
        // Handle shape selection if in selection mode
        if (_isSelectionMode)
        {
            HandleShapeSelection(point.Position);
            return;
        }
        
        if (CurrentShapeType == null)
            return;
        
        // Handle Polygon: collect multiple points
        if (CurrentShapeType.Value == ShapeType.Polygon)
        {
            // Always start fresh when clicking (reset if needed)
            if (_polygonPoints.Count == 0)
            {
                _isDrawing = true;
            }
            
            var clickPoint = point.Position;
            
            // Check if clicking near the first point (within 10 pixels) to close polygon
            if (_polygonPoints.Count >= 3)
            {
                var firstPoint = _polygonPoints[0];
                var distance = Math.Sqrt(Math.Pow(clickPoint.X - firstPoint.X, 2) + Math.Pow(clickPoint.Y - firstPoint.Y, 2));
                if (distance <= 10)
                {
                    // Close the polygon
                    FinishPolygonDrawing();
                    _isDrawing = false;
                    _polygonPoints.Clear();
                    e.Handled = true;
                    return;
                }
            }
            
            _polygonPoints.Add(clickPoint);
            
            // Update preview
            if (_polygonPoints.Count >= 2)
            {
                UpdatePolygonPreview();
            }
            // For first point, no preview needed - user can see the click
            
            e.Handled = true;
            return;
        }
        
        // Handle Triangle: collect 3 points
        if (CurrentShapeType.Value == ShapeType.Triangle)
        {
            // Always start fresh when clicking (reset if needed)
            if (_trianglePoints.Count == 0)
            {
                _isDrawing = true;
            }
            
            _trianglePoints.Add(point.Position);
            
            // If we have 3 points, finish the triangle
            if (_trianglePoints.Count == 3)
            {
                FinishTriangleDrawing();
                _isDrawing = false;
                _trianglePoints.Clear();
            }
            else if (_trianglePoints.Count >= 2)
            {
                // Update preview with current points (show line between first 2 points)
                UpdateTrianglePreview();
            }
            // For first point, no preview needed - user can see the click
            e.Handled = true;
            return;
        }
        
        // For other shapes, start drawing normally
        if (_isDrawing)
            return;

        _startPoint = point.Position;
        _isDrawing = true;
        CapturePointer(e.Pointer);
    }

    private void DrawingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || CurrentShapeType == null)
            return;

        // For Triangle and Polygon, preview updates are handled in PointerPressed
        if (CurrentShapeType.Value == ShapeType.Triangle || CurrentShapeType.Value == ShapeType.Polygon)
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
            // For Triangle, reset points collection
            if (CurrentShapeType == ShapeType.Triangle)
            {
                _trianglePoints.Clear();
            }
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
        var strokeBrush = CreateStrokeBrush(StrokeColor);
        var line = new Line
        {
            X1 = startPoint.X,
            Y1 = startPoint.Y,
            X2 = endPoint.X,
            Y2 = endPoint.Y,
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness
        };
        ApplyStrokeStyle(line);
        Children.Add(line);
        
        // Track shape for selection
        var shapeEntity = new ShapeConcrete
        {
            Id = Guid.NewGuid(),
            ShapeType = ShapeType.Line,
            StrokeColor = StrokeColor,
            StrokeThickness = StrokeThickness,
            FillColor = FillColor
        };
        _shapeMap[line] = shapeEntity;
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

        var strokeBrush = CreateStrokeBrush(StrokeColor);
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
        ApplyStrokeStyle(rect);
        XamlCanvas.SetLeft(rect, left);
        XamlCanvas.SetTop(rect, top);
        Children.Add(rect);
        
        // Track shape for selection
        var shapeEntity = new ShapeConcrete
        {
            Id = Guid.NewGuid(),
            ShapeType = ShapeType.Rectangle,
            StrokeColor = StrokeColor,
            StrokeThickness = StrokeThickness,
            FillColor = FillColor
        };
        _shapeMap[rect] = shapeEntity;
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

        var strokeBrush = CreateStrokeBrush(StrokeColor);
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
        ApplyStrokeStyle(ellipse);
        XamlCanvas.SetLeft(ellipse, left);
        XamlCanvas.SetTop(ellipse, top);
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

        var strokeBrush = CreateStrokeBrush(StrokeColor);
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
        ApplyStrokeStyle(ellipse);
        XamlCanvas.SetLeft(ellipse, startPoint.X - radius);
        XamlCanvas.SetTop(ellipse, startPoint.Y - radius);
        Children.Add(ellipse);
        
        // Track shape for selection
        var shapeEntity = new ShapeConcrete
        {
            Id = Guid.NewGuid(),
            ShapeType = ShapeType.Circle,
            StrokeColor = StrokeColor,
            StrokeThickness = StrokeThickness,
            FillColor = FillColor
        };
        _shapeMap[ellipse] = shapeEntity;
    }

    /// <summary>
    /// Updates the triangle preview based on current points.
    /// </summary>
    private void UpdateTrianglePreview()
    {
        ClearPreview();
        
        if (_trianglePoints.Count < 2)
            return;

        var strokeBrush = CreateStrokeBrush(StrokeColor);

        // If we have 2 points, show a line preview
        if (_trianglePoints.Count == 2)
        {
            var line = new Line
            {
                X1 = _trianglePoints[0].X,
                Y1 = _trianglePoints[0].Y,
                X2 = _trianglePoints[1].X,
                Y2 = _trianglePoints[1].Y,
                Stroke = strokeBrush,
                StrokeThickness = StrokeThickness
            };
            ApplyStrokeStyle(line);
            _previewShape = line;
            Children.Add(_previewShape);
        }
        // If we have 3 points, show full triangle preview (though this shouldn't happen as we finish immediately)
        else if (_trianglePoints.Count == 3)
        {
            var fillBrush = FillColor == "Transparent"
                ? null
                : new SolidColorBrush(ParseHexColor(FillColor));

            var polygon = new Polygon
            {
                Stroke = strokeBrush,
                StrokeThickness = StrokeThickness,
                Fill = fillBrush
            };
            ApplyStrokeStyle(polygon);

            var points = new PointCollection();
            foreach (var pt in _trianglePoints)
            {
                points.Add(pt);
            }
            polygon.Points = points;

            _previewShape = polygon;
            Children.Add(_previewShape);
        }
    }

    /// <summary>
    /// Updates the polygon preview based on current points.
    /// </summary>
    private void UpdatePolygonPreview()
    {
        ClearPreview();
        
        if (_polygonPoints.Count < 2)
            return;

        var strokeBrush = CreateStrokeBrush(StrokeColor);
        var fillBrush = FillColor == "Transparent"
            ? null
            : new SolidColorBrush(ParseHexColor(FillColor));

        var polygon = new Polygon
        {
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        ApplyStrokeStyle(polygon);

        var points = new PointCollection();
        foreach (var pt in _polygonPoints)
        {
            points.Add(pt);
        }
        polygon.Points = points;

        _previewShape = polygon;
        Children.Add(_previewShape);
    }

    /// <summary>
    /// Finishes polygon drawing when user closes the polygon.
    /// </summary>
    private void FinishPolygonDrawing()
    {
        if (_polygonPoints.Count < 3)
            return;

        ClearPreview();

        var strokeBrush = CreateStrokeBrush(StrokeColor);
        var fillBrush = FillColor == "Transparent"
            ? null
            : new SolidColorBrush(ParseHexColor(FillColor));

        var polygon = new Polygon
        {
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        ApplyStrokeStyle(polygon);

        var points = new PointCollection();
        foreach (var pt in _polygonPoints)
        {
            points.Add(pt);
        }
        polygon.Points = points;

        Children.Add(polygon);

        // Raise event with drawing data
        var args = new ShapeDrawingCompletedEventArgs
        {
            ShapeType = ShapeType.Polygon,
            StartPoint = _polygonPoints[0],
            EndPoint = _polygonPoints[_polygonPoints.Count - 1],
            StrokeColor = StrokeColor,
            StrokeThickness = StrokeThickness,
            FillColor = FillColor
        };

        ShapeDrawingCompleted?.Invoke(this, args);
    }

    /// <summary>
    /// Finishes triangle drawing when 3 points are collected.
    /// </summary>
    private void FinishTriangleDrawing()
    {
        if (_trianglePoints.Count != 3)
            return;

        ClearPreview();

        var strokeBrush = CreateStrokeBrush(StrokeColor);
        var fillBrush = FillColor == "Transparent"
            ? null
            : new SolidColorBrush(ParseHexColor(FillColor));

        var polygon = new Polygon
        {
            Stroke = strokeBrush,
            StrokeThickness = StrokeThickness,
            Fill = fillBrush
        };
        ApplyStrokeStyle(polygon);

        var points = new PointCollection();
        foreach (var pt in _trianglePoints)
        {
            points.Add(pt);
        }
        polygon.Points = points;

        Children.Add(polygon);
        
        // Track shape for selection
        var shapeEntity = new ShapeConcrete
        {
            Id = Guid.NewGuid(),
            ShapeType = ShapeType.Triangle,
            StrokeColor = StrokeColor,
            StrokeThickness = StrokeThickness,
            FillColor = FillColor
        };
        _shapeMap[polygon] = shapeEntity;

        // Raise event with drawing data
        var args = new ShapeDrawingCompletedEventArgs
        {
            ShapeType = ShapeType.Triangle,
            StartPoint = _trianglePoints[0],
            EndPoint = _trianglePoints[2],
            StrokeColor = StrokeColor,
            StrokeThickness = StrokeThickness,
            FillColor = FillColor
        };

        ShapeDrawingCompleted?.Invoke(this, args);
    }

    private void ClearPreview()
    {
        if (_previewShape != null)
        {
            Children.Remove(_previewShape);
            _previewShape = null;
        }
    }

    /// <summary>
    /// Gets or sets whether the canvas is in selection mode.
    /// </summary>
    public bool IsSelectionMode
    {
        get => _isSelectionMode;
        set
        {
            _isSelectionMode = value;
            if (!value)
            {
                ClearSelection();
            }
        }
    }

    /// <summary>
    /// Gets the currently selected shape UI element.
    /// </summary>
    public XamlShape? SelectedShape => _selectedShape;

    /// <summary>
    /// Handles shape selection when clicking on the canvas.
    /// </summary>
    private void HandleShapeSelection(Point clickPoint)
    {
        // Find the topmost shape at the click point
        XamlShape? hitShape = null;
        
        // Check shapes in reverse order (top to bottom)
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i] is XamlShape shape && _shapeMap.ContainsKey(shape))
            {
                if (IsPointInShape(clickPoint, shape))
                {
                    hitShape = shape;
                    break;
                }
            }
        }
        
        if (hitShape != null)
        {
            SelectShape(hitShape);
        }
        else
        {
            ClearSelection();
        }
    }

    /// <summary>
    /// Checks if a point is within a shape's bounds.
    /// </summary>
    private bool IsPointInShape(Point point, XamlShape shape)
    {
        var bounds = GetShapeBounds(shape);
        return bounds.Contains(point);
    }

    /// <summary>
    /// Gets the bounding rectangle of a shape.
    /// </summary>
    private Rect GetShapeBounds(XamlShape shape)
    {
        if (shape is Line line)
        {
            var left = Math.Min(line.X1, line.X2);
            var top = Math.Min(line.Y1, line.Y2);
            var right = Math.Max(line.X1, line.X2);
            var bottom = Math.Max(line.Y1, line.Y2);
            return new Rect(left, top, right - left, bottom - top);
        }
        else if (shape is Rectangle rect)
        {
            var left = XamlCanvas.GetLeft(rect);
            var top = XamlCanvas.GetTop(rect);
            return new Rect(left, top, rect.Width, rect.Height);
        }
        else if (shape is Ellipse ellipse)
        {
            var left = Microsoft.UI.Xaml.Controls.Canvas.GetLeft(ellipse);
            var top = Microsoft.UI.Xaml.Controls.Canvas.GetTop(ellipse);
            return new Rect(left, top, ellipse.Width, ellipse.Height);
        }
        else if (shape is Polygon polygon)
        {
            if (polygon.Points.Count == 0)
                return new Rect();
            
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            
            foreach (var pt in polygon.Points)
            {
                minX = Math.Min(minX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxX = Math.Max(maxX, pt.X);
                maxY = Math.Max(maxY, pt.Y);
            }
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        
        return new Rect();
    }

    /// <summary>
    /// Selects a shape and shows selection border.
    /// </summary>
    private void SelectShape(XamlShape shape)
    {
        ClearSelection();
        
        _selectedShape = shape;
        var bounds = GetShapeBounds(shape);
        
        // Create selection border
        _selectionBorder = new Border
        {
            BorderBrush = new SolidColorBrush(Colors.Blue),
            BorderThickness = new Thickness(2),
            Background = new SolidColorBrush(Colors.Transparent),
            IsHitTestVisible = false
        };
        
        Microsoft.UI.Xaml.Controls.Canvas.SetLeft(_selectionBorder, bounds.X - 2);
        Microsoft.UI.Xaml.Controls.Canvas.SetTop(_selectionBorder, bounds.Y - 2);
        _selectionBorder.Width = bounds.Width + 4;
        _selectionBorder.Height = bounds.Height + 4;
        
        Children.Add(_selectionBorder);
        
        // Raise selection event
        ShapeSelected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    private void ClearSelection()
    {
        if (_selectionBorder != null)
        {
            Children.Remove(_selectionBorder);
            _selectionBorder = null;
        }
        _selectedShape = null;
        
        // Raise selection event (shape deselected)
        ShapeSelected?.Invoke(this, EventArgs.Empty);
    }

    private XamlShape? CreatePreviewShape(ShapeType shapeType, Point startPoint, Point endPoint)
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

    private XamlShape CreatePreviewLine(Point start, Point end, Brush strokeBrush)
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

    private XamlShape CreatePreviewRectangle(Point start, Point end, Brush strokeBrush, Brush? fillBrush)
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
        XamlCanvas.SetLeft(rect, left);
        XamlCanvas.SetTop(rect, top);
        return rect;
    }

    private XamlShape CreatePreviewOval(Point start, Point end, Brush strokeBrush, Brush? fillBrush)
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
        XamlCanvas.SetLeft(ellipse, left);
        XamlCanvas.SetTop(ellipse, top);
        return ellipse;
    }

    private XamlShape CreatePreviewCircle(Point start, Point end, Brush strokeBrush, Brush? fillBrush)
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
        Microsoft.UI.Xaml.Controls.Canvas.SetLeft(ellipse, start.X - radius);
        Microsoft.UI.Xaml.Controls.Canvas.SetTop(ellipse, start.Y - radius);
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

