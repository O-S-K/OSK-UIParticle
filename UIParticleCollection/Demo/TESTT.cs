using OSK;
using UnityEngine;

public class TESTT : MonoBehaviour
{
    public Transform point2D1;
    public Transform point2D2;

    public Transform point2D;
    public Transform point3D;

    public Transform target;
    public Transform target3D;

    public bool autoSpawn;
    public float timeAutoSpawn = 1f;
    private float currentTimeAutoSpawn;

    public GameObject go2d;
    public GameObject go3d;

    private void Update()
    {
        if (autoSpawn)
        {
            currentTimeAutoSpawn -= Time.deltaTime;
            if (currentTimeAutoSpawn <= 0)
            {
                currentTimeAutoSpawn = timeAutoSpawn;
                UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToUI, "Coin",go2d, point2D, target);
            }
        }
    }

    public void SpawnUIToUI()
    {
        var go = Random.Range(0, 2) == 0 ? point2D1 : point2D2;
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToUI, "Coin",go.gameObject, point2D, target);
    }

    public void SpawnUIToWorld()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.UIToWorld, "Coin",go2d, point2D, target3D);
    }

    public void SpawnWorldToUI()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToUI, "Coin",go3d, point3D, target);
    }

    public void SpawnWorldToWorld()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToWorld, "Coin",go3d, point3D, target3D);
    }

    public void SpawnWorldToWorld3D()
    {
        UIParticle.Instance.Spawn(UIParticle.ETypeSpawn.WorldToWorld3D, "Coin2",go3d, point3D, target3D);
    }
}