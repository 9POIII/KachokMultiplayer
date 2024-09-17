using System;
using DG.Tweening;
using UnityEngine;

namespace ScriptableObjects.Weapons
{
    [CreateAssetMenu(fileName = "RecoilProfile", menuName = "Weapon/RecoilProfile", order = 1)]
    public class RecoilProfile : ScriptableObject
    {
        public RotationShake _RotationShake;
        public PositionShake _PositionShake;
        
        [Serializable]
        public class RotationShake
        {
            public bool IsOn;
            public float Duration;
            public Ease Ease;
            public ShakeRandomnessMode RandomnessMode;
            public Vector3 Strength;
            public float Randomness;
            public int Vibrato;
            public bool Snaping;
        }
        
        [Serializable]
        public class PositionShake
        {
            public bool IsOn;
            public float Duration;
            public Ease Ease;
            public ShakeRandomnessMode RandomnessMode;
            public Vector3 Strength;
            public float Randomness;
            public int Vibrato;
            public bool Snaping;
        }
    }
}