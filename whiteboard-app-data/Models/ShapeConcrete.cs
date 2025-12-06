namespace whiteboard_app_data.Models;

/// <summary>
/// A concrete implementation of the abstract Shape class.
/// This class is primarily used by Entity Framework Core for migrations
/// when configuring Table-Per-Hierarchy (TPH) inheritance for the abstract Shape base class.
/// It should not be instantiated directly in application logic.
/// </summary>
public class ShapeConcrete : Shape
{
    // No additional properties needed for this concrete type,
    // as all common properties are in the base Shape class.
    // The discriminator will differentiate actual shape types.
}

