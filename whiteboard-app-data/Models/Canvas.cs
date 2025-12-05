using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace whiteboard_app_data.Models;

/// <summary>
/// Represents a drawing canvas containing shapes and associated with a profile.
/// </summary>
public class Canvas
{
    /// <summary>
    /// Gets or sets the unique identifier for the canvas.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the canvas.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the width of the canvas in pixels.
    /// </summary>
    [Range(100, 10000)]
    public int Width { get; set; } = 800;

    /// <summary>
    /// Gets or sets the height of the canvas in pixels.
    /// </summary>
    [Range(100, 10000)]
    public int Height { get; set; } = 600;

    /// <summary>
    /// Gets or sets the background color of the canvas in hex format (e.g., "#FFFFFF").
    /// </summary>
    [Required]
    [MaxLength(9)]
    public string BackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// Gets or sets the date and time when the canvas was created.
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the canvas was last modified.
    /// </summary>
    [Required]
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the foreign key to the Profile that owns this canvas.
    /// </summary>
    [Required]
    public Guid ProfileId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the Profile that owns this canvas.
    /// </summary>
    [ForeignKey(nameof(ProfileId))]
    public virtual Profile? Profile { get; set; }

    /// <summary>
    /// Gets or sets the collection of shapes drawn on this canvas.
    /// </summary>
    public virtual ICollection<Shape> Shapes { get; set; } = new List<Shape>();
}
