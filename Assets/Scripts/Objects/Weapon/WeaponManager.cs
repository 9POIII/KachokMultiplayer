using System.Collections;
using System.Collections.Generic;
using Objects.PlayerScripts;
using Objects.Weapon.Fireball;
using Objects.Weapon.Pistol;
using Objects.Weapon.Shotgun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Objects.Weapon
{
    public class WeaponManager : MonoBehaviourPun
    {
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private GameObject[] weaponModels;
        [SerializeField] private GameObject weaponSelectionPanel; 
        [SerializeField] private Image[] weaponImages; 
        public List<Weapon> weapons = new List<Weapon>();
        private int currentWeaponIndex = 0;
        private Coroutine hidePanelCoroutine;  
        
        void Start()
        {
            InitializeWeapons();

            if (!photonView.IsMine) return;

            SelectWeapon();
            UpdateActiveWeapon();
            weaponSelectionPanel.SetActive(false);
        }

        private void InitializeWeapons()
        {
            PlayerShootingFireball fireballWeapon = gameObject.AddComponent<PlayerShootingFireball>();
            fireballWeapon.Initialize();
            fireballWeapon.GetMyModel(weaponModels[1]);
            weapons.Add(fireballWeapon);

            SimplePistol pistol = gameObject.AddComponent<SimplePistol>();
            pistol.Initialize();
            pistol.GetMyModel(weaponModels[0]);
            weapons.Add(pistol);

            SimpleShotgun shotgun = gameObject.AddComponent<SimpleShotgun>();
            shotgun.Initialize();
            shotgun.GetMyModel(weaponModels[2]);
            weapons.Add(shotgun);
        }

        void Update()
        {
            if (!photonView.IsMine) return;

            int previousWeaponIndex = currentWeaponIndex;

            // Логика выбора оружия и отображение панели
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (!weapons[currentWeaponIndex].IsReloading)
                {
                    currentWeaponIndex = 0;
                    ToggleWeaponSelectionPanel();
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (!weapons[currentWeaponIndex].IsReloading)
                {
                    currentWeaponIndex = 1;
                    ToggleWeaponSelectionPanel();
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (!weapons[currentWeaponIndex].IsReloading)
                {
                    currentWeaponIndex = 2;
                    ToggleWeaponSelectionPanel();
                }
            }

            if (previousWeaponIndex != currentWeaponIndex)
            {
                SelectWeapon();
                UpdateActiveWeapon();
                UpdateWeaponSelectionUI();  // Обновление UI
            }

            if (Input.GetButtonDown("Fire1") && weapons[currentWeaponIndex] != null)
            {
                weapons[currentWeaponIndex].Use();
                animationController.PlayShoot();
            }

            if (Input.GetKeyDown(KeyCode.R) && weapons[currentWeaponIndex] != null)
            {
                weapons[currentWeaponIndex].Reload();
            }
        }

        void SelectWeapon()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] != null)
                {
                    weapons[i].enabled = (i == currentWeaponIndex);
                }
            }
            Debug.Log(weapons[currentWeaponIndex].RecoilProfile);
            WeaponRecoil.Instance.SetRecoilProfile(weapons[currentWeaponIndex].RecoilProfile);
        }

        void UpdateActiveWeapon()
        {
            foreach (GameObject model in weaponModels)
            {
                model.SetActive(false);
                animationController.PlayRaiseHand(false);
            }

            if (weapons[currentWeaponIndex] is SimplePistol pistol)
            {
                pistol.UpdateAmmo(pistol.CountOfBulletsInWeapon, pistol.CountOfBulletsInBackpack);
                weaponModels[0].SetActive(true);
                animationController.PlayRaiseHand(true);
            }
            else if (weapons[currentWeaponIndex] is PlayerShootingFireball fireball)
            {
                fireball.UpdateFireballAmmo("∞");
                weaponModels[1].SetActive(true);
            }
            else if (weapons[currentWeaponIndex] is SimpleShotgun shotgun)
            {
                shotgun.UpdateAmmo(shotgun.CountOfBulletsInWeapon, shotgun.CountOfBulletsInBackpack);
                weaponModels[2].SetActive(true);
                animationController.PlayRaiseHand(true);
            }
        }

        private void ToggleWeaponSelectionPanel()
        {
            weaponSelectionPanel.SetActive(true); 
            UpdateWeaponSelectionUI();

            if (hidePanelCoroutine != null)
            {
                StopCoroutine(hidePanelCoroutine);
            }
            hidePanelCoroutine = StartCoroutine(HideWeaponSelectionPanelAfterDelay(2f));
        }

        private IEnumerator HideWeaponSelectionPanelAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            weaponSelectionPanel.SetActive(false);
        }

        private void UpdateWeaponSelectionUI()
        {
            for (int i = 0; i < weaponImages.Length; i++)
            {
                if (i == currentWeaponIndex)
                {
                    weaponImages[i].color = Color.yellow;
                }
                else
                {
                    weaponImages[i].color = Color.white;
                }
            }
        }
    }
}
