using System;
using System.Xml.Schema;
using DG.Tweening;
using ScriptableObjects.Weapons;
using UnityEngine;

namespace Objects.Weapon
{
    public class WeaponRecoil : MonoBehaviour
    {
        public static WeaponRecoil Instance;

        [SerializeField] private RecoilProfile.PositionShake positionShake = null;
        [SerializeField] private RecoilProfile.RotationShake rotationShake = null;
        
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;

            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void SetRecoilProfile(RecoilProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("RecoilProfile is null");
                return;
            }
    
            positionShake = profile._PositionShake;
            rotationShake = profile._RotationShake;
    
            if (positionShake == null || rotationShake == null)
            {
                Debug.LogError("PositionShake or RotationShake is null in RecoilProfile");
            }
        }

        public void ApplyRecoil()
        {
            if (positionShake.IsOn)
            {
                mainCamera.transform.
                    DOShakePosition(positionShake.Duration, positionShake.Strength,
                        positionShake.Vibrato, positionShake.Randomness, positionShake.Snaping, true, 
                        positionShake.RandomnessMode).
                    SetEase(positionShake.Ease).
                    SetLink(mainCamera.transform.gameObject);
            }

            if (rotationShake.IsOn)
            {
                mainCamera.transform.DOShakeRotation(rotationShake.Duration, rotationShake.Strength, rotationShake.Vibrato,
                        rotationShake.Randomness,
                        true, rotationShake.RandomnessMode).
                    SetEase(rotationShake.Ease).
                    SetLink(mainCamera.transform.gameObject);
            }
        }
    }
}