using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Objects.Weapon
{
    public class AmmoDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text ammoText;

        private void OnEnable()
        {
            WeaponEvents.OnAmmoChanged.AddListener(UpdateAmmoText);
            WeaponEvents.OnFireballAmmoChanged.AddListener(UpdateFireballAmmoText);
        }

        private void OnDisable()
        {
            WeaponEvents.OnAmmoChanged.RemoveListener(UpdateAmmoText);
            WeaponEvents.OnFireballAmmoChanged.RemoveListener(UpdateFireballAmmoText);
        }

        private void UpdateAmmoText(int currentAmmo, int ammoInBackpack)
        {
            ammoText.text = $"{currentAmmo}/{ammoInBackpack}";
        }

        private void UpdateFireballAmmoText(string ammoText)
        {
            this.ammoText.text = $"{ammoText}";
        }
    }
}