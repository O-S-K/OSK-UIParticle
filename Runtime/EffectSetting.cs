using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine;

namespace OSK
{
    [System.Serializable]
    public class EffectSetting
    {
        [Header("Setup")]
        public string name;
        //public int id { get; set; }
        //public GameObject icon;
        [Min(1)]
        public int numberOfEffects = 10;
        public bool isDrop = true;

        [PropertyOrder(-1)]
        [BoxGroup("Quick Presets", CenterLabel = true)]
        [HorizontalGroup("Quick Presets/Buttons")]
        [Button(ButtonSizes.Medium, Name = "Coin Explosion")]
        public void ApplyCoinExplosion()
        {
            isDrop = true;
            sphereRadius = 2f;
            delayDrop = new MinMaxFloat(0, 0.2f);
            timeDrop = new MinMaxFloat(0.4f, 0.6f);
            typeAnimationDrop = TypeAnimation.Ease;
            easeDrop = Ease.OutBack;
            
            isScaleDrop = true;
            scaleDropStart = 0;
            scaleDropEnd = 1.2f;

            typeMove = TypeMove.Straight;
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.InBack;
            timeMove = new MinMaxFloat(0.6f, 0.8f);
            delayMove = new MinMaxFloat(0.1f, 0.3f);
            
            isScaleMove = true;
            scaleMoveStart = 1.2f;
            scaleMoveTarget = 0.5f;
        }

        [HorizontalGroup("Quick Presets/Buttons")]
        [Button(ButtonSizes.Medium, Name = "Fast Collect")]
        public void ApplyFastCollect()
        {
            isDrop = false;
            typeMove = TypeMove.Straight;
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.InQuad;
            timeMove = new MinMaxFloat(0.4f, 0.5f);
            delayMove = new MinMaxFloat(0, 0.1f);
            
            isScaleMove = true;
            scaleMoveStart = 1f;
            scaleMoveTarget = 0.8f;
        }

        [HorizontalGroup("Quick Presets/Buttons")]
        [Button(ButtonSizes.Medium, Name = "Smooth Bounce")]
        public void ApplySmoothBounce()
        {
            isDrop = true;
            sphereRadius = 0.5f;
            timeDrop = new MinMaxFloat(0.3f, 0.4f);
            easeDrop = Ease.OutQuad;

            typeMove = TypeMove.Around;
            height = new MinMaxFloat(2f, 4f);
            midPointOffsetX = new MinMaxFloat(-2f, 2f);
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.InOutSine;
            timeMove = new MinMaxFloat(0.8f, 1.2f);
        }

        public void ApplyPuzzleBurst()
        {
            isDrop = true;
            sphereRadius = 1.8f;
            timeDrop = new MinMaxFloat(0.4f, 0.5f);
            easeDrop = Ease.OutBack;

            typeMove = TypeMove.Around;
            height = new MinMaxFloat(50f, 100f);
            midPointOffsetX = new MinMaxFloat(-150f, 150f);
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.InBack;
            timeMove = new MinMaxFloat(0.6f, 0.8f);
            delayMove = new MinMaxFloat(0.2f, 0.4f);
            
            isScaleMove = true;
            scaleMoveStart = 1.2f;
            scaleMoveTarget = 0.6f;
        }

        public void ApplyPuzzleArc()
        {
            isDrop = false;
            typeMove = TypeMove.Around;
            height = new MinMaxFloat(200f, 300f);
            midPointOffsetX = new MinMaxFloat(-250f, 250f);
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.InOutQuart;
            timeMove = new MinMaxFloat(0.8f, 1.0f);
            delayMove = new MinMaxFloat(0f, 0.2f);
        }

        public void ApplyPuzzleMagnetic()
        {
            isDrop = true;
            sphereRadius = 0.4f;
            timeDrop = new MinMaxFloat(0.2f, 0.3f);
            easeDrop = Ease.OutSine;

            typeMove = TypeMove.Straight;
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.InQuart;
            timeMove = new MinMaxFloat(0.5f, 0.6f);
            delayMove = new MinMaxFloat(0.1f, 0.2f);
        }

        public void ApplyPuzzleSequential()
        {
            isDrop = false;
            typeMove = TypeMove.Sin;
            pointsCount = 10;
            height = new MinMaxFloat(30f, 60f);
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.InOutSine;
            timeMove = new MinMaxFloat(1.0f, 1.2f);
            delayMove = new MinMaxFloat(0.1f, 0.8f); // High delay spread
        }

        public void ApplyPuzzleElastic()
        {
            isDrop = true;
            sphereRadius = 1.0f;
            timeDrop = new MinMaxFloat(0.3f, 0.4f);
            easeDrop = Ease.OutQuad;

            typeMove = TypeMove.DoJump;
            jumpPower = new MinMaxFloat(100f, 150f);
            jumpNumber = 1;
            typeAnimationMove = TypeAnimation.Ease;
            easeMove = Ease.OutElastic;
            timeMove = new MinMaxFloat(1.2f, 1.5f);
        }

        [Space(10)]

        #region Drop

        [Header("Drop")]
        [ShowIf(nameof(isDrop), true)]
        public float sphereRadius = 1;

        [ShowIf(nameof(isDrop), true)]
        public MinMaxFloat delayDrop;

        [ShowIf(nameof(isDrop), true)]
        public MinMaxFloat timeDrop;
        
        // scale
        public bool isScaleDrop = false;
        public float scaleDropStart = 1;
        public float scaleDropEnd = 1;

        [ShowIf(nameof(isDrop), true)] 
        public TypeAnimation typeAnimationDrop = TypeAnimation.Ease;

        private bool isShowEase => typeAnimationDrop == TypeAnimation.Ease && isDrop;
        [ShowIf(nameof(isShowEase), true)]
        public Ease easeDrop = Ease.Linear;

        private bool isShowCurve => typeAnimationDrop == TypeAnimation.Curve && isDrop;
        [ShowIf(nameof(isShowCurve), true)]
        public AnimationCurve curveDrop;

        #endregion

        [Space(10)]

        #region Move

        [Header("Move")]
        public TypeMove typeMove = TypeMove.Straight;
        
        public List<Vector3> paths;
        public TypeAnimation typeAnimationMove = TypeAnimation.Ease;
 
        public Ease easeMove = Ease.Linear;
        public AnimationCurve curveMove;
        
        // scale
        public bool isScaleMove = false;
        public float scaleMoveStart = 1;
        public float scaleMoveTarget = 1;
        
        // Jump
        [Min(0)]
        public MinMaxFloat jumpPower;
        [Min(1)]
        public int jumpNumber;
 
        public MinMaxFloat midPointOffsetX;
        public MinMaxFloat midPointOffsetZ;
        
        // Sin
        [Min(4)]
        public int pointsCount = 10;

        public MinMaxFloat height;
        public MinMaxFloat delayMove;
        public MinMaxFloat timeMove;
        public System.Action OnCompleted;

        #endregion

        public Vector3 pointSpawn { get; set; }
        public Vector3 pointTarget { get; set; }
        
        public EffectSetting Clone()
        {
            return (EffectSetting) MemberwiseClone();
        }
    }
}