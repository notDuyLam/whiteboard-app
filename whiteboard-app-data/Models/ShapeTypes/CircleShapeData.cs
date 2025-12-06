namespace whiteboard_app_data.Models.ShapeTypes;

/// <summary>
/// Data model for Circle shape containing center coordinates and radius.
/// This data is serialized to JSON and stored in Shape.SerializedData.
/// </summary>
public class CircleShapeData
{
    /// <summary>
    /// Gets or sets the X coordinate of the center point.
    /// </summary>
    public double CenterX { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the center point.
    /// </summary>
    public double CenterY { get; set; }

    /// <summary>
    /// Gets or sets the radius of the circle.
    /// </summary>
    public double Radius { get; set; }
}

