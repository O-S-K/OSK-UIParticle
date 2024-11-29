# UI Particles Collections

Enhance your game's visual feedback with smooth, customizable collection animations.

Link Yotube: https://youtu.be/miw8NwCDcBE

---

## Key Features

- **Collectible Animations**:
  - Animate items (coins, gems, etc.) moving from the game world (or world canvas) to UI components.
  - Visualize item purchases from the store UI to resource bars.
  - Add excitement to resource/energy spending with engaging animations.

- **Customizable Effects**:
  - Fully customizable collection effects for added visual impact.

- **2D and 3D Support**:
  - Works seamlessly in both 2D and 3D environments.

---

## How to Use

### 1. Test the Demo
- Check out the **Demo Folder** and play the provided **Scene** to see it in action.

### 2. Add the Script to Your Scene
1. Drag the **UIParticle** script into your Scene.
2. Create a configuration:
   - **Menu**: `Create -> OSK -> UIParticleConfig`.
   - Adjust the configuration settings to match your needs.

---

## API Reference

### 3. Playable Functions

Use the following functions to play the effects:

```csharp
public void SpawnUIToUI()
{
    UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToUI, "Coin", point2D, target);
}

public void SpawnUIToWorld()
{
    UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToWorld, "Coin", point2D, target3D);
}

public void SpawnWorldToUI()
{
    UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToUI, "Coin", point3D, target);
}

public void SpawnWorldToWorld()
{
    UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToWorld, "Coin", point3D, target3D);
}

public void SpawnWorldToWorld3D()
{
    UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToWorld3D, "Coin2", point3D, target3D);
}

public void DestroyAll()
{
    UIParticle.Instance.DestroyParticle("Coin");
}
```  
## Parameters

### `Spawn` Method

| Parameter     | Type                      | Description                                                         |
|---------------|---------------------------|---------------------------------------------------------------------|
| `type`        | `UIParticle.ETypeSpawn`   | The effect type (UIToUI, UIToWorld, WorldToUI, WorldToWorld, etc.). |
| `particleKey` | `string`                  | The name of the particle effect (e.g., "Coin", "Gem").              |
| `startPoint`  | `Vector2` or `Vector3`    | The starting point of the effect (depends on 2D/3D type) .          |
| `target`      | `Transform`               | The destination target for the effect.                              |

---

## Support

If you have any questions or issues, please create an **Issue** in this repository or contact the development team.

Link FB: https://www.facebook.com/xOskx/

Thank you for using **UI Particles Collections**! 🎉
