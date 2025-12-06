namespace whiteboard_app_data.Models.ShapeTypes;

/// <summary>
/// Data model for Line shape containing start and end coordinates.
/// This data is serialized to JSON and stored in Shape.SerializedData.
/// </summary>
public class LineShapeData
{
    /// <summary>
    /// Gets or sets the X coordinate of the start point.
    /// </summary>
    public double StartX { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the start point.
    /// </summary>
    public double StartY { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate of the end point.
    /// </summary>
    public double EndX { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the end point.
    /// </summary>
    public double EndY { get; set; }
}

