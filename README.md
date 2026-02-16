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

1. In the `mod/SilksongAImod` directory, duplicate the `Directory.Build.props` file and rename it to `Directory.Build.props.user`.  
2. Edit `Directory.Build.props.user` and set the `GameDir` property to your game path.  
3. Restart visual studio for the changes to take affect.

Example of correct `Directory.Build.props.user` file:

```xml
<Project>
  <PropertyGroup>
    <GameDir>C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight Silksong</GameDir>
  </PropertyGroup>
</Project>
```

### Building the Mod

1. Open `SilksongAImod.sln` in Visual Studio.  
2. Choose your configuration: **Release** is recommended for gameplay.  
3. Build the solution (`Build → Build Solution`).  

---

### Deploying the Mod

You have two options:

#### 1. Deploy manually
Copy the `SilksongAImod.dll` file from `SilksongAImod\bin\Release` to `\BepInEx\plugins` folder inside your game instalation directory

#### 2. Use the DeployAndRun target to copy the DLL and launch the game automatically:
Use the `DeployAndRun` MSBuild target to copy the DLL to your game’s BepInEx/plugins folder and automatically run the game:

```powershell
msbuild SilksongAImod.csproj /t:DeployAndRun /p:Configuration=Release
```