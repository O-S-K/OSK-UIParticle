using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.Collections;
using Random = UnityEngine.Random;

namespace OSK
{
    public class UIParticle : MonoBehaviour
    {
        public static UIParticle Instance;
        public UIParticleSO UIParticleSO;
        public Canvas canvas;

        private EffectSetting[] _effectSettings;
        private List<GameObject> _parentEffects;
        private Dictionary<string, Coroutine> _activeCoroutines = new Dictionary<string, Coroutine>();

        public enum ETypeSpawn
        {
            UIToUI,
            UIToWorld,
            WorldToUI,
            WorldToWorld,
            WorldToWorld3D,
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        { 
            _effectSettings = UIParticleSO.EffectSettings;
            if (_effectSettings.Length == 0)
                return;

            _parentEffects = new List<GameObject>();
            for (int i = 0; i < _effectSettings.Length; i++)
            {
                _parentEffects.Add(new GameObject(_effectSettings[i].name));
                // _parentEffects[i].gameObject.GetOrAdd<RectTransform>();
                // _parentEffects[i].SetLayer("UI");
                _parentEffects[i].transform.SetParent(canvas.transform);
                _parentEffects[i].transform.localScale = Vector3.one;
                AddPaths(_effectSettings[i]);
            }
        }

        public void Spawn(ETypeSpawn typeSpawn, string name, Transform from, Transform to, int num = -1,
            System.Action onCompleted = null)
        {
            string key = $"{name}_{from.GetInstanceID()}_{to.GetInstanceID()}";
            if (_activeCoroutines.ContainsKey(key))
            {
                StopCoroutine(_activeCoroutines[key]);
                _activeCoroutines.Remove(key);
            }

            Coroutine coroutine = StartCoroutine(SpawnCoroutine(typeSpawn, name, from, to, num, onCompleted));
            _activeCoroutines[key] = coroutine;
        }

        private IEnumerator SpawnCoroutine(ETypeSpawn typeSpawn, string name, Transform from, Transform to, int num,
            System.Action onCompleted)
        {
            Vector3 fromPosition = from.position;
            Vector3 toPosition = to.position;
            bool is3D = false;

            switch (typeSpawn)
            {
                case ETypeSpawn.UIToWorld:
                    toPosition = ConvertToUICameraSpace(to);
                    break;
                case ETypeSpawn.WorldToUI:
                    fromPosition = ConvertToUICameraSpace(from);
                    break;
                case ETypeSpawn.WorldToWorld:
                    fromPosition = ConvertToUICameraSpace(from);
                    toPosition = ConvertToUICameraSpace(to);
                    break;
                case ETypeSpawn.WorldToWorld3D:
                    is3D = true;
                    break;
            }

            SpawnEffect(is3D, name, fromPosition, toPosition, num, onCompleted);
            yield return null;
        }

        private void SpawnEffect(bool is3D, string nameEffect, Vector3 pointSpawn, Vector3 pointTarget,
            int numberOfEffects,
            System.Action onCompleted)
        {
            var effectSetting = _effectSettings.ToList().Find(x => x.name == nameEffect).Clone();
            effectSetting.pointSpawn = pointSpawn;
            effectSetting.pointTarget = pointTarget;

            if (numberOfEffects > 0)
                effectSetting.numberOfEffects = numberOfEffects;
            effectSetting.OnCompleted = onCompleted;

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(IESpawnEffect(is3D, effectSetting));
            }
        }

        private IEnumerator IESpawnEffect(bool is3D, EffectSetting effectSetting)
        {
            var parent = _parentEffects.Find(x => x.name == effectSetting.name)?.transform;
            if (parent == null || !parent.gameObject.activeInHierarchy)
                yield break;

            for (int i = 0; i < effectSetting.numberOfEffects; i++)
            {
                var effect = Pool.Spawn("UIParticle", effectSetting.icon);
                effect.transform.SetParent(parent);
                effect.transform.position = effectSetting.pointSpawn;

                if (!is3D)
                {
                    effect.transform.localScale = Vector3.one;
                    effect.gameObject.GetOrAdd<RectTransform>();
                }

                if (effectSetting.isDrop)
                {
                    DoDropEffect(effect, effectSetting);
                }
                else
                {
                    DoMoveTarget(effect, effectSetting);
                }
            }

            var timedrop = (effectSetting.timeDrop.max + effectSetting.timeDrop.min) / 2;
            var timeDropDelay = (effectSetting.delayDrop.max + effectSetting.delayDrop.min) / 2;
            var timeMove = (effectSetting.timeMove.max + effectSetting.timeMove.min) / 2;
            var timeMoveDelay = (effectSetting.delayMove.max + effectSetting.delayMove.min) / 2;

            var totalTimeOnCompleted = timedrop + timeDropDelay + timeMove + timeMoveDelay;
            yield return new WaitForSeconds(totalTimeOnCompleted - 0.1f);
            effectSetting.OnCompleted?.Invoke();
        }

