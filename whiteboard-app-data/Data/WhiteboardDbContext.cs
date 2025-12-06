using Microsoft.EntityFrameworkCore;
using whiteboard_app_data.Enums;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Profile entity
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Canvas entity
        modelBuilder.Entity<Canvas>(entity =>
        {
            entity.HasOne(e => e.Profile)
                  .WithMany(p => p.Canvases)
                  .HasForeignKey(e => e.ProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ProfileId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CreatedDate);
        });

        // Configure Shape entity (Table-Per-Hierarchy for different shape types)
        modelBuilder.Entity<Shape>(entity =>
        {
            entity.HasDiscriminator<ShapeType>("ShapeType")
                  .HasValue<ShapeConcrete>(ShapeType.Line)
                  .HasValue<ShapeConcrete>(ShapeType.Rectangle)
                  .HasValue<ShapeConcrete>(ShapeType.Oval)
                  .HasValue<ShapeConcrete>(ShapeType.Circle)
                  .HasValue<ShapeConcrete>(ShapeType.Triangle)
                  .HasValue<ShapeConcrete>(ShapeType.Polygon);

            entity.HasOne(e => e.Canvas)
                  .WithMany(c => c.Shapes)
                  .HasForeignKey(e => e.CanvasId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CanvasId);
            entity.HasIndex(e => e.ShapeType);
            entity.HasIndex(e => e.IsTemplate);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => new { e.IsTemplate, e.TemplateName })
                .HasFilter("[IsTemplate] = 1 AND [TemplateName] IS NOT NULL");
        });
    }
}

