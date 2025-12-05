namespace whiteboard_app_data.Enums;

/// <summary>
/// Enum representing the types of shapes that can be drawn on the canvas.
/// </summary>
public enum ShapeType
{
    /// <summary>
    /// Line shape - drawn from start point to end point.
    /// </summary>
    Line = 0,

    /// <summary>
    /// Rectangle shape - drawn with width and height.
    /// </summary>
    Rectangle = 1,

    /// <summary>
    /// Oval shape - elliptical shape with radius X and radius Y.
    /// </summary>
    Oval = 2,

    /// <summary>
    /// Circle shape - perfect circle with single radius.
    /// </summary>
    Circle = 3,

    /// <summary>
    /// Triangle shape - drawn with three points.
    /// </summary>
    Triangle = 4,

    /// <summary>
    /// Polygon shape - drawn with multiple points.
    /// </summary>
    Polygon = 5
}

