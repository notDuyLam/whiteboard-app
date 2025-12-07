using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using whiteboard_app_data.Enums;

namespace whiteboard_app_data.Models;

/// <summary>
/// Base abstract class representing a shape drawn on a canvas.
/// Specific shape types (Line, Rectangle, etc.) will be represented by this entity
/// with shape-specific data stored in SerializedData as JSON.
/// </summary>
public abstract class Shape
{
    /// <summary>
    /// Gets or sets the unique identifier for the shape.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the type of the shape.
    /// </summary>
    [Required]
    public ShapeType ShapeType { get; set; }

    /// <summary>
    /// Gets or sets the stroke color in hex format (e.g., "#FF0000").
    /// </summary>
    [Required]
    [MaxLength(9)]
    public string StrokeColor { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the stroke thickness in pixels.
    /// </summary>
    [Range(0.5, 50.0)]
    public double StrokeThickness { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the fill color in hex format (e.g., "#FF0000" or "Transparent").
    /// </summary>
    [MaxLength(20)]
    public string FillColor { get; set; } = "Transparent";

    /// <summary>
    /// Gets or sets the stroke style (Solid, Dash, Dot).
    /// </summary>
    public StrokeStyle StrokeStyle { get; set; } = StrokeStyle.Solid;

    /// <summary>
    /// Gets or sets a value indicating whether this shape is saved as a template.
    /// </summary>
    public bool IsTemplate { get; set; } = false;

    /// <summary>
    /// Gets or sets the template name if this shape is a template.
    /// </summary>
    [MaxLength(200)]
    public string? TemplateName { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the shape was created.
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the foreign key to the Canvas that contains this shape.
    /// Nullable for template shapes that are not yet placed on a canvas.
    /// </summary>
    public Guid? CanvasId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the Canvas that contains this shape.
    /// </summary>
    [ForeignKey(nameof(CanvasId))]
    public virtual Canvas? Canvas { get; set; }

    /// <summary>
    /// Gets or sets the serialized JSON data containing shape-specific information
    /// (e.g., coordinates, dimensions, points) that varies by shape type.
    /// </summary>
    [Required]
    [Column(TypeName = "TEXT")]
    public string SerializedData { get; set; } = string.Empty;
}
