namespace whiteboard_app_data.Models.ShapeTypes;

/// <summary>
/// Data model for Triangle shape containing coordinates of its three points.
/// This data is serialized to JSON and stored in Shape.SerializedData.
/// </summary>
public class TriangleShapeData
{
    /// <summary>
    /// Gets or sets the X coordinate of the first point.
    /// </summary>
    public double Point1X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the first point.
    /// </summary>
    public double Point1Y { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate of the second point.
    /// </summary>
    public double Point2X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the second point.
    /// </summary>
    public double Point2Y { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate of the third point.
    /// </summary>
    public double Point3X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the third point.
    /// </summary>
    public double Point3Y { get; set; }
}

