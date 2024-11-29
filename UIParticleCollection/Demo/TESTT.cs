using OSK;
using UnityEngine;

public class TESTT : MonoBehaviour
{
    public Transform point2D;
    public Transform point3D;
    
    public Transform target;
    public Transform target3D;

    public bool autoSpawn;
    public float timeAutoSpawn = 1f;
    private float currentTimeAutoSpawn;

    private void Update()
    {
        if (autoSpawn)
        {
            currentTimeAutoSpawn -= Time.deltaTime;
            if (currentTimeAutoSpawn <= 0)
            {
                currentTimeAutoSpawn = timeAutoSpawn;
                UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToUI,"Coin", point2D, target);
            }
        }
    }
    
    public void SpawnUIToUI()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToUI,"Coin", point2D, target);
    }
    
    public void SpawnUIToWorld()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToWorld,"Coin", point2D, target3D);
    }
    
    public void SpawnWorldToUI()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToUI,"Coin", point3D, target);
    } 
    public void SpawnWorldToWorld()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToWorld,"Coin", point3D, target3D);
    }
    
    public void SpawnWorldToWorld3D()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToWorld3D,"Coin2", point3D, target3D);
    }
    
    public void DestroyAll()
    {
        UIParticle.Instance.DestroyParticle("Coin");
    }
     
}