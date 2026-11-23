# ModCreator

> [!WARNING]
> **Development is on hold indefinitely.** ModCreator is not maintained: no new features, no fixes, no support, and no
> date to resume. It may not work with the current version of the game or of ModLib. To make a mod, use
> [Fouru's ModLib](https://github.com/4azuo/GuiGuBaHuang-ModLib#create-a-mod-with-modlib) directly, with the mod projects
> and scripts of [GuiGuBaHuang-Taoist](https://github.com/4azuo/GuiGuBaHuang-Taoist) as the example.

A WPF tool (.NET 9) that was meant to create and edit mod projects of Gui Gu Ba Huang (鬼谷八荒) without code, on top of
[Fouru's ModLib](https://github.com/4azuo/GuiGuBaHuang-ModLib).

## What it has

- **Project management**: create, edit, delete and organize mod projects
- **ModConf editor**: game configs of a mod as JSON, from the samples of every config type
- **ModImg manager**: the images of a mod
- **Global variables** of a mod
- **ModEvent editor**: events of a mod built visually
- **Guidelines**: the documentation of the main repository, shown in the tool

Details per feature: [docs/00-Overview.md](docs/00-Overview.md).

## Build (as is)

ModCreator is the `GuiGuBaHuang-ModCreator` submodule of
[GuiGuBaHuang-Taoist](https://github.com/4azuo/GuiGuBaHuang-Taoist) and expects that repository around it.

```powershell
git clone --recursive https://github.com/4azuo/GuiGuBaHuang-Taoist.git
cd GuiGuBaHuang-Taoist\GuiGuBaHuang-ModCreator
.\build.ps1          # or .\run.ps1 to build and start it
.\ModCreator\bin\Release\ModCreator.exe
```

Requirements: .NET 9 SDK, Visual Studio 2022+. The paths (repository, game, Steam Workshop) are in
`ModCreator\Resources\embeded-settings.json`.
