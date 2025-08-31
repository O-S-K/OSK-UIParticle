using System.Collections;
using System.Collections.Generic;
using OSK;
using Sirenix.OdinInspector;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ExampleUIParticle : MonoBehaviour
{
    public Transform from;
    public Transform to;
    public GameObject icon;
    public int numCoin = 20;
    
    [Button]
    private void TestUIParticleAsync()
    {
        TestUIParticle().Forget();
    }
    
    private async UniTaskVoid  TestUIParticle()
    {
        await UIParticle.Instance.Spawn(new ParticleSetup()
        {
            typeSpawn = UIParticle.ETypeSpawn.UIToUI,
            name = "Coin",
            prefab = icon,
            from = from,
            to = to,
            num = numCoin,
            onCompleted = () => { Debug.Log("Complete"); }
        });
    }
    
    private void DestroyUI()
    {
        UIParticle.Instance.DestroyEffect("Coin");
        //UIParticle.Instance.DestroyAllEffects();
    }
}
