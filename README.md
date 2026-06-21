<div align="center">

# Paint Blocks

*A mobile puzzle game about placing blocks, mixing colors, and making strategic decisions on an 8x8 board.*

![Unity](https://img.shields.io/badge/Unity-2D-black?style=flat-square&logo=unity)
![C%23](https://img.shields.io/badge/C%23-Gameplay%20Programming-68217A?style=flat-square&logo=csharp)
![Platform](https://img.shields.io/badge/Platform-Android%20Mobile-3DDC84?style=flat-square&logo=android)
![Status](https://img.shields.io/badge/Status-In%20Development-EF9F27?style=flat-square)

</div>

## About

**Paint Blocks** is a 2D mobile puzzle game developed with Unity and C#.

The game is inspired by classic block-placement puzzle games, but it adds a new color-mixing mechanic. Instead of only placing blocks to clear rows and columns, players also need to think about how colors interact with each other on the board.

This project was created as a portfolio project to practice Unity gameplay programming, mobile UI interaction, puzzle logic, and game system design.

## Main Idea

The main new point of this game is the **color-mixing system**.

Players place blocks with primary colors:

- Red
- Yellow
- Blue

When two different primary colors are placed next to each other, they can mix into a secondary color:

- Red + Yellow = Orange
- Yellow + Blue = Green
- Blue + Red = Purple

However, placing the wrong third color next to a secondary color can create an **Ash** block. This makes the gameplay more strategic because players need to consider both block shape and color placement.

## Features

- 8x8 puzzle board
- Drag-and-drop block placement
- Mobile touch support
- Color mixing mechanic
- Ash block penalty mechanic
- Row and column clearing
- Target color objective
- Energy system
- 3x3 bomb skill
- Placement preview
- Score system
- Main menu
- Pause menu
- Settings panel
- Sound effects and background music

## Gameplay

Players drag blocks from the tray and place them onto the board.

The goal is to clear rows or columns, create useful color combinations, gain score, and use the bomb skill when the energy bar is full.

Unlike a normal block puzzle game, **Paint Blocks** rewards careful placement instead of only filling empty spaces.

## Technology

- Unity 2D
- C#
- Unity UI
- TextMeshPro
- Android mobile input
- Git / GitHub

## Screenshots / Demo

Add gameplay screenshots or GIFs here.

```md
![Gameplay Screenshot](Assets/Docs/gameplay.png)
```

## How to Run

Clone the repository:

```bash
git clone https://github.com/your-username/your-repository-name.git
```

Open the project in Unity.

Open the main scene:

```text
Assets/Project/Scenes/MainMenu.unity
```

Make sure these scenes are added to Build Profiles / Build Settings:

```text
MainMenu
Game
```

Press Play to test the game.

## Status

The core gameplay is playable.

Implemented systems include block placement, color mixing, clearing, scoring, energy, bomb skill, menu UI, pause UI, audio settings, and mobile input support.

Future improvements may include better visual effects, animations, sound polish, more block types, and improved game balancing.

## Author

**Nông Gia Khánh**

Unity / C# Developer  
Portfolio project for internship application and learning purposes.

<div align="center">

Thank you for checking out this project.

</div>
