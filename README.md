# Silksong AI Mod

A mod for **Hollow Knight: Silksong** that adds AI-controlled features. This project is intended for developers and modders who want to experiment with AI mods or contribute to the project.

---

## Features

- AI-driven gameplay behavior for bosses  
- Fully open for modification and experimentation  
- Works with **BepInEx mod loader**

---

## Getting Started

### Requirements

- **Hollow Knight: Silksong** installed  
- **BepInEx mod loader** installed in the game directory (can be found here https://github.com/BepInEx/BepInEx)
- **Visual Studio 2022** (or newer) for building the mod

---

### Setting the Game Path

Before building, you need to tell the project where your game is installed:

1. Copy `Local.props.example` → `Local.props` in the project folder.  
2. Edit `Local.props` and set the `GameDir` property to your game path.  

Example:

```xml
<Project>
  <PropertyGroup>
    <GameDir>C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight Silksong</GameDir>
  </PropertyGroup>
</Project>
```