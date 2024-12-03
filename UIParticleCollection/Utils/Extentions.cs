using System.Collections.Generic;
using UnityEngine;

namespace OSK
{
    public static class Extentions
    {
        public static RectTransform GetRectTransform(this Transform transform)
        {
            return transform as RectTransform;
        }

        public static List<Vector3> AddFirstList(this List<Vector3> list, Vector3 obj)
        {
            list.Insert(0, obj);
            return list;
        }

        public static List<Vector3> AddLastList(this List<Vector3> list, Vector3 obj)
        {
            list.Add(obj);
            return list;
        }
        
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
    }

    [System.Serializable]
    public class MinMaxFloat
    {
        public float min;
        public float max;

        public float RandomValue => Random.Range(min, max);
        public float Average => (min + max) / 2;

        public MinMaxFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, min, max);
        }
    }

    [System.Serializable]
    public class MinMaxInt
    {
        public int min;
        public int max;

        public int RandomValue => Random.Range(min, max);

        public MinMaxInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public float Clamp(int value)
        {
            return Mathf.Clamp(value, min, max);
        }
    }
}
