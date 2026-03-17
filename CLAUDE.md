# SPACE NINJA: TACTICS — Claude Instructions

## Git
**Do not run any git commands.** The user handles all commits, pushes, and branching.

---

## Project Overview
2D turn-based tactics game built in **Godot 4.6.1 mono** with **C# (.NET 8)**.
Namespace: `NinjaCowboy`. Design resolution: **1920×1080** (scales down to 1024×768 via stretch mode).

## Key Paths
- Godot exe: `D:\Programs\Godot_v4.6.1-stable_mono_win64\Godot_v4.6.1-stable_mono_win64.exe`
- Solution: `Ninja Cowboy.sln` / `Ninja Cowboy.csproj`
- Build: `dotnet build "Ninja Cowboy.csproj"`

## Architecture

### Autoloads (singletons)
| Node | Script | Purpose |
|---|---|---|
| `SpriteLoader` | `scripts/Core/SpriteLoader.cs` | Generates all solid-colour placeholder textures at startup. Access via `SpriteLoader.Instance.Get(SpriteType.X)` |
| `GameManager` | `scripts/Core/GameManager.cs` | Scene transitions, stores `BoardingTiles[]` between scenes |

### Scene flow
```
MainMenu → BoardingSelect → BattleScene → VictoryScreen / GameOverScreen
                                  ↑ (restart loops back to BoardingSelect)
```

### Key scripts
| Script | Notes |
|---|---|
| `scripts/Core/GridManager.cs` | 30×15 grid, `TileSize=64`, `GridOrigin=(0,60)`. A* pathfinding, Bresenham LOS. Battle-scene child node (not autoload). |
| `scripts/Core/TurnManager.cs` | Battle-scene child. Tracks `TurnPhase` and `TurnNumber`. |
| `scripts/Units/Unit.cs` | Base `Node2D`. Call `Initialize(stats, cell, grid)` **before** `AddChild`. |
| `scripts/Units/Ninja.cs` | Extends Unit. Adds `Shuriken`, `IsAmbush`, `IsConceal`, `HasActedThisTurn`. |
| `scripts/Units/Cowboy.cs` | Extends Unit. Adds `AIState`, `LastKnownNinjaPos`. AI logic in `BattleScene`. |
| `scripts/Data/UnitStats.cs` | `[GlobalClass]` Resource with `[Export]` fields. Use `UnitStats.DefaultNinja()` / `DefaultCowboy()`. |
| `scripts/Data/Missions/Mission01.cs` | Static helpers: `BuildWallMap()`, `BuildDoorMap()`, `GetVaccineRoomCells()`. Add `Mission02.cs` etc. to extend. |
| `scripts/Combat/CombatSystem.cs` | Static hit/damage roll helpers. Extend with new weapon methods. |
| `scripts/BattleScene.cs` | Also defines `CursorNode` (same file). All AI logic lives here. |

### UI pattern
All UI is built in C# `_Ready()` — scenes are minimal (root node + script only).
Dialogues are `CanvasLayer` children of `BattleScene` shown/hidden on demand.
`CursorNode` lives in a `CanvasLayer` (Layer=5) inside BattleScene so it renders above units.

### Rendering / scaling
- Tiles drawn via `_Draw()` / `DrawTextureRect` on the `BattleScene` Node2D (ZIndex 0).
- Units: ZIndex 3. Vaccine on ground: ZIndex 2. Vaccine on dead ninja: ZIndex 4.
- `project.godot` uses `stretch/mode="canvas_items"` + `stretch/aspect="keep"` so the 1920×1080 canvas scales uniformly to any window ≥ 1024×576.

### Adding a new mission
1. Create `scripts/Data/Missions/MissionNN.cs` mirroring `Mission01.cs`.
2. Add a `MissionData` factory method.
3. In `GameManager`, route `CurrentMission` to the correct scene/data.

### Adding new weapons / abilities
- Add stats to `UnitStats.cs` with `[Export]`.
- Add a roll method to `CombatSystem.cs`.
- Add dialogue options in `BattleScene.HandleAttackClick` / `ShowNinjaActionDialogue`.
- Handle the chosen action string in `BattleScene.OnActionChosen`.
