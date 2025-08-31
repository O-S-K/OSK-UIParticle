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
- Add plugin Dotween on Asset Store.

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
    UIParticle.Instance.Spawn(new ParticleSetup()
    {
         typeSpawn = UIParticle.ETypeSpawn.UIToUI,
         name = "Coin",
         prefab = icon,
         from = point2D,
         to = targetUI,
         num = numCoin,
         onCompleted = () => { Debug.Log("Complete"); }
    }); 
}

public void SpawnUIToWorld()
{
    UIParticle.Instance.Spawn(new ParticleSetup()
    {
         typeSpawn = UIParticle.ETypeSpawn.UIToWorld,
         name = "Coin",
         prefab = icon,
         from = point2D,
         to = target3D,
         num = numCoin,
         onCompleted = () => { Debug.Log("Complete"); }
    });  
}

public void SpawnWorldToUI()
{
    UIParticle.Instance.Spawn(new ParticleSetup()
    {
         typeSpawn = UIParticle.ETypeSpawn.WorldToUI,
         name = "Coin",
         prefab = icon,
         from = point3D,
         to = target3D,
         num = numCoin,
         onCompleted = () => { Debug.Log("Complete"); }
    }); 
}

public void SpawnWorldToWorld()
{
    UIParticle.Instance.Spawn(new ParticleSetup()
    {
         typeSpawn = UIParticle.ETypeSpawn.WorldToWorld,
         name = "Coin",
         prefab = icon,
         from = point3D,
         to = target3D,
         num = numCoin,
         onCompleted = () => { Debug.Log("Complete"); }
    });  
}

public void SpawnWorldToWorld3D()
{
    UIParticle.Instance.Spawn(new ParticleSetup()
    {
         typeSpawn = UIParticle.ETypeSpawn.WorldToWorld3D,
         name = "Coin3D",
         prefab = icon,
         from = point3D,
         to = target3D,
         num = numCoin,
         onCompleted = () => { Debug.Log("Complete"); }
    });   
}

public void DestroyAll()
{
    UIParticle.Instance.DestroyEffect("Coin");
    //UIParticle.Instance.DestroyAllEffects();
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
