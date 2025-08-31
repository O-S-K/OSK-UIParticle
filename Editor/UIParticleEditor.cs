#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OSK
{
    [CustomEditor(typeof(UIParticle))]
    public class UIParticleEditor : Editor
    {
        private UIParticle _uiParticle;
        public string[] listParticleNames;
        protected void OnEnable()
        {
            _uiParticle = (UIParticle)target;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(20);
            
            // show list of particle names from UIParticleSO
            if (_uiParticle.UIParticleSO != null)
            {
                EditorGUILayout.LabelField("List of Particle Names in Assigned UIParticleSO:", EditorStyles.boldLabel);
                listParticleNames = _uiParticle.UIParticleSO.EffectSettings.ToList().Select(x => x.name).ToArray();
                if (listParticleNames.Length > 0)
                {
                    for (int i = 0; i < listParticleNames.Length; i++)
                    {
                        EditorGUILayout.LabelField((i + 1) + ". " + listParticleNames[i]);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No particles found in the assigned UIParticleSO.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No UIParticleSO assigned. Please create or assign one.", MessageType.Info);
            }
            
            GUILayout.Space(20);
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