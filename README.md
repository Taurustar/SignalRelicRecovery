# Signal Relic Recovery

A compact Unity WebGL prototype for Mental Vision Games. The goal is to demonstrate accessibility-first design: the game is fully playable without sight, while remaining intuitive and engaging for sighted players.

## Core Idea

You are in a small training area with several relic stations. Each station emits a unique sound and has a distinct shape/label. Every round you receive a target condition (for example, "Find the buzzing station"). Use focus-based navigation to listen, identify, and select the correct station. Difficulty increases across three rounds:
- Round 1: 3 stations, target announced by name and sound.
- Round 2: 4 stations, target announced by sound only.
- Round 3: 5 stations, sounds play intermittently, testing memory and timing.

## Accessibility Approach

- **No free movement required.** Navigation is focus-based: cycle through stations with Previous/Next and confirm with Select. This removes the need for precise spatial movement.
- **No color-only cues.** Stations differ by sound, shape, and visible label/pattern.
- **Full audio information.** Target conditions, focused station names, and feedback are announced through both on-screen text and audio.
- **Accessibility Mode.** Toggle in the main menu to enable longer announcements and larger text.
- **Multiple input methods.** Keyboard, mouse, touch, and gamepad are all supported through the Unity Input System.

## Controls

| Action | Keyboard | Gamepad | Mouse / Touch |
|--------|----------|---------|---------------|
| Previous station | Left Arrow, 1 | D-Pad Left | Click station |
| Next station | Right Arrow, 2 | D-Pad Right | Click station |
| Select | Space, Enter, E | South Button (A) | Click station |
| Back / Cancel | Escape | East Button (B) | Back button |

## How to Run

1. Open the project in **Unity 6000.x** (URP 3D core template).
2. Open `Assets/Scenes/MainScene.unity`.
3. Press Play in the Editor.
4. To build for WebGL:
   - `File > Build Settings`
   - Select `WebGL`
   - `Build And Run` or `Build`

The project is already configured for WebGL via `Signal Relic Recovery > Configure WebGL Build`.

## Project Structure

```
Assets/
  Scripts/              Gameplay logic split into focused scripts
  ScriptableObjects/    GameConfig tunable values
  Audio/                Generated station loops and SFX
  Materials/            Patterned materials for station visuals
  Prefabs/              RelicStation prefab
  Scenes/
    MainScene.unity     Single scene containing menu, gameplay, and results
```

## Key Scripts

- `GameManager.cs` — game flow, rounds, scoring, timer.
- `StationFocusManager.cs` — focus-based navigation and selection.
- `RelicStation.cs` — per-station identity, sound, and feedback.
- `AudioManager.cs` — one-shot SFX.
- `AnnouncementManager.cs` — on-screen text and optional voiced announcements.
- `UIManager.cs` — menu, HUD, results, accessibility toggle.
- `GameplayEventLogger.cs` — records gameplay events for the debug panel.
- `EventLogUI.cs` — displays the event log in the HUD.
- `InputReader.cs` — thin wrapper over the Input System asset.
- `GameConfig.cs` — ScriptableObject for tunable values.

## What Was Intentionally Left Unfinished

- Native OS screen-reader integration (WebGL limitations). The game relies on large dynamic text and audio announcements instead.
- Advanced spatial audio mixing. Current spatial blend is functional but could be enhanced with reverb zones.
- Procedural texture generation is basic. A real project would use authored textures.
- No persistent save data or leaderboards.
- Mobile-specific UI polish (e.g., swipe gestures) is minimal.

## Biggest Technical Decision

**Focus-based navigation instead of free movement.**

Free movement would have required camera controls, collision, and precise spatial reasoning, all of which create friction for players relying on audio. Focus-based navigation lets the player concentrate on listening and matching sounds, making the accessibility path equal to (and in some ways faster than) the sighted path. It also simplifies WebGL input handling because keyboard, gamepad, mouse, and touch all map to the same three actions: Previous, Next, Select.

## Scaling to Integrity Discarded Kombat

This prototype is a slice of a larger mobile action game. In the **Integrity Discarded Kombat** universe, it becomes an accessibility-first training mission where the player calibrates audio-relic detection gear before combat:

- **Narrative framing:** Each round is a drill to identify the correct relic signature from a field of similar signals.
- **Mobile adaptation:** Swipe left/right to cycle focus, double-tap to confirm, haptics for focus changes and selection feedback. Screen-reader APIs announce objectives.
- **Progression:** Successful drills unlock harder signature libraries (compound conditions, shorter audio bursts, more stations).
- **Combat tie-in:** The same listening skills transfer to locating enemies or hazards by sound during action sequences.
- `GameConfig` makes it easy to add more rounds, stations, and condition chains.
- The `AnnouncementManager` pattern can drive native mobile screen readers or in-engine TTS.
- New station types, hazards, timers, and scoring modifiers can be added without rewriting the core flow.

## Credits

- Unity Technologies — Unity Engine, URP, Input System.
- Audio — AI-generated sound effects for prototype use.
