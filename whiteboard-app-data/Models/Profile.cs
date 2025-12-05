using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace whiteboard_app_data.Models;

/// <summary>
/// Represents a user profile containing configuration settings for the whiteboard application.
/// </summary>
public class Profile
{
    /// <summary>
    /// Gets or sets the unique identifier for the profile.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the profile.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the theme preference. Values: "Light", "Dark", "System".
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Theme { get; set; } = "System";

    /// <summary>
    /// Gets or sets the default canvas width in pixels.
    /// </summary>
    [Range(100, 10000)]
    public int DefaultCanvasWidth { get; set; } = 800;

    /// <summary>
    /// Gets or sets the default canvas height in pixels.
    /// </summary>
    [Range(100, 10000)]
    public int DefaultCanvasHeight { get; set; } = 600;

    /// <summary>
    /// Gets or sets the default stroke color in hex format (e.g., "#FF0000").
    /// </summary>
    [Required]
    [MaxLength(9)]
    public string DefaultStrokeColor { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the default stroke thickness in pixels.
    /// </summary>
    [Range(0.5, 50.0)]
    public double DefaultStrokeThickness { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the default fill color in hex format (e.g., "#FF0000" or "Transparent").
    /// </summary>
    [MaxLength(9)]
    public string DefaultFillColor { get; set; } = "Transparent";

    /// <summary>
    /// Gets or sets the date and time when the profile was created.
    /// </summary>
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the profile was last modified.
    /// </summary>
    [Required]
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether this profile is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of canvases associated with this profile.
    /// </summary>
    public virtual ICollection<Canvas> Canvases { get; set; } = new List<Canvas>();
}

