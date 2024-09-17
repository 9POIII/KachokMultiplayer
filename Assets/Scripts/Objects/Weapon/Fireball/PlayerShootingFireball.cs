using System;
using Objects.PlayerScripts;
using Photon.Pun;
using ScriptableObjects.Weapons;
using UnityEngine;

namespace Objects.Weapon.Fireball
{
    public class PlayerShootingFireball : Weapon
    {
        [SerializeField] private FireballData data;
        
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireballSpeed;
        [SerializeField] private int fireballDamage;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private GameObject decalPrefab;
        [SerializeField] private int manaCost;
        [SerializeField] private float explosionRadius;
        [SerializeField] private LayerMask damageableLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private RecoilProfile recoilProfile;
        
        private CharacterModel model;

        private float lastShotTime;

        public void Initialize()
        {
            data = Resources.Load<FireballData>("Data/Fireball");
            
            model = GetComponent<CharacterModel>();
            
            manaCost = data.manaCost;
            fireballPrefab = data.fireballPrefab;
            firePoint = GameObject.Find("FireballFirePoint").gameObject.transform;
            fireballSpeed = data.fireballSpeed;
            fireballDamage = data.fireballDamage;
            explosionPrefab = data.explosionPrefab;
            decalPrefab = data.decalPrefab;
            shotSound = data.shotSound;
            shotTimeout = data.shotTimeout;
            explosionRadius = data.explosionRadius;
            damageableLayer = data.damageableLayer;
            obstacleLayer = data.obstacleLayer;
            recoilProfile = data.recoilProfile;

            base.Initialize("Fireball", fireballDamage, false, 0f, shotSound, shotTimeout, recoilProfile);
            lastShotTime = -shotTimeout;

            UpdateFireballAmmo("∞");
        }

        public override void Use()
        {
            if (Time.time >= lastShotTime + shotTimeout && model.Mana >= manaCost)
            {
                lastShotTime = Time.time;
                
                if (weaponAnimator != null)
                {
                    weaponAnimator.SetTrigger("shotWeapon");
                }

                GameObject fireball = PhotonNetwork.Instantiate(fireballPrefab.name, firePoint.position, firePoint.rotation);
                fireball.GetComponent<Fireball>().Initialize(explosionPrefab, decalPrefab, fireballDamage, explosionRadius, damageableLayer, obstacleLayer);
                Rigidbody rb = fireball.GetComponent<Rigidbody>();
                rb.velocity = firePoint.forward * fireballSpeed;
                model.SpendMana(manaCost); 
                WeaponRecoil.Instance.ApplyRecoil();
            }
        }
    }
}