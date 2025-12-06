using System;
using Microsoft.UI.Xaml;

namespace whiteboard_app.Services;

/// <summary>
/// Interface for a service that manages the application's theme (Light, Dark, System).
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current application theme.
    /// </summary>
    ElementTheme CurrentTheme { get; }

    /// <summary>
    /// Gets a value indicating whether the current theme is Light.
    /// </summary>
    bool IsLightTheme { get; }

    /// <summary>
    /// Gets a value indicating whether the current theme is Dark.
    /// </summary>
    bool IsDarkTheme { get; }

    /// <summary>
    /// Event raised when the application theme changes.
    /// </summary>
    event EventHandler<ElementTheme> ThemeChanged;

    /// <summary>
    /// Sets the application theme.
    /// </summary>
    /// <param name="theme">The theme to apply (Light, Dark, or System).</param>
    void SetTheme(ElementTheme theme);

    /// <summary>
    /// Synchronizes the application theme with the system theme.
    /// </summary>
    void SyncToSystemTheme();

    /// <summary>
    /// Sets the main window of the application to apply theme changes.
    /// This should be called once during application startup.
    /// </summary>
    /// <param name="mainWindow">The main window of the application.</param>
    void SetMainWindow(Window mainWindow);
}

