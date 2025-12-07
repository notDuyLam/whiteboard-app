# Whiteboard App - WinUI 3 Application

A modern drawing application built with WinUI 3, following MVVM pattern and Fluent Design principles. This application allows users to create, manage, and save drawings with various shapes and templates.

## Features

### 🎨 Drawing Capabilities
- **Shape Drawing**: Draw 6 types of shapes using mouse interaction:
  - Line
  - Rectangle
  - Oval
  - Circle
  - Triangle
  - Polygon (multi-point)
- **Stroke Customization**:
  - Stroke color (hex format)
  - Stroke thickness (1-20 pixels)
  - Stroke style (Solid, Dash, Dot)
- **Fill Color**: Fill shapes with custom colors or transparent
- **Selection & Editing**: Select, move, edit, and delete shapes
- **Canvas Management**: Create canvases with custom size and background color

### 📋 Canvas Management
- **Save & Load**: Save canvases with all shapes to database
- **Canvas Properties**: Customizable canvas size and background color
- **Draft System**: Shapes remain as drafts until explicitly saved
- **Canvas Deletion**: Remove canvases and all associated shapes

### 📚 Template System
- **Save as Template**: Save any drawn shape as a reusable template
- **Load Templates**: Quickly add saved templates to new canvases
- **Template Management**: View, manage, and delete saved templates

### 👤 Profile Management
- **Multiple Profiles**: Create and manage multiple user profiles
- **Profile Settings**: Each profile stores:
  - Theme preference (Light/Dark/System)
  - Default canvas size
  - Default stroke settings (color, thickness, style)
- **Profile-Specific Canvases**: All canvases are associated with a profile

### 📊 Dashboard & Statistics
- **Overview Dashboard**: View statistics including:
  - Total profiles
  - Total canvases
  - Total shapes
  - Total templates
- **Visual Charts**: 
  - Shape type distribution (Pie Chart)
  - Top templates usage (Column Chart)

### 🎯 Canvas Manager
- **View All Canvases**: Browse all canvases across all profiles
- **Canvas Details**: See canvas name, profile, creation date, and last modified date
- **Quick Access**: Open any canvas directly from the manager

### 🎨 Fluent Design
- **Custom TitleBar**: Redesigned title bar following Fluent Design guidelines
- **Mica Backdrop**: Modern backdrop material
- **NavigationView**: Responsive navigation with back button support
- **BreadcrumbBar**: Navigation breadcrumbs for better UX
- **Theme Support**: System theme synchronization with manual override
- **Windows 11 Icons**: Consistent iconography

### 📱 Responsive Design
- **Adaptive Layout**: Sidebars automatically adjust based on window size
- **Navigation Pane**: Auto-collapses to compact/minimal mode on smaller screens
- **Drawing Tools Panel**: Responsive sidebar with toggle button for narrow windows
- **Visual State Management**: Smooth transitions between layout states

## Technology Stack

- **.NET 8**
- **WinUI 3** (Windows App SDK)
- **Entity Framework Core** with SQLite
- **MVVM Toolkit** (CommunityToolkit.Mvvm)
- **Dependency Injection** (Microsoft.Extensions.DependencyInjection)
- **LiveChartsCore** for Dashboard statistics and charts

## Project Structure

### Solution Overview
- **whiteboard-app**: WinUI 3 application (main project)
- **whiteboard-app-data**: Class Library containing data models, DbContext, and migrations

### Project References
- `whiteboard-app` references `whiteboard-app-data` for data access layer