        private void DoDropEffect(GameObject effect, EffectSetting effectSetting)
        {
            Vector3 randomOffset = Random.insideUnitSphere * effectSetting.sphereRadius;
            Vector3 target = effectSetting.pointSpawn + randomOffset;
            var timeDrop = effectSetting.timeDrop.RandomValue;
            var timeDropDelay = effectSetting.delayDrop.RandomValue;

            Tween tween = effect.transform
                .DOMove(target, timeDrop)
                .SetDelay(timeDropDelay);

            if (tween != null)
            {
                if (effectSetting.typeAnimationDrop == TypeAnimation.Ease)
                {
                    tween.SetEase(effectSetting.easeDrop);
                }
                else if (effectSetting.typeAnimationDrop == TypeAnimation.Curve)
                {
                    tween.SetEase(effectSetting.curveDrop);
                }
                else
                {
                    tween.SetEase(Ease.Linear);
                }

                tween.OnComplete(() => { DoMoveTarget(effect, effectSetting); });
            }
        }

        private void DoMoveTarget(GameObject effect, EffectSetting effectSetting)
        {
            Tween tween = null;
            var timeMove = effectSetting.timeMove.RandomValue;
            var timeMoveDelay = effectSetting.delayMove.RandomValue;

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
                        Pool.Despawn("UIParticle", effect);
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
                        Pool.Despawn("UIParticle", effect);
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
                    // Calculate a control point to define the curvature at the spawn point
                    Vector3 controlPoint = effectSetting.pointSpawn +
                                           new Vector3(effectSetting.midPointOffsetX.RandomValue,
                                               effectSetting.height.RandomValue,
                                               effectSetting.midPointOffsetZ.RandomValue);

                    // Create the path
                    Vector3[] path = new Vector3[]
                        { effectSetting.pointSpawn, controlPoint, effectSetting.pointTarget };
                    tween = effect.transform.DOPath(path, timeMove, PathType.CatmullRom)
                        .SetDelay(timeMoveDelay);
                    break;
                case TypeMove.Sin:
                    Vector3[] path1 = new Vector3[effectSetting.pointsCount];

                    for (int i = 0; i < effectSetting.pointsCount; i++)
                    {
                        float t = (float)i / (effectSetting.pointsCount - 1);
                        Vector3 point = Vector3.Lerp(effectSetting.pointSpawn, effectSetting.pointTarget, t);
                        point.y += Mathf.Sin(t * Mathf.PI * 2) *
                                   effectSetting.height.RandomValue; // Apply sine wave offset
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

                // effect.transform.DOScale(effectSetting.scaleTarget, timeMove)
                //     .SetDelay(timeMoveDelay);
                tween.OnComplete(() => {  Pool.Despawn("UIParticle", effect); });
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


        public void DestroyParticle(string nameEffect)
        {
            var parent = _parentEffects.Find(x => x.name == nameEffect)?.transform;
            if (parent == null)
                return;
             Pool.DespawnAll(nameEffect);
        }
 
        public Vector3 ConvertToUICameraSpace(Transform pointTarget)
        {
            Vector3 uiWorldPosition;
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera is not found, unable to convert position.");
                return Vector3.zero;
            }

            Camera uiCamera = canvas.worldCamera; 
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(pointTarget.GetRectTransform(),
                    pointTarget.position, uiCamera, out uiWorldPosition);
            }
            else
            {
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(pointTarget.position);
                if (screenPoint.z < 0)
                {
                    Debug.LogWarning("Object is behind the main camera, unable to convert position.");
                    return Vector3.zero;
                }

                uiWorldPosition = uiCamera.ScreenToWorldPoint(screenPoint);
                uiWorldPosition.z = 0;
            }

            return uiWorldPosition;
        }
    }
}