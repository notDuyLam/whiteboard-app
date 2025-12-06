using System.Collections.Generic;

namespace whiteboard_app_data.Models.ShapeTypes;

/// <summary>
/// Represents a single point with X and Y coordinates.
/// </summary>
public class PointData
{
    /// <summary>
    /// Gets or sets the X coordinate of the point.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the point.
    /// </summary>
    public double Y { get; set; }
}

/// <summary>
/// Data model for Polygon shape containing a collection of points.
/// This data is serialized to JSON and stored in Shape.SerializedData.
/// </summary>
public class PolygonShapeData
{
    /// <summary>
    /// Gets or sets the list of points that define the polygon.
    /// </summary>
    public List<PointData> Points { get; set; } = new List<PointData>();
}

