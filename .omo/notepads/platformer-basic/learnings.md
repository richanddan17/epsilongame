# Platformer Basic - Learnings

## 2D Physics Settings (Camera Jitter Prevention)

Applied settings to prevent camera jitter:

- **Gravity Y = -20** (faster than default -9.81 for snappier platforming)
- **Physics2D.defaultContactOffset = 0.01** (tighter collision detection)
- **Physics2D.autoSyncTransforms = true** (auto-sync transforms to prevent visual stutter)

### Notes
- \Physics2D.autoSyncTransforms\ is marked obsolete in newer Unity versions; use \Physics2D.SyncTransforms()\ manually if needed
- Later: Player Rigidbody2D should have \Interpolation = Interpolate\ for smooth camera follow
- 3D Physics settings (Physics.gravity) also updated to match for consistency

## Basic Scene Structure

Created `Assets/Scenes/Main.unity` with minimal setup:
- **Main Camera** (Transform, Camera, AudioListener) - kept from default template
- **Directional Light** (Transform, Light with `m_Type = Directional`) - added manually since default template didn't include it
- Light rotation set to `[50, -30, 0]` for typical downward angle

### Notes
- Default template only created Main Camera, no Directional Light
- Light type must be set via serialized field `m_Type` (Enum: Directional)
- Scene saved and verified clean hierarchy
