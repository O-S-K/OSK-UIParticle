using UnityEngine;

namespace OSK
{
    public class ParticleSetup
    {
        public UIParticle.ETypeSpawn typeSpawn;
        public string name;
        public GameObject prefab;
        public Transform from = null;
        public Transform to = null;
        public int num = -1;
        public System.Action onCompleted = null;
    }
}