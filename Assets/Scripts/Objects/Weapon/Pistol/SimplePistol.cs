using System.Collections;
using Objects.Enemies;
using Photon.Pun;
using ScriptableObjects.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Objects.Weapon.Pistol
{
    public class SimplePistol : Weapon
    {
        [SerializeField] private PistolData data;
        
        [SerializeField] private int bullets;
        [SerializeField] private int bulletsInBackpack;
        [SerializeField] private int pistolDamage;
        [SerializeField] private GameObject hitObjectPrefab;
        [SerializeField] private GameObject decalPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Image reloadProgressImage;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private RecoilProfile recoilProfile;

        public int CountOfBulletsInWeapon => countOfBulletsInWeapon;
        public int CountOfBulletsInBackpack => countOfBulletsInBackpack;
        
        private int countOfBulletsInWeapon;
        private int countOfBulletsInBackpack;
        private float lastShotTime;
        private LayerMask ignoreLayerMask;

        public void Initialize()
        {
            data = Resources.Load<PistolData>("Data/Pistol");
            bullets = data.bullets;
            bulletsInBackpack = data.bulletsInBackpack;
            pistolDamage = data.pistolDamage;
            hitObjectPrefab = data.hitObjectPrefab;
            decalPrefab = data.decalPrefab;
            firePoint = GameObject.Find("PistolFirePoint").gameObject.transform;
            reloadProgressImage = GameObject.Find("ReloadingProgress").GetComponent<Image>();
            shotSound = data.shotSound;
            shotTimeout = data.shotTimeout;
            reloadTime = data.reloadTime;
            reloadSound = data.reloadSound;
            muzzleFlash = data.muzzleFlash;
            recoilProfile = data.recoilProfile;
            weaponAnimator = GameObject.Find("Pistol").GetComponentInChildren<Animator>();
            
            base.Initialize("Pistol", pistolDamage, true, reloadTime, shotSound, shotTimeout, recoilProfile);
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
                    Debug.Log(weaponAnimator);
                    
                    Ray ray = new Ray(firePoint.transform.position, Camera.main.transform.forward);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, ~ignoreLayerMask))
                    {
                        EnemyModel enemy = hit.collider.gameObject.GetComponent<EnemyModel>();
                        if (enemy != null)
                        {
                            PhotonView targetPhotonView = hit.collider.gameObject.GetComponent<PhotonView>();
                            if (targetPhotonView != null)
                            {
                                targetPhotonView.RPC("TakeDamage", RpcTarget.All, damage);
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

                    countOfBulletsInWeapon--;
                    UpdateAmmo(countOfBulletsInWeapon, countOfBulletsInBackpack);

                    if (shotSound != null)
                    {
                        PlayAudioLocally(shotSound);
                        photonView.RPC("PlayPistolAudio", RpcTarget.Others, "shotSound");
                    } 
                    WeaponRecoil.Instance.ApplyRecoil();
                }
                else
                {
                    Debug.Log("No bullets left in the pistol.");
                }
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
        
        public void CreateShootEffectRPC()
        {
            PhotonNetwork.Instantiate(muzzleFlash.name, myModel.transform.GetChild(0).GetChild(1).position, Quaternion.identity);
        }

        [PunRPC]
        private void PlayPistolAudio(string clipName)
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
                photonView.RPC("PlayPistolAudio", RpcTarget.Others, "reloadSound");
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

            int bulletsNeeded = 12 - countOfBulletsInWeapon;

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
    }
}