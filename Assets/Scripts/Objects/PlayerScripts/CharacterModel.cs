using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Objects.PlayerScripts
{
    public class CharacterModel : MonoBehaviourPun
    {
        private int health;
        private int mana;
        private int armor;
        private CharacterView view;
        private Image damageOverlay;

        public int Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, 100);
                view.UpdateHealthText(health);
            }
        }
        public int Mana
        {
            get => mana;
            set
            {
                mana = Mathf.Clamp(value, 0, 100);
                view.UpdateManaText(mana);
            }
        }
        public int Armor
        {
            get => armor;
            set
            {
                armor = Mathf.Clamp(value, 0, 100);
                view.UpdateArmorText(armor);
            }
        }

        public void Initialize(int health, int mana, int armor, CharacterView view)
        {
            this.health = health;
            this.mana = mana;
            this.armor = armor;
            this.view = view;
            damageOverlay = GameObject.Find("DamageOverlay").GetComponent<Image>();
        }

        [PunRPC]
        public void TakeDamage(int damage)
        {
            if (armor > 0)
            {
                int remainingDamage = Mathf.Max(damage - armor, 0);
                Armor = Mathf.Max(armor - damage, 0);
                Health = Mathf.Max(health - remainingDamage, 0);
            }
            else
            {
                Health = Mathf.Max(health - damage, 0);
            }

            PlayDamageSound(damage);
        }

        private void PlayDamageSound(int damage)
        {
            if (damage <= 10) return;

            AudioSource source = GetComponent<AudioSource>();
            AudioClip clip = null;

            if (damage >= 11 && damage <= 30)
            {
                clip = Resources.Load<AudioClip>("Sounds/Player/WoundWeak");
            }
            else if (damage >= 31 && damage <= 40)
            {
                TriggerDamageOverlay();
                clip = Resources.Load<AudioClip>("Sounds/Player/WoundMedium");
            }
            else if (damage > 40)
            {
                TriggerDamageOverlay();
                clip = Resources.Load<AudioClip>("Sounds/Player/WoundStrong");
            }

            if (clip != null)
            {
                source.PlayOneShot(clip);
            }
        }

        [PunRPC]
        public void Heal(int heal)
        {
            Health += heal;
        }

        [PunRPC]
        public void SpendMana(int count)
        {
            Mana -= count;
        }

        [PunRPC]
        public void AddMana(int count)
        {
            Mana += count;
        }
        
        [PunRPC]
        public void SpendArmor(int count)
        {
            Armor -= count;
        }

        [PunRPC]
        public void AddArmor(int count)
        {
            Armor += count;
        }
        
        public void TriggerDamageOverlay()
        {
            StartCoroutine(DamageOverlayCoroutine());
        }

        private IEnumerator DamageOverlayCoroutine()
        {
            Color originalColor = new Color(255, 0, 0, 0);
            Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);

            damageOverlay.color = targetColor;

            yield return new WaitForSeconds(0.1f);

            float elapsedTime = 0f;
            float duration = 0.5f;

            while (elapsedTime < duration)
            {
                damageOverlay.color = Color.Lerp(targetColor, originalColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            damageOverlay.color = originalColor;
        }
    }
}