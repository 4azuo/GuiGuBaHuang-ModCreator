# ModCreator

A visual WPF tool for creating and managing mods for the GuiGuBaHuang game. No coding required - intuitive interface for mod creation.

## ✨ Features

- ✅ **Project Management** - Create, edit, delete, and organize mod projects
- ✅ **Guidelines** - Built-in documentation and best practices
- ✅ **ModConf Editor** - Configure mod metadata and settings
- ✅ **ModImg Manager** - Manage mod images and assets
- ✅ **ModEvent Editor** - Create and edit game events visually
- ✅ **User-Friendly** - No coding knowledge required

## 🚀 Installation & Running

**Requirements**: .NET 9, Visual Studio 2022+

### Installation Steps

1. **Clone the main repository** (GuiGuBaHuang-ModLib, not ModCreator)
   ```powershell
   cd C:\git
   git clone https://github.com/4azuo/GuiGuBaHuang-ModLib.git
   cd GuiGuBaHuang-ModLib
   git submodule update --init --recursive
   ```

2. **Configure settings**
   - Edit `GuiGuBaHuang-ModCreator\ModCreator\Resources\embeded-settings.json`
   - Update paths if needed (default is `C:\git\GuiGuBaHuang-ModLib`)

3. **Run the application**
   ```powershell
   cd GuiGuBaHuang-ModCreator
   .\run.ps1
   ```

### Build & Run Manually
```powershell
.\build.ps1
.\ModCreator\bin\Release\ModCreator.exe
```
