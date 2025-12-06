using System;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

namespace whiteboard_app.Services;

/// <summary>
/// A service that manages the application's theme (Light, Dark, System) and synchronizes with system settings.
/// </summary>
public class ThemeService : IThemeService
{
    private Window? _mainWindow;
    private UISettings _uiSettings;

    public event EventHandler<ElementTheme> ThemeChanged = delegate { };

    public ThemeService()
    {
        _uiSettings = new UISettings();
        _uiSettings.ColorValuesChanged += UiSettings_ColorValuesChanged;
        CurrentTheme = ElementTheme.Default;
    }

    public ElementTheme CurrentTheme { get; private set; }

    public bool IsLightTheme => CurrentTheme == ElementTheme.Light;

    public bool IsDarkTheme => CurrentTheme == ElementTheme.Dark;

    /// <summary>
    /// Sets the main window of the application to apply theme changes.
    /// This should be called once during application startup.
    /// </summary>
    /// <param name="mainWindow">The main window of the application.</param>
    public void SetMainWindow(Window mainWindow)
    {
        _mainWindow = mainWindow;
        ApplyThemeToMainWindow();
    }

    public void SetTheme(ElementTheme theme)
    {
        if (CurrentTheme != theme)
        {
            CurrentTheme = theme;
            ApplyThemeToMainWindow();
            ThemeChanged?.Invoke(this, CurrentTheme);
        }
    }

    public void SyncToSystemTheme()
    {
        SetTheme(ElementTheme.Default);
    }

    private void UiSettings_ColorValuesChanged(UISettings sender, object args)
    {
        _mainWindow?.DispatcherQueue.TryEnqueue(() =>
        {
            if (CurrentTheme == ElementTheme.Default)
            {
                ApplyThemeToMainWindow();
                ThemeChanged?.Invoke(this, CurrentTheme);
            }
        });
    }

    private void ApplyThemeToMainWindow()
    {
        if (_mainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = CurrentTheme;
        }
    }
}

