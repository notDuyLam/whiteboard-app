namespace whiteboard_app_data.Models.ShapeTypes;

/// <summary>
/// Data model for Oval shape containing center coordinates and radii.
/// This data is serialized to JSON and stored in Shape.SerializedData.
/// </summary>
public class OvalShapeData
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
    /// Gets or sets the radius along the X-axis.
    /// </summary>
    public double RadiusX { get; set; }

    /// <summary>
    /// Gets or sets the radius along the Y-axis.
    /// </summary>
    public double RadiusY { get; set; }
}

