using System;
using System.Text.Json;
using whiteboard_app_data.Enums;
using whiteboard_app_data.Models;
using whiteboard_app_data.Models.ShapeTypes;

namespace whiteboard_app_data.Data;

/// <summary>
/// Static class for initializing and seeding the database with default data.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database and seeds it with default data if it's empty.
    /// Note: Database migrations should be applied before calling this method.
    /// </summary>
    /// <param name="context">The database context to initialize.</param>
    public static void Initialize(WhiteboardDbContext context)
    {
        try
        {
            // Check if database has been seeded
            if (context.Profiles.Any())
            {
                return; // Database has been seeded
            }

            SeedProfiles(context);
            SeedCanvases(context);
            SeedTemplates(context);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Forces seeding of database data, even if profiles already exist.
    /// Use with caution - this will add duplicate data if profiles already exist.
    /// </summary>
    public static void ForceSeed(WhiteboardDbContext context)
    {
        SeedProfiles(context);
        SeedCanvases(context);
        SeedTemplates(context);
    }

    private static void SeedProfiles(WhiteboardDbContext context)
    {
        var profiles = new List<Profile>
        {
            new Profile
            {
                Name = "Default Light",
                Theme = "Light",
                DefaultCanvasWidth = 800,
                DefaultCanvasHeight = 600,
                DefaultStrokeColor = "#000000",
                DefaultStrokeThickness = 2.0,
                DefaultFillColor = "Transparent",
                IsActive = true
            },
            new Profile
            {
                Name = "Default Dark",
                Theme = "Dark",
                DefaultCanvasWidth = 1200,
                DefaultCanvasHeight = 800,
                DefaultStrokeColor = "#FFFFFF",
                DefaultStrokeThickness = 2.5,
                DefaultFillColor = "Transparent",
                IsActive = false
            },
            new Profile
            {
                Name = "System Theme",
                Theme = "System",
                DefaultCanvasWidth = 1000,
                DefaultCanvasHeight = 700,
                DefaultStrokeColor = "#0066CC",
                DefaultStrokeThickness = 3.0,
                DefaultFillColor = "#E0E0E0",
                IsActive = false
            }
        };

        context.Profiles.AddRange(profiles);
        context.SaveChanges();
    }

    private static void SeedCanvases(WhiteboardDbContext context)
    {
        var profile = context.Profiles.First();
        var canvases = new List<Canvas>
        {
            new Canvas
            {
                Name = "Sample Canvas 1",
                Width = 800,
                Height = 600,
                BackgroundColor = "#FFFFFF",
                ProfileId = profile.Id
            },
            new Canvas
            {
                Name = "Sample Canvas 2",
                Width = 1200,
                Height = 800,
                BackgroundColor = "#F5F5F5",
                ProfileId = profile.Id
            }
        };

        context.Canvases.AddRange(canvases);
        context.SaveChanges();

        // Add some sample shapes to the first canvas
        var canvas1 = canvases[0];
        SeedShapesForCanvas(context, canvas1);
    }

    private static void SeedShapesForCanvas(WhiteboardDbContext context, Canvas canvas)
    {
        var shapes = new List<Shape>
        {
            new ShapeConcrete
            {
                ShapeType = ShapeType.Line,
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "Transparent",
                CanvasId = canvas.Id,
                SerializedData = JsonSerializer.Serialize(new LineShapeData
                {
                    StartX = 100,
                    StartY = 100,
                    EndX = 300,
                    EndY = 200
                })
            },
            new ShapeConcrete
            {
                ShapeType = ShapeType.Rectangle,
                StrokeColor = "#FF0000",
                StrokeThickness = 2.5,
                FillColor = "#FFE0E0",
                CanvasId = canvas.Id,
                SerializedData = JsonSerializer.Serialize(new RectangleShapeData
                {
                    X = 350,
                    Y = 150,
                    Width = 200,
                    Height = 150
                })
            },
            new ShapeConcrete
            {
                ShapeType = ShapeType.Circle,
                StrokeColor = "#0000FF",
                StrokeThickness = 3.0,
                FillColor = "#E0E0FF",
                CanvasId = canvas.Id,
                SerializedData = JsonSerializer.Serialize(new CircleShapeData
                {
                    CenterX = 600,
                    CenterY = 300,
                    Radius = 75
                })
            }
        };

        context.Shapes.AddRange(shapes);
        context.SaveChanges();
    }

    private static void SeedTemplates(WhiteboardDbContext context)
    {
        var templates = new List<Shape>
        {
            new ShapeConcrete
            {
                ShapeType = ShapeType.Rectangle,
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "Transparent",
                IsTemplate = true,
                TemplateName = "Standard Rectangle",
                SerializedData = JsonSerializer.Serialize(new RectangleShapeData
                {
                    X = 0,
                    Y = 0,
                    Width = 100,
                    Height = 100
                })
            },
            new ShapeConcrete
            {
                ShapeType = ShapeType.Circle,
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "Transparent",
                IsTemplate = true,
                TemplateName = "Standard Circle",
                SerializedData = JsonSerializer.Serialize(new CircleShapeData
                {
                    CenterX = 50,
                    CenterY = 50,
                    Radius = 50
                })
            },
            new ShapeConcrete
            {
                ShapeType = ShapeType.Triangle,
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "Transparent",
                IsTemplate = true,
                TemplateName = "Standard Triangle",
                SerializedData = JsonSerializer.Serialize(new TriangleShapeData
                {
                    Point1X = 50,
                    Point1Y = 0,
                    Point2X = 0,
                    Point2Y = 100,
                    Point3X = 100,
                    Point3Y = 100
                })
            },
            new ShapeConcrete
            {
                ShapeType = ShapeType.Line,
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "Transparent",
                IsTemplate = true,
                TemplateName = "Standard Line",
                SerializedData = JsonSerializer.Serialize(new LineShapeData
                {
                    StartX = 0,
                    StartY = 0,
                    EndX = 100,
                    EndY = 100
                })
            },
            new ShapeConcrete
            {
                ShapeType = ShapeType.Oval,
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "Transparent",
                IsTemplate = true,
                TemplateName = "Standard Oval",
                SerializedData = JsonSerializer.Serialize(new OvalShapeData
                {
                    CenterX = 50,
                    CenterY = 50,
                    RadiusX = 60,
                    RadiusY = 40
                })
            },
            new ShapeConcrete
            {
                ShapeType = ShapeType.Polygon,
                StrokeColor = "#000000",
                StrokeThickness = 2.0,
                FillColor = "Transparent",
                IsTemplate = true,
                TemplateName = "Standard Hexagon",
                SerializedData = JsonSerializer.Serialize(new PolygonShapeData
                {
                    Points = new List<PointData>
                    {
                        new PointData { X = 50, Y = 0 },
                        new PointData { X = 100, Y = 25 },
                        new PointData { X = 100, Y = 75 },
                        new PointData { X = 50, Y = 100 },
                        new PointData { X = 0, Y = 75 },
                        new PointData { X = 0, Y = 25 }
                    }
                })
            }
        };

        context.Shapes.AddRange(templates);
        context.SaveChanges();
    }
}

