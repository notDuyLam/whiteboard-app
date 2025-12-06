using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace whiteboard_app.Services;

/// <summary>
/// A service that manages page navigation within the application using a Frame control.
/// </summary>
public class NavigationService : INavigationService
{
    private Frame? _navigationFrame;

    /// <summary>
    /// Gets a value indicating whether there is at least one entry in the back navigation history.
    /// </summary>
    public bool CanGoBack => _navigationFrame?.CanGoBack ?? false;

    /// <summary>
    /// Navigates to the most recent item in the back navigation history.
    /// </summary>
    public void GoBack()
    {
        if (_navigationFrame?.CanGoBack == true)
        {
            _navigationFrame.GoBack();
        }
    }

    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <param name="pageType">The type of the page to navigate to.</param>
    /// <param name="parameter">An optional parameter to pass to the target page.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    public bool NavigateTo(Type pageType, object? parameter = null)
    {
        if (_navigationFrame == null)
        {
            return false;
        }

        if (_navigationFrame.Content?.GetType() == pageType && parameter == null)
        {
            return false;
        }

        return _navigationFrame.Navigate(pageType, parameter);
    }

    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <typeparam name="TPage">The type of the page to navigate to.</typeparam>
    /// <param name="parameter">An optional parameter to pass to the target page.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    public bool NavigateTo<TPage>(object? parameter = null) where TPage : Page
    {
        return NavigateTo(typeof(TPage), parameter);
    }

    /// <summary>
    /// Clears the navigation history.
    /// </summary>
    public void ClearHistory()
    {
        _navigationFrame?.BackStack.Clear();
        _navigationFrame?.ForwardStack.Clear();
    }

    /// <summary>
    /// Sets the Frame used for navigation. This should typically be called once during application startup.
    /// </summary>
    /// <param name="frame">The Frame control to use for navigation.</param>
    public void SetNavigationFrame(Frame frame)
    {
        _navigationFrame = frame;
    }
}

