# GuiGuBaHuang-ModCreator Instructions

## Project Overview
ModCreator tool for creating GuiGuBaHuang game mods. Built entirely with Copilot - code must be minimal.

## Code Minimization Rules
- Remove unnecessary `using` statements
- Remove unnecessary comments
- No `try-catch` unless critical (App.xaml.cs handles global exceptions)
- Minimize `.xaml` and `.cs` code
- Regex patterns: create const/static readonly for reuse

## Architecture Patterns

### Windows & Context
- Windows inherit from `CWindow`
- Window context data in `WindowData/` folder, inherit from `CWindowData`
- Separation: `.xaml` (layout), `*Window.xaml.cs` (event triggers), `*WindowData.cs` (binding context & data processing)

### Models & Binding
- Models use `AutoNotifiableObject` for window binding (no `PropertyChanged`)
- Prefer binding over code-behind

### Business Logic
- Business processing in `Businesses/*Business.cs`

## Resource Management

### Messages & Text
- Messages managed in `Resources/messages.json`
- Window text managed in `UITextHelper` for binding

### Styles & Constants
- XAML styles in `Styles/` folder for reuse
- Reuse `Constants.cs`

### Settings & Master Data
- Settings: `embeded-settings.json` (embedded, read-only) + `settings.json` (user-editable after release)
- Master data lists in `Resources/*.json` (embedded, e.g., `image-extensions.json`)

## Helper Classes

### Existing Helpers
**Always check and reuse existing Helpers before creating new code:**

- **AvalonHelper**: AvalonEdit operations (syntax highlighting for JSON/C#)
- **BitmapHelper**: Image loading/operations (with size control, validation)
- **DebugHelper**: Logging with levels (Debug/Info/Warning/Error/Fatal), log file management, MessageBox display
- **FileHelper**: File/directory operations (read/write UTF-8, copy directory, relative path, ensure directory exists)
- **MessageHelper**: Load messages from `messages.json` (Get, GetFormat methods) - use in C# code
- **ObjectHelper**: Object comparison/cloning (property comparison, deep clone, copy properties, get differences)
- **ProjectHelper**: Project CRUD operations (create from template, load/save projects, apply replacements)
- **ResourceHelper**: Load embedded resources (read as string or deserialize to type)
- **SettingHelper**: Settings management (load from `embeded-settings.json` + `settings.json`)
- **UITextHelper**: Window text bindings for XAML (MainWindowText, AboutWindowText, HelpWindowText, etc.)

### Helper Creation Rules
- **Check existing Helpers first** - reuse before creating new code
- **Create new Helper** when similar processing appears in multiple places
- Extract common logic to Helper with static methods
- Name Helper by domain: `[Domain]Helper.cs` (e.g., `JsonHelper`, `ValidationHelper`)
- Keep Helpers focused on single responsibility

## File Organization
- One class per file (no nested classes for long files)
