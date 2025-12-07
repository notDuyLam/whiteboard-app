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
                .OrderByDescending(s => s.CreatedDate) // Sort by creation date, newest first
                .ToListAsync();
            
            // Fix incorrect ShapeType for Line templates that were saved as Polygon
            foreach (var template in templates)
            {
                if (!string.IsNullOrEmpty(template.SerializedData))
                {
                    if (template.SerializedData.Contains("startX") && template.SerializedData.Contains("endX"))
                    {
                        // Fix the ShapeType if it's wrong
                        if (template.ShapeType != whiteboard_app_data.Enums.ShapeType.Line)
                        {
                            template.ShapeType = whiteboard_app_data.Enums.ShapeType.Line;
                            _context.Shapes.Update(template);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            return templates.Cast<Shape>().ToList();
        }
        catch (Exception ex)
        {
            // Only clean up if there's a discriminator error
            if (ex.Message.Contains("discriminator", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM Shapes WHERE IsTemplate = 1");
                }
                catch
                {
                    // Ignore cleanup errors
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

            // Check if shape is actually ShapeConcrete
            var isShapeConcrete = shape is ShapeConcrete;
            if (!isShapeConcrete)
            {
                throw new InvalidOperationException($"Shape must be ShapeConcrete, but got {shape.GetType().Name}");
            }
            
            // Ensure ShapeType is explicitly set before adding to context
            // This is critical for EF Core discriminator mapping
            if (shape is ShapeConcrete concrete)
            {
                concrete.ShapeType = shape.ShapeType;
            }
            
            _context.Shapes.Add(shape);
            await _context.SaveChangesAsync();
            
            return shape;
        }
        catch (Exception ex)
        {
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

