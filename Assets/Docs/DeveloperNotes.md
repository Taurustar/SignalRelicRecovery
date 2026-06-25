# Signal Relic Recovery — Developer Notes

## Biggest Technical Decision

**Focus-based navigation instead of free movement.**

The context document explicitly recommended guided/focus-based navigation for accessibility. We implemented a station-focus system where the player cycles through relic stations with Previous/Next and confirms with Select. This decision:

- Removes the need for camera control, collision, and precise 3D movement.
- Makes the game equally playable with keyboard, gamepad, mouse, touch, and (future) screen-reader/voice input.
- Ensures the audio path is not a "lesser" version of the game; listening becomes the primary skill.
- Simplifies WebGL deployment because input mapping is small and predictable.

## Architecture Notes

- Single scene (`MainScene`) keeps iteration fast and state management simple.
- `GameConfig` ScriptableObject centralizes tunable values so designers can adjust difficulty without code changes.
- `InputReader` decouples gameplay code from the Input System asset, making control remapping easier.
- `AnnouncementManager` drives both visual text and optional audio announcements through a single call site.
- `RelicStation` prefab combines a primitive shape, looping audio, label, and click handler so stations can be spawned and configured at runtime.

## Scaling into Integrity Discarded Kombat

This prototype is designed as a slice of a larger mobile action game. In the **Integrity Discarded Kombat** universe, this system becomes an accessibility-first training mission:

- **Narrative framing:** The player is calibrating their audio-relic detection gear before combat. Each round is a drill: identify the correct relic signature from a field of similar signals.
- **Mobile adaptation:** Swipe left/right to cycle focus, double-tap to confirm, haptics for focus changes and selection feedback. Screen-reader APIs announce objectives.
- **Progression:** Successful drills unlock harder signature libraries (compound conditions, shorter audio bursts, more stations).
- **Combat tie-in:** The same listening skills transfer to locating enemies or hazards by sound during action sequences.

## Scaling Roadmap

### Short Term
- Replace generated audio with authored sound packs.
- Add haptic feedback for mobile.
- Improve station materials and environment art.

### Medium Term
- Compound target conditions (e.g., "buzzing AND beeping").
- Memory rounds where sounds stop after a short preview.
- Multiple training environments.

### Long Term
- Mobile release with swipe navigation and native accessibility APIs.
- Cloud leaderboard and progression save.
- Full integration with Integrity Discarded Kombat as a pre-mission accessibility training module.
