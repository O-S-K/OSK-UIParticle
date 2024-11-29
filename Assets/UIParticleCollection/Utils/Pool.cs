using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool 
{
    private static Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();

    public static GameObject Spawn(string key, GameObject prefab)
    {
        if (!_pool.ContainsKey(key))
        {
            _pool.Add(key, new Queue<GameObject>());
        }

        if (_pool[key].Count > 0)
        {
            GameObject obj = _pool[key].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return GameObject.Instantiate(prefab);
        }
    }

    public static void Despawn(string key, GameObject obj)
    {
        obj.SetActive(false);
        _pool[key].Enqueue(obj);
    }
    
    public static void DespawnAll(string key)
    {
        if (_pool.ContainsKey(key))
        {
            foreach (var obj in _pool[key])
            {
                obj.SetActive(false);
            }
        }
    }
}
