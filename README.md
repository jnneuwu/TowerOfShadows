# Tower of Shadows

A pixel-art top-down dungeon shooter built in Unity. Inspired by **Soul Knight**, **Hades**, and **CS:GO**.

![Unity](https://img.shields.io/badge/Unity-6000.0%2B-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![Status](https://img.shields.io/badge/Status-Final%20Submission-success)

## Features

- 7 distinct weapons (pistol, machine gun, shotgun, sniper, flame, frost, laser)
- 3 hand-designed floors with unique room layouts
- 3 multi-phase boss fights
- CS:GO-style weapon drop and pick-up (G key)
- Procedurally-generated pixel art for every sprite
- Procedural and royalty-free audio with M/N toggle
- Dynamic 2D lighting (sprite-overlay system, no URP required)
- 6 different particle effects (muzzle flash, death burst, explosion, boss attacks, etc.)
- Player state machine with on-screen debug panel
- Checkpoint-style respawn on the current floor

## Controls

| Key | Action |
|---|---|
| WASD | Move |
| Mouse | Aim |
| Left click | Shoot |
| E | Open chest |
| G | Drop / pick up weapon |
| ESC | Pause / resume |
| M | Toggle music |
| N | Toggle sound effects |

## How to run

1. Open Unity Hub
2. Add the `TowerOfShadows` folder as a project
3. Open with Unity 6 (6000.0+)
4. First-time open takes 1-2 minutes (Library rebuild)
5. Open `Assets/Scenes/MainMenu.unity` and press Play

## Code structure

```
Assets/Scripts/
├── Core/          GameManager, AudioManager, CameraFollow
├── Player/        PlayerController, PlayerStateMachine, WeaponData, WeaponPickup, Bullet
├── Enemies/       EnemyBase, ShadowWalker, ShadowCharger, ShadowCaster, CasterOrb, BossController
├── Environment/   WeaponChest, ExplosiveCrate, Portal, HealthPickup, RoomManager, RoomDoor
├── Effects/       SimpleLight, TorchLight, PulsingLight, ParticleFx, FxAssetHolder
├── UI/            UIManager, MainMenuManager, MenuActions
└── Editor/        PrototypeAssets, PrototypeBuilder, PrototypeData
```

Approximately 32 C# scripts, 4,700 lines of code.

## Lab progression

| Lab | Feature added |
|---|---|
| 1-2 | Player movement, camera, basic shooting |
| 3 | Three enemy AI types + status effects |
| 4 | UI suite + player state machine |
| 5 | Audio system (BGM + 8 SFX + toggle keys) |
| 6 | Lighting overlay + particle effects + weapon drop/pickup |

## Credits

- **Music:** *Symmetry* by Kevin MacLeod (incompetech.com) - CC BY 4.0
- **Sound effects:** procedural sine-wave synthesis (in code) + one "boom" meme clip for explosions
- **Sprites:** procedurally generated in `PrototypeAssets.cs`
- **Inspiration:** Soul Knight (ChillyRoom), Hades (Supergiant Games), CS:GO (Valve)

## License

This is an academic project submission. Educational use only.

---

*Submission for Games Development module - Y4 BSCH Computer Science*  
*Mingyu Yang - 2026*