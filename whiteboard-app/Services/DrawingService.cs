using System;
using System.Text.Json;
using whiteboard_app_data.Enums;
using whiteboard_app_data.Models;

namespace whiteboard_app.Services;

/// <summary>
/// Implementation of IDrawingService, providing shape creation and serialization functionality.
/// </summary>
public class DrawingService : IDrawingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Shape CreateShape(
        ShapeType shapeType,
        Guid? canvasId,
        string strokeColor,
        double strokeThickness,
        string fillColor,
        string serializedData)
    {
        return new ShapeConcrete
        {
            ShapeType = shapeType,
            CanvasId = canvasId,
            StrokeColor = strokeColor,
            StrokeThickness = strokeThickness,
            FillColor = fillColor,
            SerializedData = serializedData,
            CreatedDate = DateTime.UtcNow
        };
    }

    public string SerializeShapeData<T>(T shapeData) where T : class
    {
        if (shapeData == null)
            throw new ArgumentNullException(nameof(shapeData));

        return JsonSerializer.Serialize(shapeData, JsonOptions);
    }

    public T? DeserializeShapeData<T>(string jsonString) where T : class
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(jsonString, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

