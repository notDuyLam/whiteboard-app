using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace whiteboard_app_data.Data;

/// <summary>
/// Design-time factory for WhiteboardDbContext.
/// This is required for EF Core CLI tools (e.g., migrations) to create a DbContext instance.
/// </summary>
public class WhiteboardDbContextFactory : IDesignTimeDbContextFactory<WhiteboardDbContext>
{
    public WhiteboardDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WhiteboardDbContext>();
        // Use the default connection string logic from OnConfiguring
        return new WhiteboardDbContext(optionsBuilder.Options);
    }
}

