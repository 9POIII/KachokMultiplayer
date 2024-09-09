using UnityEngine;
using UnityEngine.UI;

namespace ScriptableObjects.Weapons
{
    [CreateAssetMenu(fileName = "Shotgun", menuName = "Weapon/shotgun", order = 1)]
    public class ShotgunData : ScriptableObject
    {
        public int bullets;
        public int bulletsInBackpack;
        public int shotgunDamage;
        public int pelletCount;
        public float spreadAngle;
        public GameObject hitObjectPrefab;
        public GameObject decalPrefab;
        public AudioClip shotSound;
        public float shotTimeout = 0.5f;
        public float reloadTime = 2f;
        public AudioClip reloadSound;
        public GameObject muzzleFlash;
    }
}