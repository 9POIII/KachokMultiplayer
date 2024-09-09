using System.Collections;
using Objects.Enemies;
using Photon.Pun;
using ScriptableObjects.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Objects.Weapon.Shotgun
{
    public class SimpleShotgun : Weapon
    {
        [SerializeField] private ShotgunData data;
        
        [SerializeField] private int bullets;
        [SerializeField] private int bulletsInBackpack;
        [SerializeField] private int shotgunDamage;
        [SerializeField] private int pelletCount; // Количество дробинок
        [SerializeField] private float spreadAngle; // Угол рассеивания
        [SerializeField] private GameObject hitObjectPrefab;
        [SerializeField] private GameObject decalPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Image reloadProgressImage;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private GameObject muzzleFlash;

        public int CountOfBulletsInWeapon => countOfBulletsInWeapon;
        public int CountOfBulletsInBackpack => countOfBulletsInBackpack;
        
        private int countOfBulletsInWeapon;
        private int countOfBulletsInBackpack;
        private float lastShotTime;
        private LayerMask ignoreLayerMask;

        public void Initialize()
        {
            data = Resources.Load<ShotgunData>("Data/Shotgun");
            bullets = data.bullets;
            bulletsInBackpack = data.bulletsInBackpack;
            shotgunDamage = data.shotgunDamage;
            pelletCount = data.pelletCount;
            spreadAngle = data.spreadAngle;
            hitObjectPrefab = data.hitObjectPrefab;
            decalPrefab = data.decalPrefab;
            firePoint = GameObject.Find("PistolFirePoint").gameObject.transform;
            reloadProgressImage = GameObject.Find("ReloadingProgress").GetComponent<Image>();
            shotSound = data.shotSound;
            shotTimeout = data.shotTimeout;
            reloadTime = data.reloadTime;
            reloadSound = data.reloadSound;
            muzzleFlash = data.muzzleFlash;
            weaponAnimator = GameObject.Find("Shotgun").GetComponentInChildren<Animator>();
            
            base.Initialize("Shotgun", shotgunDamage, true, reloadTime, shotSound, shotTimeout);
            countOfBulletsInWeapon = bullets;
            countOfBulletsInBackpack = bulletsInBackpack;
            lastShotTime = -shotTimeout;

            ignoreLayerMask = LayerMask.GetMask("Ragdoll");

            UpdateAmmo(countOfBulletsInWeapon, countOfBulletsInBackpack);

            if (reloadProgressImage != null)
            {
                reloadProgressImage.fillAmount = 0;
            }
        }

        public override void Use()
        {
            if (!photonView.IsMine || isReloading) return;
            
            if (Time.time >= lastShotTime + shotTimeout)
            {
                if (countOfBulletsInWeapon > 0)
                {
                    lastShotTime = Time.time;
                    
                    CreateShootEffectRPC();
                    
                    if (weaponAnimator != null)
                    {             
                        weaponAnimator.SetTrigger("shootWeapon");
                    }
                    
                    for (int i = 0; i < pelletCount; i++)
                    {
                        ShootPellet();
                    }

                    countOfBulletsInWeapon--;
                    UpdateAmmo(countOfBulletsInWeapon, countOfBulletsInBackpack);

                    if (shotSound != null)
                    {
                        PlayAudioLocally(shotSound);
                        //photonView.RPC("PlayAudio", RpcTarget.Others, "shotSound");
                        photonView.RPC("PlayShotgunAudio", RpcTarget.Others, "shotSound");
                    }
                }
                else
                {
                    Debug.Log("No bullets left in the shotgun.");
                }
            }
        }

        private void ShootPellet()
        {
            Vector3 randomSpread = firePoint.transform.forward;
            randomSpread = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle), 
                Random.Range(-spreadAngle, spreadAngle), 
                Random.Range(-spreadAngle, spreadAngle)) * randomSpread;
            
            Ray ray = new Ray(firePoint.transform.position, randomSpread);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, ~ignoreLayerMask))
            {
                EnemyModel enemy = hit.collider.gameObject.GetComponent<EnemyModel>();
                if (enemy != null)
                {
                    PhotonView targetPhotonView = hit.collider.gameObject.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("TakeDamage", RpcTarget.All, shotgunDamage);
                    }
                }
                else
                {
                    Quaternion rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(0, 180, 0);
                    Vector3 position = hit.point + hit.normal * 0.06f;
                    GameObject decal = PhotonNetwork.Instantiate(decalPrefab.name, position, rotation);
                    decal.transform.SetParent(hit.collider.transform);
                    PhotonNetwork.Instantiate(hitObjectPrefab.name, decal.transform.position, Quaternion.identity);
                }
            }
        }

        public override void Reload()
        {
            if (countOfBulletsInWeapon < bullets && countOfBulletsInBackpack > 0 && !isReloading)
            {
                if (weaponAnimator != null)
                {
                    weaponAnimator.SetTrigger("reloadWeapon");
                }
                StartCoroutine(ReloadCoroutine());
            }
        }

        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            
            if (reloadSound != null)
            {
                PlayAudioLocally(reloadSound);
                //photonView.RPC("PlayAudio", RpcTarget.Others, "reloadSound");
                photonView.RPC("PlayShotgunAudio", RpcTarget.Others, "reloadSound");
            }
            
            float elapsedTime = 0f;

            while (elapsedTime < reloadTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / reloadTime);
                if (reloadProgressImage != null)
                {
                    reloadProgressImage.fillAmount = progress;
                }
                yield return null;
            }

            int bulletsNeeded = 6 - countOfBulletsInWeapon;

            if (countOfBulletsInBackpack >= bulletsNeeded)
            {
                countOfBulletsInWeapon += bulletsNeeded;
                countOfBulletsInBackpack -= bulletsNeeded;
            }
            else
            {
                countOfBulletsInWeapon += countOfBulletsInBackpack;
                countOfBulletsInBackpack = 0;
            }

            isReloading = false;
            UpdateAmmo(countOfBulletsInWeapon, countOfBulletsInBackpack);

            if (reloadProgressImage != null)
            {
                reloadProgressImage.fillAmount = 0;
            }
        }
        
        private void PlayAudioLocally(AudioClip clip)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.maxDistance = 30f;
            source.spatialBlend = 1f;
            source.volume = 0.5f;
            source.Play();
            Destroy(source, clip.length);
        }
        
        [PunRPC]
        private void PlayShotgunAudio(string clipName)
        {
            if (clipName == "shotSound")
            {
                PlayAudioLocally(shotSound);
            }
            else if (clipName == "reloadSound")
            {
                PlayAudioLocally(reloadSound);
            }
        }
        
        public void CreateShootEffectRPC()
        {
            PhotonNetwork.Instantiate(muzzleFlash.name, myModel.transform.GetChild(0).GetChild(1).position, Quaternion.identity);
        }
    }
}