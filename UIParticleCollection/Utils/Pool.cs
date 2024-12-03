using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSK
{
    public class Pool
    {
        private static Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();
 
        
        public static GameObject Spawn(string key, GameObject prefab)
        {
            if (!_pool.ContainsKey(key))
            {
                _pool.Add(key, new Queue<GameObject>());
            }

            GameObject obj = null;
            if (_pool[key].Count > 0)
            {
                obj = _pool[key].Dequeue();
                obj.SetActive(true);
            }
            else
            {
                obj = GameObject.Instantiate(prefab);
                obj.name = key;
            }

            return obj;
        } 

        public static void Despawn(GameObject gameObject)
        {
            gameObject.SetActive(false);
            _pool[gameObject.name].Enqueue(gameObject);
        } 
    }
}