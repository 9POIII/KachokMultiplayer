using System;
using System.Xml.Schema;
using DG.Tweening;
using UnityEngine;

namespace Objects.Weapon
{
    public class WeaponRecoil : MonoBehaviour
    {
        public static WeaponRecoil Instance;

        [SerializeField] private Vector3 shakePosition = new Vector3(-0.125f, 0.015f, 0);
        [SerializeField] private Vector3 shakeRotation = new Vector3(2f, 0.5f,0.5f);
        
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;

            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void ApplyRecoil(Weapon weapon)
        {
            mainCamera.transform.
                DOShakePosition(0.1f, shakePosition, 15, 0f, false,
                true, ShakeRandomnessMode.Full).
                SetEase(Ease.Linear).
                SetLink(mainCamera.transform.gameObject);

            mainCamera.transform.DOShakeRotation(0.1f, shakeRotation, 5, 0f,
                    true, ShakeRandomnessMode.Harmonic).
                SetEase(Ease.Linear).
                SetLink(mainCamera.transform.gameObject);
        }
    }
}