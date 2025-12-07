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
        // Reload entity from DB to ensure it's tracked correctly
        var existingCanvas = await _context.Canvases.FindAsync(canvas.Id);
        if (existingCanvas == null)
        {
            throw new InvalidOperationException($"Canvas with Id {canvas.Id} not found in database");
        }
        
        // Update properties
        existingCanvas.Name = canvas.Name;
        existingCanvas.Width = canvas.Width;
        existingCanvas.Height = canvas.Height;
        existingCanvas.BackgroundColor = canvas.BackgroundColor;
        existingCanvas.ProfileId = canvas.ProfileId;
        existingCanvas.LastModifiedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existingCanvas;
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
        System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - START: canvasId={canvasId}");
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
                // First, let's check what CanvasId values actually exist in the database
                using var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "SELECT DISTINCT CanvasId FROM Shapes WHERE CanvasId IS NOT NULL LIMIT 10";
                var canvasIds = new List<string>();
                using (var canvasIdReader = await checkCommand.ExecuteReaderAsync())
                {
                    while (await canvasIdReader.ReadAsync())
                    {
                        if (!canvasIdReader.IsDBNull(0))
                        {
                            canvasIds.Add(canvasIdReader.GetString(0));
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - Found CanvasIds in database: {string.Join(", ", canvasIds)}");
                System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - Looking for CanvasId: {canvasId.ToString()}");
                
                // Check if there are any shapes with this CanvasId at all
                // Use UPPER() to handle case-insensitive comparison since SQLite stores GUIDs in different cases
                using var checkCommand2 = connection.CreateCommand();
                checkCommand2.CommandText = "SELECT COUNT(*) FROM Shapes WHERE UPPER(CanvasId) = UPPER(@canvasId)";
                var checkParam = checkCommand2.CreateParameter();
                checkParam.ParameterName = "@canvasId";
                checkParam.Value = canvasId.ToString();
                checkCommand2.Parameters.Add(checkParam);
                var totalCount = Convert.ToInt32(await checkCommand2.ExecuteScalarAsync());
                System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - Total shapes with CanvasId {canvasId}: {totalCount}");
                
                // Also check without IsTemplate filter
                using var checkCommand3 = connection.CreateCommand();
                checkCommand3.CommandText = "SELECT COUNT(*) FROM Shapes WHERE UPPER(CanvasId) = UPPER(@canvasId) AND IsTemplate = 0";
                var checkParam2 = checkCommand3.CreateParameter();
                checkParam2.ParameterName = "@canvasId";
                checkParam2.Value = canvasId.ToString();
                checkCommand3.Parameters.Add(checkParam2);
                var nonTemplateCount = Convert.ToInt32(await checkCommand3.ExecuteScalarAsync());
                System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - Non-template shapes with CanvasId {canvasId}: {nonTemplateCount}");
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, ShapeType, StrokeColor, StrokeThickness, FillColor, 
                           IsTemplate, TemplateName, CreatedDate, CanvasId, SerializedData, StrokeStyle
                    FROM Shapes 
                    WHERE UPPER(CanvasId) = UPPER(@canvasId) AND IsTemplate = 0
                    ORDER BY CreatedDate";
                
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@canvasId";
                parameter.Value = canvasId.ToString();
                command.Parameters.Add(parameter);
                
                System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - Executing query with CanvasId: {canvasId.ToString()}");
                
                var shapes = new List<ShapeConcrete>();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var shape = new ShapeConcrete
                    {
                        Id = reader.GetGuid(0),
                        ShapeType = (whiteboard_app_data.Enums.ShapeType)reader.GetInt32(1),
                        StrokeColor = reader.IsDBNull(2) ? "#000000" : reader.GetString(2),
                        StrokeThickness = reader.GetDouble(3),
                        FillColor = reader.IsDBNull(4) ? "Transparent" : reader.GetString(4),
                        IsTemplate = reader.GetBoolean(5),
                        TemplateName = reader.IsDBNull(6) ? null : reader.GetString(6),
                        CreatedDate = reader.GetDateTime(7),
                        CanvasId = reader.IsDBNull(8) ? (Guid?)null : reader.GetGuid(8),
                        SerializedData = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                        StrokeStyle = reader.IsDBNull(10) ? whiteboard_app_data.Enums.StrokeStyle.Solid : (whiteboard_app_data.Enums.StrokeStyle)reader.GetInt32(10)
                    };
                    shapes.Add(shape);
                }
                
                System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - SUCCESS: Found {shapes.Count} shapes for canvas {canvasId}");
                return shapes.Cast<Shape>().ToList();
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
            System.Diagnostics.Debug.WriteLine($"[DataService] GetShapesByCanvasIdAsync - ERROR: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
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
        System.Diagnostics.Debug.WriteLine($"[DataService] CreateShapeAsync - START: Type={shape.ShapeType}, CanvasId={shape.CanvasId}, IsTemplate={shape.IsTemplate}");
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
            
            System.Diagnostics.Debug.WriteLine($"[DataService] CreateShapeAsync - Adding shape to context: Id={shape.Id}, CanvasId={shape.CanvasId}, SerializedData length={shape.SerializedData?.Length ?? 0}");
            
            _context.Shapes.Add(shape);
            var savedCount = await _context.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"[DataService] CreateShapeAsync - SaveChangesAsync returned: {savedCount} entities saved");
            
            // Verify the shape was saved correctly by querying it back
            var verifyShape = await _context.ShapeConcretes.FindAsync(shape.Id);
            if (verifyShape != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DataService] CreateShapeAsync - Verified saved shape: Id={verifyShape.Id}, CanvasId={verifyShape.CanvasId}, IsTemplate={verifyShape.IsTemplate}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DataService] CreateShapeAsync - WARNING: Could not verify saved shape!");
            }
            
            System.Diagnostics.Debug.WriteLine($"[DataService] CreateShapeAsync - SUCCESS: Shape saved with Id={shape.Id}");
            return shape;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DataService] CreateShapeAsync - ERROR: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    public async Task<Shape> UpdateShapeAsync(Shape shape)
    {
        // Reload entity from DB to ensure it's tracked correctly
        var existingShape = await _context.ShapeConcretes.FindAsync(shape.Id);
        if (existingShape == null)
        {
            throw new InvalidOperationException($"Shape with Id {shape.Id} not found in database");
        }
        
        // Update properties
        existingShape.ShapeType = shape.ShapeType;
        existingShape.StrokeColor = shape.StrokeColor;
        existingShape.StrokeThickness = shape.StrokeThickness;
        existingShape.FillColor = shape.FillColor;
        existingShape.StrokeStyle = shape.StrokeStyle;
        existingShape.SerializedData = shape.SerializedData;
        existingShape.CanvasId = shape.CanvasId;
        existingShape.IsTemplate = shape.IsTemplate;
        existingShape.TemplateName = shape.TemplateName;
        
        await _context.SaveChangesAsync();
        return existingShape;
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


