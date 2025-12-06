using Microsoft.EntityFrameworkCore;
using whiteboard_app_data.Models;

namespace whiteboard_app_data.Data;

/// <summary>
/// Entity Framework Core DbContext for the Whiteboard application.
/// Manages database operations for Profiles, Canvases, and Shapes.
/// </summary>
public class WhiteboardDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the Profiles DbSet.
    /// </summary>
    public DbSet<Profile> Profiles { get; set; }

    /// <summary>
    /// Gets or sets the Canvases DbSet.
    /// </summary>
    public DbSet<Canvas> Canvases { get; set; }

    /// <summary>
    /// Gets or sets the Shapes DbSet.
    /// </summary>
    public DbSet<Shape> Shapes { get; set; }

    public WhiteboardDbContext(DbContextOptions<WhiteboardDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WhiteboardApp",
                "whiteboard.db");

            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}

