#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OSK
{
    [CustomEditor(typeof(UIParticle))]
    public class UIParticleEditor : Editor
    {
        private UIParticle _uiParticle;
        protected void OnEnable()
        {
            _uiParticle = (UIParticle)target;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Create ParticleSO"))
            {
                UIParticleSO asset = CreateInstance<UIParticleSO>();
                string path = "Assets/OSK/Resources/Configs/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "UIParticleSO.asset");
                AssetDatabase.CreateAsset(asset, assetPathAndName);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                _uiParticle.SetUIParticleSO(asset);
                EditorUtility.SetDirty(_uiParticle);
            }
            
            if (GUILayout.Button("Select Current Particle SO Setup"))
            {
                if (_uiParticle.UIParticleSO != null)
                {
                    Selection.activeObject = _uiParticle.UIParticleSO;
                }
                else
                {
                    Debug.LogWarning("No Particle SO setup assigned.");
                }
            }
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

    }
}
#endif