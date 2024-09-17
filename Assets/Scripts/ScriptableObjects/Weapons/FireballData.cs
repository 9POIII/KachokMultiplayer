using UnityEngine;

namespace ScriptableObjects.Weapons
{
    [CreateAssetMenu(fileName = "Fireball", menuName = "Weapon/fireball", order = 1)]
    public class FireballData: ScriptableObject
    {
        public GameObject fireballPrefab;
        public Transform firePoint;
        public float fireballSpeed = 20f;
        public int fireballDamage = 20;
        public GameObject explosionPrefab;
        public GameObject decalPrefab;
        public AudioClip shotSound;
        public float shotTimeout = 0.5f;
        public int manaCost = 20;
        public float explosionRadius;
        public LayerMask damageableLayer;
        public LayerMask obstacleLayer;
        public RecoilProfile recoilProfile;
    }
}