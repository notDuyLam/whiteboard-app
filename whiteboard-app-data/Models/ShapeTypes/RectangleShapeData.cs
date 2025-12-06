namespace whiteboard_app_data.Models.ShapeTypes;

/// <summary>
/// Data model for Rectangle shape containing position and dimensions.
/// This data is serialized to JSON and stored in Shape.SerializedData.
/// </summary>
public class RectangleShapeData
{
    /// <summary>
    /// Gets or sets the X coordinate of the top-left corner.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the top-left corner.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the rectangle.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the rectangle.
    /// </summary>
    public double Height { get; set; }
}

