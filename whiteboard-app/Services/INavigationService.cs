using System;
using Microsoft.UI.Xaml.Controls;

namespace whiteboard_app.Services;

/// <summary>
/// Interface for a navigation service that manages page navigation within the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets a value indicating whether there is at least one entry in the back navigation history.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Navigates to the most recent item in the back navigation history.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <param name="pageType">The type of the page to navigate to.</param>
    /// <param name="parameter">An optional parameter to pass to the target page.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool NavigateTo(Type pageType, object? parameter = null);

    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <typeparam name="TPage">The type of the page to navigate to.</typeparam>
    /// <param name="parameter">An optional parameter to pass to the target page.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool NavigateTo<TPage>(object? parameter = null) where TPage : Page;

    /// <summary>
    /// Clears the navigation history.
    /// </summary>
    void ClearHistory();

    /// <summary>
    /// Sets the Frame used for navigation. This should typically be called once during application startup.
    /// </summary>
    /// <param name="frame">The Frame control to use for navigation.</param>
    void SetNavigationFrame(Frame frame);
}

