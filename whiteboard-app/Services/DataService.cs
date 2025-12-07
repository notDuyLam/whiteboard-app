using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;
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
        System.Diagnostics.Debug.WriteLine("[DataService] GetAllProfilesAsync - START");
        try
        {
            // Don't include Canvases to avoid loading Shapes which causes discriminator issues
            var result = await _context.Profiles
                .OrderBy(p => p.Name)
                .ToListAsync();
            System.Diagnostics.Debug.WriteLine($"[DataService] GetAllProfilesAsync - SUCCESS: {result.Count} profiles");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] GetAllProfilesAsync - ERROR: {ex.Message}");
            throw;
        }
    }

    public async Task<Profile?> GetProfileByIdAsync(Guid id)
    {
        // Don't include Canvases to avoid loading Shapes which causes discriminator issues
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Profile?> GetActiveProfileAsync()
    {
        // Don't include Canvases to avoid loading Shapes which causes discriminator issues
        return await _context.Profiles
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
        // Don't include Shapes to avoid discriminator issues
        // Shapes will be loaded separately when needed
        return await _context.Canvases
            .Include(c => c.Profile)
            .OrderByDescending(c => c.LastModifiedDate)
            .ToListAsync();
    }

    public async Task<List<Canvas>> GetCanvasesByProfileIdAsync(Guid profileId)
    {
        System.Diagnostics.Debug.WriteLine($"[DataService] GetCanvasesByProfileIdAsync - START: profileId={profileId}");
        try
        {
            // Don't include Shapes to avoid discriminator issues
            // Shapes will be loaded separately when needed
            var result = await _context.Canvases
                .Include(c => c.Profile)
                .Where(c => c.ProfileId == profileId)
                .OrderByDescending(c => c.LastModifiedDate)
                .ToListAsync();
            System.Diagnostics.Debug.WriteLine($"[DataService] GetCanvasesByProfileIdAsync - SUCCESS: {result.Count} canvases");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] GetCanvasesByProfileIdAsync - ERROR: {ex.Message}");
            throw;
        }
    }

    public async Task<Canvas?> GetCanvasByIdAsync(Guid id)
    {
        // Don't include Shapes to avoid discriminator issues
        // Shapes will be loaded separately when needed
        return await _context.Canvases
            .Include(c => c.Profile)
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
        // Use ShapeConcretes to avoid discriminator issues
        return await _context.ShapeConcretes
            .Where(s => s.CanvasId == canvasId)
            .OrderBy(s => s.CreatedDate)
            .Cast<Shape>()
            .ToListAsync();
    }

    public async Task<List<Shape>> GetAllTemplatesAsync()
    {
        System.Diagnostics.Debug.WriteLine("[DataService] GetAllTemplatesAsync - START");
        try
        {
            // Use raw SQL to avoid discriminator issues when materializing entities
            // Query only the fields we need and create ShapeConcrete objects manually
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen)
            {
                await connection.OpenAsync();
            }
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, ShapeType, StrokeColor, StrokeThickness, FillColor, 
                           IsTemplate, TemplateName, CreatedDate, CanvasId, SerializedData, StrokeStyle
                    FROM Shapes 
                    WHERE IsTemplate = 1 AND TemplateName IS NOT NULL
                    ORDER BY CreatedDate DESC";
                
                var templates = new List<ShapeConcrete>();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var template = new ShapeConcrete
                    {
                        Id = reader.GetGuid(0),
                        ShapeType = (whiteboard_app_data.Enums.ShapeType)reader.GetInt32(1),
                        StrokeColor = reader.IsDBNull(2) ? null : reader.GetString(2),
                        StrokeThickness = reader.GetDouble(3),
                        FillColor = reader.IsDBNull(4) ? null : reader.GetString(4),
                        IsTemplate = reader.GetBoolean(5),
                        TemplateName = reader.IsDBNull(6) ? null : reader.GetString(6),
                        CreatedDate = reader.GetDateTime(7),
                        CanvasId = reader.IsDBNull(8) ? (Guid?)null : reader.GetGuid(8),
                        SerializedData = reader.IsDBNull(9) ? null : reader.GetString(9),
                        StrokeStyle = reader.IsDBNull(10) ? whiteboard_app_data.Enums.StrokeStyle.Solid : (whiteboard_app_data.Enums.StrokeStyle)reader.GetInt32(10)
                    };
                    templates.Add(template);
                }
                
                System.Diagnostics.Debug.WriteLine($"[DataService] Found {templates.Count} templates via raw SQL");
                
                // Fix incorrect ShapeType for Line templates that were saved as Polygon
                // NOTE: Batch save instead of saving in loop to avoid blocking
                var templatesToFix = new List<ShapeConcrete>();
                foreach (var template in templates)
                {
                    if (!string.IsNullOrEmpty(template.SerializedData))
                    {
                        if (template.SerializedData.Contains("startX") && template.SerializedData.Contains("endX"))
                        {
                            // Fix the ShapeType if it's wrong
                            if (template.ShapeType != whiteboard_app_data.Enums.ShapeType.Line)
                            {
                                System.Diagnostics.Debug.WriteLine($"[DataService] Fixing ShapeType for template {template.Id}");
                                template.ShapeType = whiteboard_app_data.Enums.ShapeType.Line;
                                templatesToFix.Add(template);
                            }
                        }
                    }
                }
                
                if (templatesToFix.Count > 0)
                {
                    // Batch update all fixed templates
                    _context.Shapes.UpdateRange(templatesToFix);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[DataService] Fixed {templatesToFix.Count} template ShapeTypes");
                }
                
                return templates.Cast<Shape>().ToList();
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] GetAllTemplatesAsync - ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DataService] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DataService] Inner: {ex.InnerException.Message}");
            }
            
            // Only clean up if there's a discriminator error
            if (ex.Message.Contains("discriminator", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine("[DataService] Cleaning up templates due to discriminator error...");
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM Shapes WHERE IsTemplate = 1");
                    System.Diagnostics.Debug.WriteLine("[DataService] Cleanup completed");
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
        // Use ShapeConcretes to avoid discriminator issues
        return await _context.ShapeConcretes.FindAsync(id);
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
        catch
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

    // Statistics operations - use raw SQL to avoid discriminator issues
    public async Task<int> GetTotalShapesCountAsync()
    {
        System.Diagnostics.Debug.WriteLine("[DataService] GetTotalShapesCountAsync - START");
        try
        {
            // Use raw SQL to count shapes without materializing entities
            // This avoids discriminator issues completely
            System.Diagnostics.Debug.WriteLine("[DataService] Opening database connection...");
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen)
            {
                await connection.OpenAsync();
                System.Diagnostics.Debug.WriteLine("[DataService] Database connection opened");
            }
            try
            {
                System.Diagnostics.Debug.WriteLine("[DataService] Executing SQL query...");
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Shapes WHERE IsTemplate = 0";
                var count = await command.ExecuteScalarAsync();
                var result = count != null ? Convert.ToInt32(count) : 0;
                System.Diagnostics.Debug.WriteLine($"[DataService] GetTotalShapesCountAsync - SUCCESS: {result} shapes");
                return result;
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                    System.Diagnostics.Debug.WriteLine("[DataService] Database connection closed");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] GetTotalShapesCountAsync - ERROR: {ex.Message}");
            // Return 0 if there's any error
            return 0;
        }
    }

    public async Task<Dictionary<whiteboard_app_data.Enums.ShapeType, int>> GetShapeTypeStatisticsAsync()
    {
        System.Diagnostics.Debug.WriteLine("[DataService] GetShapeTypeStatisticsAsync - START");
        try
        {
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen)
            {
                await connection.OpenAsync();
            }
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT ShapeType, COUNT(*) as Count 
                    FROM Shapes 
                    WHERE IsTemplate = 0 
                    GROUP BY ShapeType";
                
                var statistics = new Dictionary<whiteboard_app_data.Enums.ShapeType, int>();
                
                // Initialize all shape types with 0
                foreach (whiteboard_app_data.Enums.ShapeType shapeType in Enum.GetValues(typeof(whiteboard_app_data.Enums.ShapeType)))
                {
                    statistics[shapeType] = 0;
                }
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var shapeType = (whiteboard_app_data.Enums.ShapeType)reader.GetInt32(0);
                    var count = reader.GetInt32(1);
                    statistics[shapeType] = count;
                }
                
                System.Diagnostics.Debug.WriteLine($"[DataService] GetShapeTypeStatisticsAsync - SUCCESS");
                return statistics;
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] GetShapeTypeStatisticsAsync - ERROR: {ex.Message}");
            // Return empty dictionary if there's any error
            return new Dictionary<whiteboard_app_data.Enums.ShapeType, int>();
        }
    }

    public async Task<List<(string TemplateName, int UsageCount)>> GetTopTemplatesAsync(int topCount = 10)
    {
        System.Diagnostics.Debug.WriteLine("[DataService] GetTopTemplatesAsync - START");
        try
        {
            // For now, return templates sorted by creation date
            // In the future, we could track actual usage count if needed
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen)
            {
                await connection.OpenAsync();
            }
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $@"
                    SELECT TemplateName, COUNT(*) as UsageCount
                    FROM Shapes 
                    WHERE IsTemplate = 1 AND TemplateName IS NOT NULL
                    GROUP BY TemplateName
                    ORDER BY COUNT(*) DESC
                    LIMIT {topCount}";
                
                var templates = new List<(string TemplateName, int UsageCount)>();
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var templateName = reader.GetString(0);
                    var usageCount = reader.GetInt32(1);
                    templates.Add((templateName, usageCount));
                }
                
                System.Diagnostics.Debug.WriteLine($"[DataService] GetTopTemplatesAsync - SUCCESS: {templates.Count} templates");
                return templates;
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] GetTopTemplatesAsync - ERROR: {ex.Message}");
            return new List<(string TemplateName, int UsageCount)>();
        }
    }

    // Save changes
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}