### whiteboard-app-data
- **Models/**: Entity models (Profile, Canvas, Shape, ShapeConcrete)
- **Data/**: DbContext and database initialization
- **Migrations/**: EF Core migrations
- **Enums/**: Enumerations (ShapeType, StrokeStyle)
- **Models/ShapeTypes/**: Shape-specific data models (LineShapeData, RectangleShapeData, etc.)

### whiteboard-app
- **ViewModels/**: MVVM ViewModels (ProfileViewModel, etc.)
- **Views/**: XAML pages (HomePage, DrawingPage, ManagementPage, ProfilePage)
- **Services/**: Business logic and services
  - `IDataService` / `DataService`: Database operations
  - `IDrawingService` / `DrawingService`: Shape serialization/deserialization
  - `INavigationService` / `NavigationService`: Page navigation
  - `IThemeService` / `ThemeService`: Theme management
- **Controls/**: Custom controls (`DrawingCanvas`)
- **Helpers/**: Utility classes
- **Converters/**: Value converters
- **Styles/**: Custom styles and resources

## Getting Started

### Prerequisites
- Windows 10 version 1809 or later (Windows 11 recommended)
- Visual Studio 2022 with Windows App SDK workload
- .NET 8 SDK

### Building the Solution
1. Clone or download the repository
2. Open `whiteboard-app.sln` in Visual Studio 2022
3. Restore NuGet packages:
   ```
   dotnet restore
   ```
4. Build the solution:
   - Press `Ctrl+Shift+B` in Visual Studio, or
   - Run `dotnet build` from the command line

### Running the Application
1. Set `whiteboard-app` as the startup project
2. Press `F5` to run in Debug mode, or `Ctrl+F5` to run without debugging
3. The application will start with the Home page

## Usage Guide

### Creating a Profile
1. Navigate to the **Home** page
2. Click **"Create New Profile"**
3. Fill in profile details:
   - Name
   - Theme preference
   - Default canvas size
   - Default stroke settings
4. Click **"Save"**

### Starting a Drawing Session
1. Select a profile from the **Home** page
2. Click **"Start Drawing"**, or
3. Navigate to **Drawing** tab (will prompt for profile selection)
4. The drawing canvas will load with profile-specific settings

### Drawing Shapes
1. Select a drawing tool from the sidebar (Line, Rectangle, etc.)
2. Configure stroke settings (color, thickness, style)
3. Set fill color if needed
4. Click and drag on the canvas to draw
5. For Polygon: Click multiple points, then double-click to finish

### Managing Shapes
1. Enable **Selection Mode** toggle
2. Click on a shape to select it
3. Use the following options:
   - **Edit Selected Shape**: Modify properties
   - **Fill Selected Shape**: Apply fill color
   - **Delete Selected Shape**: Remove from canvas
   - **Save as Template**: Save for reuse

### Saving Canvas
1. Draw your shapes (they remain as drafts)
2. Click **"Save Canvas"** button
3. Enter a canvas name (if creating new)
4. All shapes will be saved to the database

### Using Templates
1. Draw a shape and select it
2. Click **"Save as Template"** and provide a name
3. To use a template: Click **"Load Template"** and select from the list
4. Manage templates: Click **"Manage Templates"** to view/delete

### Viewing Statistics
1. Navigate to **Management** tab
2. View the **Dashboard** with:
   - Summary cards (profiles, canvases, shapes, templates)
   - Shape type distribution chart
   - Top templates chart
3. Click **"Canvas Manager"** to view all canvases

## Database

The application uses **SQLite** database with Entity Framework Core:
- Database file: `whiteboard.db` (created automatically in app data folder)
- **Table-Per-Hierarchy (TPH)** inheritance for Shape entities
- Automatic migrations on first run
- Data seeding for initial setup

### Entity Models
- **Profile**: User profiles with settings
- **Canvas**: Drawing canvases with properties
- **Shape**: Base class for all shapes (abstract)
- **ShapeConcrete**: Concrete implementation for EF Core TPH mapping

## Architecture

### MVVM Pattern
- ViewModels handle business logic and data binding
- Views are XAML-only with minimal code-behind
- Services are injected via Dependency Injection

### Dependency Injection
- Services registered in `App.xaml.cs`
- Services available via `App.ServiceProvider`
- Singleton services for DataService, NavigationService, etc.

### Custom Controls
- **DrawingCanvas**: Custom control handling all drawing operations, shape rendering, and selection

## Development Status

✅ **Completed Features:**
- Profile management (CRUD)
- Canvas creation and management
- All 6 shape types drawing
- Shape selection and editing
- Template system
- Save/Load functionality
- Dashboard with statistics
- Canvas Manager
- Responsive design
- Fluent Design implementation

🔄 **In Progress:**
- Polish and testing
- Performance optimization

## Known Issues & Limitations

- SQLite case sensitivity: GUID comparisons use `UPPER()` function for compatibility
- Shape serialization: Shape-specific data stored as JSON in `SerializedData` column
- Draft system: Shapes must be explicitly saved via "Save Canvas" button

## Contributing

This is a personal project for Windows Programming coursework. For questions or issues, please refer to the project documentation.

## License

This project is developed for educational purposes.

---

**Built with ❤️ using WinUI 3 and .NET 8**
