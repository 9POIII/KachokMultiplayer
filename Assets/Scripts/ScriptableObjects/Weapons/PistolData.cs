using UnityEngine;

namespace ScriptableObjects.Weapons
{
    [CreateAssetMenu(fileName = "Pistol", menuName = "Weapon/pistol", order = 1)]
    public class PistolData: ScriptableObject
    {
        public int bullets = 12;
        public int bulletsInBackpack = 36;
        public int pistolDamage = 10;
        public GameObject hitObjectPrefab;
        public GameObject decalPrefab;
        public Transform firePoint;
        public AudioClip shotSound;
        public float shotTimeout = 0.5f;
        public float reloadTime = 2f;
        public AudioClip reloadSound;
        public GameObject muzzleFlash;
        public RecoilProfile recoilProfile;
    }
}