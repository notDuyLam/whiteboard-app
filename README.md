# Whiteboard App - WinUI 3 Application

A drawing application built with WinUI 3, following MVVM pattern and Fluent Design principles.

## Project Structure

### Solution Overview
- **whiteboard-app**: WinUI 3 application (main project)
- **whiteboard-app-data**: Class Library containing data models, DbContext, and migrations

### Project References
- `whiteboard-app` references `whiteboard-app-data` for data access layer

## Technology Stack

- **.NET 8**
- **WinUI 3** (Windows App SDK)
- **Entity Framework Core** with SQLite
- **MVVM Toolkit** (CommunityToolkit.Mvvm)
- **Dependency Injection** (Microsoft.Extensions.DependencyInjection)
- **LiveCharts** for Dashboard statistics

## Getting Started

### Prerequisites
- Windows 10 version 1809 or later
- Visual Studio 2022 with Windows App SDK workload
- .NET 8 SDK

### Building the Solution
1. Open `whiteboard-app.sln` in Visual Studio 2022
2. Restore NuGet packages
3. Build the solution (Ctrl+Shift+B)

## Project Architecture

### whiteboard-app-data
- **Models/**: Entity models (Profile, Canvas, Shape, etc.)
- **Data/**: DbContext and database initialization
- **Migrations/**: EF Core migrations
- **Enums/**: Enumerations (ShapeType, StrokeStyle)

### whiteboard-app
- **ViewModels/**: MVVM ViewModels
- **Views/**: XAML pages and windows
- **Services/**: Business logic and services (Navigation, Theme, Data, Drawing)
- **Controls/**: Custom controls (DrawingCanvas)
- **Helpers/**: Utility classes
- **Converters/**: Value converters
- **Styles/**: Custom styles and resources

## Development Status

This project is in active development following the plan outlined in `PLAN.md`.

