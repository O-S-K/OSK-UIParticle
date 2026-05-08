using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace OSK
{
    public class UIParticle : MonoBehaviour
    {
        public static UIParticle Instance { get; private set; }
        
        private readonly Dictionary<string, CancellationTokenSource> _activeTasks = new();
        [ShowInInspector, ReadOnly]
        private Dictionary<string, RectTransform> _groupLookup = new();
        private List<GameObject> _parentEffects = new List<GameObject>();

        [ShowInInspector, ReadOnly]
        private Dictionary<string, EffectSetting> _settingsLookup = new();
        [ShowInInspector, ReadOnly]
        private List<EffectSetting> _effectSettings = new List<EffectSetting>();
        
        public UIParticleSO UIParticleSO => _UIParticleSO;
        [Required, SerializeField] private UIParticleSO _UIParticleSO;


        [SerializeField, Required]
        private Transform _canvasTransform;

        [ShowInInspector, ReadOnly]
        private Camera _mainCamera;
        public Camera MainCamera
        {
            get => _mainCamera;
            set => _mainCamera = value;
        }

        [SerializeField, Required]
        private Camera _uiCamera;
        public Camera UICamera
        {
            get => _uiCamera;
            set => _uiCamera = value;
        }

        public enum ETypeSpawn
        {
            UIToUI = 0,
            UIToWorld = 1,
            WorldToUI = 2,
            WorldToWorld = 3,
            WorldToWorld3D = 4,
        }

        private void Awake() => Initialize();

        private void Initialize()
        {
            Instance = this;
            
            if(_mainCamera == null) _mainCamera = Camera.main;

            // Ensure we have a canvas to draw on in the new system
            if (_canvasTransform == null && Main.UI != null)
            {
                _canvasTransform = Main.UI.RootUI.PopupContainer;
                _uiCamera = Main.UI.UICamera;
            }

            _effectSettings = _UIParticleSO.EffectSettings.ToList();
            if (_effectSettings.Count == 0)
                return;

            _parentEffects = new List<GameObject>();
            for (int i = 0; i < _effectSettings.Count; i++)
            {
                // Create a fixed group for each effect type as a UI RectTransform
                var groupName = $"[Group] {_effectSettings[i].name}";
                var group = new GameObject(groupName, typeof(RectTransform));
                
                if (_canvasTransform != null)
                    group.transform.SetParent(_canvasTransform);
                
                // Reset RectTransform to fill parent or stay at zero
                var rect = group.GetComponent<RectTransform>();
                rect.localPosition = Vector3.zero;
                rect.localScale = Vector3.one;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                group.layer = 5; // UI Layer

                // OPTIMIZATION: Sub-Canvas for render isolation
                var canvas = group.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 1000 + i;

                // Ensure the group itself doesn't block raycasts
                var canvasGroup = group.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;

                _parentEffects.Add(group);
                _groupLookup[_effectSettings[i].name] = rect;
                _settingsLookup[_effectSettings[i].name] = _effectSettings[i];

                if (_effectSettings[i].typeMove == TypeMove.Beziers ||
                    _effectSettings[i].typeMove == TypeMove.CatmullRom ||
                    _effectSettings[i].typeMove == TypeMove.Path)
                {
                    AddPaths(_effectSettings[i]);
                }
            }
        }

        /// <summary>
        /// Pre-warm the pool for a specific style and prefab
        /// </summary>
        public void Preload(string styleName, GameObject prefab, int count = 10)
        {
            if (!_settingsLookup.ContainsKey(styleName))
            {
                Debug.LogWarning($"UIParticle: Cannot preload style '{styleName}' because it's not in SO.");
                return;
            }
            Main.Pool.ExpandPool(styleName, prefab, count);
        }

        public void SetUIParticleSO(UIParticleSO uiParticleSO)
        {
            _UIParticleSO = uiParticleSO;
        }

        public UniTask<List<GameObject>> Spawn(ParticleSetup particleSetup)
        {
            return Spawn(particleSetup.typeSpawn, particleSetup.name, particleSetup.prefab, particleSetup.from,
                particleSetup.to, particleSetup.num,
                particleSetup.onCompleted);
        }

        public async UniTask<List<GameObject>> Spawn(ETypeSpawn typeSpawn, string name, GameObject effectPrefab,
            Transform from, Transform to,
            int num = -1, System.Action onCompleted = null)
        {
            if (!_settingsLookup.TryGetValue(name, out var effectSetting))
            {
                MyLogger.LogError($"Effect setting with name {name} not found.");
                return null;
            }

            // High-performance group lookup
            _groupLookup.TryGetValue(name, out var parent);
            
            var spawnedObjects = new List<GameObject>();
            string key = $"{name}_{from.GetInstanceID()}_{to.GetInstanceID()}";
            if (_activeTasks.ContainsKey(key))
            {
                _activeTasks[key].Cancel();
                _activeTasks.Remove(key);
            }

            var cts = new CancellationTokenSource();
            _activeTasks[key] = cts;

            await SpawnAsync(typeSpawn, name, effectPrefab, from, to, num, onCompleted, spawnedObjects, cts.Token);
            _activeTasks.Remove(key);

            return spawnedObjects;
        }

        private async UniTask SpawnAsync(ETypeSpawn typeSpawn, string name, GameObject prefab, Transform from,
            Transform to, int num, System.Action onCompleted, List<GameObject> spawnedObjects, CancellationToken token)
        {
            Vector3 fromPosition = from.position;
            Vector3 toPosition = to.position;
            bool is3D = false;

            switch (typeSpawn)
            {
                case ETypeSpawn.UIToWorld: toPosition = ConvertToUICameraSpace(to); break;
                case ETypeSpawn.WorldToUI: fromPosition = ConvertToUICameraSpace(from); break;
                case ETypeSpawn.WorldToWorld:
                    fromPosition = ConvertToUICameraSpace(from);
                    toPosition = ConvertToUICameraSpace(to);
                    break;
                case ETypeSpawn.WorldToWorld3D: is3D = true; break;
            }
            await IESpawnEffect(is3D, name, prefab, fromPosition, toPosition, num, onCompleted, spawnedObjects, token);
        }

        private async UniTask IESpawnEffect(bool is3D, string name, GameObject prefab, Vector3 from, Vector3 to, 
            int num, System.Action onCompleted, List<GameObject> spawnedObjects, CancellationToken token)
        {
            if (!_settingsLookup.TryGetValue(name, out var settingSource)) return;
            var effectSetting = settingSource.Clone();

            effectSetting.pointSpawn = from;
            effectSetting.pointTarget = to;
            if (num > 0)
                effectSetting.numberOfEffects = num;
            effectSetting.OnCompleted = onCompleted;

            if (!_groupLookup.TryGetValue(name, out var parent))
            {
                Debug.LogWarning($"UIParticle: Parent group for '{name}' not found!");
                return;
            }

            if (prefab == null)
            {
                Debug.LogError($"UIParticle: No prefab found for spawn call '{name}'!");
                return;
            }

            for (int i = 0; i < effectSetting.numberOfEffects; i++)
            {
                token.ThrowIfCancellationRequested();

                // SMART POOLING: Use the effect name as the group key. 
                // PoolManager will remember 'parent' as the DefaultParent for this specific effect type.
                var effect = Main.Pool.Spawn(effectSetting.name, prefab, parent);
                if (effect == null) continue;

                // OPTIMIZATION: Disable raycast targeting for particles
                var graphics = effect.GetComponentsInChildren<UnityEngine.UI.Graphic>();
                for (int j = 0; j < graphics.Length; j++)
                {
                    graphics[j].raycastTarget = false;
                }

                if (!is3D)
                    effect.transform.localScale = Vector3.one;

                effect.transform.position = effectSetting.pointSpawn;
                spawnedObjects.Add(effect);

                if (effectSetting.isDrop)
                    DoDropEffect(effect, effectSetting);
                else
                    DoMoveTarget(effect, effectSetting);
            }

            float totalTimeOnCompleted = effectSetting.timeDrop.TimeAverage +
                                         effectSetting.delayDrop.TimeAverage +
                                         effectSetting.timeMove.TimeAverage +
                                         effectSetting.delayMove.TimeAverage;

            try
            {
                await UniTask.Delay((int)((totalTimeOnCompleted - 0.1f) * 1000), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            effectSetting.OnCompleted?.Invoke();
        }

        private void DoDropEffect(GameObject effect, EffectSetting effectSetting)
        {
            float timeDrop = effectSetting.timeDrop.RandomValue;
            Tween tween = effect.transform.DOMove(effectSetting.pointSpawn +
                                                  Random.insideUnitSphere * effectSetting.sphereRadius, timeDrop)
                .SetDelay(effectSetting.delayDrop.RandomValue);

            if (effectSetting.isScaleDrop)
            {
                effect.transform.localScale = effectSetting.scaleDropStart * Vector3.one;
                effect.transform.DOScale(effectSetting.scaleDropEnd, timeDrop)
                    .SetDelay(effectSetting.delayDrop.RandomValue).SetEase(effectSetting.easeDrop);
            }

            if (tween != null)
            {
                if (effectSetting.typeAnimationDrop == TypeAnimation.Ease)
                    tween.SetEase(effectSetting.easeDrop);
                else if (effectSetting.typeAnimationDrop == TypeAnimation.Curve)
                    tween.SetEase(effectSetting.curveDrop);
                else
                    tween.SetEase(Ease.Linear);

                tween.OnComplete(() =>
                {
                    effect.transform.DOKill();
                    DoMoveTarget(effect, effectSetting);
                });
            }
        }

        private void DoMoveTarget(GameObject effect, EffectSetting effectSetting)
        {
            Tween tween = null;
            var timeMove = effectSetting.timeMove.RandomValue;
            var timeMoveDelay = effectSetting.delayMove.RandomValue;

            if (effectSetting.isScaleMove)
            {
                effect.transform.localScale = effectSetting.scaleMoveStart * Vector3.one;
                effect.transform.DOScale(effectSetting.scaleMoveTarget, timeMove)
                    .SetDelay(timeMoveDelay).SetEase(effectSetting.easeMove);
            }

            switch (effectSetting.typeMove)
            {
                case TypeMove.Straight:
                    tween = effect.transform.DOMove(effectSetting.pointTarget, timeMove)
                        .SetDelay(timeMoveDelay);
                    break;
                case TypeMove.Beziers:
                    if (effectSetting.paths.Count % 3 != 0)
                    {
                        Debug.LogError("CubicBezier paths must contain waypoints in multiple of 3");
                        Main.Pool.Despawn(effect);
                        effectSetting.OnCompleted?.Invoke();
                        break;
                    }

                    tween = effect.transform
                        .DOPath(effectSetting.paths.Select(x => x).ToArray(), timeMove, PathType.CubicBezier)
                        .SetDelay(timeMoveDelay);
                    break;
                case TypeMove.CatmullRom:
                    if (effectSetting.paths.Count < 4)
                    {
                        Debug.LogError("CatmullRom paths must contain at least 4 waypoints");
                        Main.Pool.Despawn(effect);
                        effectSetting.OnCompleted?.Invoke();
                        break;
                    }

                    tween = effect.transform
                        .DOPath(effectSetting.paths.Select(x => x).ToArray(), timeMove, PathType.CatmullRom)
                        .SetDelay(timeMoveDelay);
                    break;
                case TypeMove.Path:
                    tween = effect.transform
                        .DOPath(effectSetting.paths.Select(x => x).ToArray(), timeMove, PathType.Linear)
                        .SetDelay(timeMoveDelay);
                    break;
                case TypeMove.DoJump:
                    tween = effect.transform
                        .DOJump(effectSetting.pointTarget, effectSetting.jumpPower.RandomValue,
                            effectSetting.jumpNumber, timeMove)
                        .SetDelay(timeMoveDelay);
                    break;
                case TypeMove.Around:
                    Vector3 controlPoint = effectSetting.pointSpawn +
                                           new Vector3(effectSetting.midPointOffsetX.RandomValue,
                                               effectSetting.height.RandomValue,
                                               effectSetting.midPointOffsetZ.RandomValue);
                    Vector3[] path = new Vector3[] { effectSetting.pointSpawn, controlPoint, effectSetting.pointTarget };
                    tween = effect.transform.DOPath(path, timeMove, PathType.CatmullRom)
                        .SetDelay(timeMoveDelay);
                    break;
                case TypeMove.Sin:
                    Vector3[] path1 = new Vector3[effectSetting.pointsCount];
                    for (int i = 0; i < effectSetting.pointsCount; i++)
                    {
                        float t = (float)i / (effectSetting.pointsCount - 1);
                        Vector3 point = Vector3.Lerp(effectSetting.pointSpawn, effectSetting.pointTarget, t);
                        point.y += Mathf.Sin(t * Mathf.PI * 2) * effectSetting.height.RandomValue;
                        path1[i] = point;
                    }
                    tween = effect.transform.DOPath(path1, timeMove, PathType.CatmullRom)
                        .SetDelay(timeMoveDelay);
                    break;
            }

            if (tween != null)
            {
                if (effectSetting.typeAnimationMove == TypeAnimation.Ease)
                    tween.SetEase(effectSetting.easeMove);
                else if (effectSetting.typeAnimationMove == TypeAnimation.Curve)
                    tween.SetEase(effectSetting.curveMove);
                else
                    tween.SetEase(Ease.Linear);

                tween.OnComplete(() =>
                {
                    effect.transform.DOKill();
                    Main.Pool.Despawn(effect);
                });
            }
        }

        private void AddPaths(EffectSetting effectSetting)
        {
            if (effectSetting.paths == null)
                effectSetting.paths = new List<Vector3>();
            if (!effectSetting.paths.Contains(effectSetting.pointSpawn))
                effectSetting.paths.AddFirstList(effectSetting.pointSpawn);
            if (!effectSetting.paths.Contains(effectSetting.pointTarget))
                effectSetting.paths.AddLastList(effectSetting.pointTarget);
        }

        public void DestroyEffect(string nameEffect)
        {
            if (Main.Pool.HasGroup(nameEffect))
                Main.Pool.DestroyAllInGroup(nameEffect);
        }

        public void DestroyAllEffects()
        {
            foreach (var setting in _effectSettings)
            {
                Main.Pool.DestroyAllInGroup(setting.name);
            }
        }

        private Vector3 ConvertToUICameraSpace(Transform pointTarget)
        {
            Vector3 uiWorldPosition;
            if (Main.UI.Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                if (pointTarget is RectTransform rectTarget)
                {
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        rectTarget, rectTarget.position, _uiCamera, out uiWorldPosition);
                }
                else uiWorldPosition = _mainCamera.WorldToScreenPoint(pointTarget.position);
            }
            else
            {
                Vector3 screenPoint = _mainCamera.WorldToScreenPoint(pointTarget.position);
                if (screenPoint.z < 0)
                {
                    Debug.LogWarning("Object is behind the main camera, unable to convert position.");
                    return Vector3.zero;
                }

                uiWorldPosition = _uiCamera.ScreenToWorldPoint(screenPoint);
                uiWorldPosition.z = 0;
            }
            return uiWorldPosition;
        }
    }
}
