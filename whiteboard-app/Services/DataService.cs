using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using whiteboard_app_data.Data;
using whiteboard_app_data.Models;
using ShapeConcrete = whiteboard_app_data.Models.ShapeConcrete;

namespace whiteboard_app.Services;

/// <summary>
/// A service that provides repository pattern for database operations using Entity Framework Core.
/// </summary>
public class DataService : IDataService
{
    private readonly WhiteboardDbContext _context;

    public DataService(WhiteboardDbContext context)
    {
        _context = context;
    }

    // Profile operations
    public async Task<List<Profile>> GetAllProfilesAsync()
    {
        return await _context.Profiles
            .Include(p => p.Canvases)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Profile?> GetProfileByIdAsync(Guid id)
    {
        return await _context.Profiles
            .Include(p => p.Canvases)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Profile?> GetActiveProfileAsync()
    {
        return await _context.Profiles
            .Include(p => p.Canvases)
            .FirstOrDefaultAsync(p => p.IsActive);
    }

    public async Task<Profile> CreateProfileAsync(Profile profile)
    {
        profile.Id = Guid.NewGuid();
        profile.CreatedDate = DateTime.UtcNow;
        profile.LastModifiedDate = DateTime.UtcNow;

        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<Profile> UpdateProfileAsync(Profile profile)
    {
        profile.LastModifiedDate = DateTime.UtcNow;
        _context.Profiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<bool> DeleteProfileAsync(Guid id)
    {
        var profile = await _context.Profiles.FindAsync(id);
        if (profile == null)
            return false;

        _context.Profiles.Remove(profile);
        await _context.SaveChangesAsync();
        return true;
    }

    // Canvas operations
    public async Task<List<Canvas>> GetAllCanvasesAsync()
    {
        return await _context.Canvases
            .Include(c => c.Profile)
            .Include(c => c.Shapes)
            .OrderByDescending(c => c.LastModifiedDate)
            .ToListAsync();
    }

    public async Task<List<Canvas>> GetCanvasesByProfileIdAsync(Guid profileId)
    {
        return await _context.Canvases
            .Include(c => c.Shapes)
            .Where(c => c.ProfileId == profileId)
            .OrderByDescending(c => c.LastModifiedDate)
            .ToListAsync();
    }

    public async Task<Canvas?> GetCanvasByIdAsync(Guid id)
    {
        return await _context.Canvases
            .Include(c => c.Profile)
            .Include(c => c.Shapes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Canvas> CreateCanvasAsync(Canvas canvas)
    {
        canvas.Id = Guid.NewGuid();
        canvas.CreatedDate = DateTime.UtcNow;
        canvas.LastModifiedDate = DateTime.UtcNow;

        _context.Canvases.Add(canvas);
        await _context.SaveChangesAsync();
        return canvas;
    }

    public async Task<Canvas> UpdateCanvasAsync(Canvas canvas)
    {
        canvas.LastModifiedDate = DateTime.UtcNow;
        _context.Canvases.Update(canvas);
        await _context.SaveChangesAsync();
        return canvas;
    }

    public async Task<bool> DeleteCanvasAsync(Guid id)
    {
        var canvas = await _context.Canvases.FindAsync(id);
        if (canvas == null)
            return false;

        _context.Canvases.Remove(canvas);
        await _context.SaveChangesAsync();
        return true;
    }

    // Shape operations
    public async Task<List<Shape>> GetShapesByCanvasIdAsync(Guid canvasId)
    {
        return await _context.Shapes
            .Where(s => s.CanvasId == canvasId)
            .OrderBy(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Shape>> GetAllTemplatesAsync()
    {
        try
        {
            // Query using ShapeConcretes DbSet directly to avoid discriminator issues
            // This ensures EF Core materializes entities correctly
            var templates = await _context.ShapeConcretes
                .Where(s => s.IsTemplate && s.TemplateName != null)
                .OrderBy(s => s.TemplateName)
                .Cast<Shape>()
                .ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"[DataService] Found {templates.Count} templates");
            return templates;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] Error in GetAllTemplatesAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DataService] Inner: {ex.InnerException?.Message}");
            System.Diagnostics.Debug.WriteLine($"[DataService] StackTrace: {ex.StackTrace}");
            
            // Only clean up if there's a discriminator error
            if (ex.Message.Contains("discriminator", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM Shapes WHERE IsTemplate = 1");
                    System.Diagnostics.Debug.WriteLine($"[DataService] Deleted all templates due to discriminator error");
                }
                catch (Exception cleanupEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[DataService] Cleanup error: {cleanupEx.Message}");
                }
            }
            
            return new List<Shape>();
        }
    }

    public async Task<Shape?> GetShapeByIdAsync(Guid id)
    {
        return await _context.Shapes.FindAsync(id);
    }

    public async Task<Shape> CreateShapeAsync(Shape shape)
    {
        try
        {
            shape.Id = Guid.NewGuid();
            shape.CreatedDate = DateTime.UtcNow;

            // Log shape details before saving
            var shapeType = shape.GetType().Name;
            var isShapeConcrete = shape is ShapeConcrete;
            System.Diagnostics.Debug.WriteLine($"[DataService] Creating shape: Type={shape.ShapeType}, ShapeType={shapeType}, IsTemplate={shape.IsTemplate}, TemplateName={shape.TemplateName}, IsShapeConcrete={isShapeConcrete}");
            
            // Check if shape is actually ShapeConcrete
            if (!isShapeConcrete)
            {
                throw new InvalidOperationException($"Shape must be ShapeConcrete, but got {shapeType}");
            }
            
            _context.Shapes.Add(shape);
            System.Diagnostics.Debug.WriteLine($"[DataService] Shape added to context, about to save changes...");
            
            await _context.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"[DataService] Shape created successfully with Id: {shape.Id}");
            return shape;
        }
        catch (Exception ex)
        {
            var errorDetails = $"[DataService] Error creating shape: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorDetails += $"\nInner: {ex.InnerException.Message}";
            }
            errorDetails += $"\nType: {ex.GetType().Name}";
            errorDetails += $"\nStackTrace: {ex.StackTrace}";
            
            System.Diagnostics.Debug.WriteLine(errorDetails);
            System.Console.WriteLine(errorDetails); // Also write to console
            
            throw;
        }
    }

    public async Task<Shape> UpdateShapeAsync(Shape shape)
    {
        _context.Shapes.Update(shape);
        await _context.SaveChangesAsync();
        return shape;
    }

    public async Task<bool> DeleteShapeAsync(Guid id)
    {
        var shape = await _context.Shapes.FindAsync(id);
        if (shape == null)
            return false;

        _context.Shapes.Remove(shape);
        await _context.SaveChangesAsync();
        return true;
    }

    // Save changes
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

