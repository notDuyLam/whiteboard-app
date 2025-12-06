using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using whiteboard_app_data.Enums;
using whiteboard_app_data.Models;

namespace whiteboard_app.Services;

/// <summary>
/// Interface for the drawing service, providing shape creation and serialization functionality.
/// </summary>
public interface IDrawingService
{
    /// <summary>
    /// Creates a Shape entity from shape data.
    /// </summary>
    /// <param name="shapeType">The type of shape to create.</param>
    /// <param name="canvasId">The ID of the canvas this shape belongs to.</param>
    /// <param name="strokeColor">The stroke color in hex format.</param>
    /// <param name="strokeThickness">The stroke thickness in pixels.</param>
    /// <param name="fillColor">The fill color in hex format or "Transparent".</param>
    /// <param name="serializedData">The serialized shape-specific data as JSON string.</param>
    /// <returns>A new Shape entity.</returns>
    Shape CreateShape(
        ShapeType shapeType,
        Guid? canvasId,
        string strokeColor,
        double strokeThickness,
        string fillColor,
        string serializedData);

    /// <summary>
    /// Serializes shape-specific data to JSON string.
    /// </summary>
    /// <typeparam name="T">The type of shape data to serialize.</typeparam>
    /// <param name="shapeData">The shape data object to serialize.</param>
    /// <returns>A JSON string representation of the shape data.</returns>
    string SerializeShapeData<T>(T shapeData) where T : class;

    /// <summary>
    /// Deserializes JSON string to shape-specific data.
    /// </summary>
    /// <typeparam name="T">The type of shape data to deserialize.</typeparam>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>A deserialized shape data object.</returns>
    T? DeserializeShapeData<T>(string jsonString) where T : class;
}

