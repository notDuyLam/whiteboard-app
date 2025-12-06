using whiteboard_app_data.Models;

namespace whiteboard_app.Services;

/// <summary>
/// Interface for a data service that provides repository pattern for database operations.
/// </summary>
public interface IDataService
{
    // Profile operations
    Task<List<Profile>> GetAllProfilesAsync();
    Task<Profile?> GetProfileByIdAsync(Guid id);
    Task<Profile?> GetActiveProfileAsync();
    Task<Profile> CreateProfileAsync(Profile profile);
    Task<Profile> UpdateProfileAsync(Profile profile);
    Task<bool> DeleteProfileAsync(Guid id);

    // Canvas operations
    Task<List<Canvas>> GetAllCanvasesAsync();
    Task<List<Canvas>> GetCanvasesByProfileIdAsync(Guid profileId);
    Task<Canvas?> GetCanvasByIdAsync(Guid id);
    Task<Canvas> CreateCanvasAsync(Canvas canvas);
    Task<Canvas> UpdateCanvasAsync(Canvas canvas);
    Task<bool> DeleteCanvasAsync(Guid id);

    // Shape operations
    Task<List<Shape>> GetShapesByCanvasIdAsync(Guid canvasId);
    Task<List<Shape>> GetAllTemplatesAsync();
    Task<Shape?> GetShapeByIdAsync(Guid id);
    Task<Shape> CreateShapeAsync(Shape shape);
    Task<Shape> UpdateShapeAsync(Shape shape);
    Task<bool> DeleteShapeAsync(Guid id);

    // Save changes
    Task<int> SaveChangesAsync();
}

