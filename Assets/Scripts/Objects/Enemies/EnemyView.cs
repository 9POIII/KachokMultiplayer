using TMPro;
using UnityEngine;

namespace Objects.Enemies
{
    public interface IEnemyView
    {
        void UpdateHealthText(int health);
        void UpdateDamageText(int damage);
        void SetTextVisibility(bool isVisible);
    }

    public class EnemyView : MonoBehaviour, IEnemyView
    {
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text damageText;

        public void UpdateHealthText(int health)
        {
            healthText.text = $"Health: {health}";
        }

        public void UpdateDamageText(int damage)
        {
            damageText.text = $"Damage: {damage}";
        }

        public void SetTextVisibility(bool isVisible)
        {
            healthText.gameObject.SetActive(isVisible);
            damageText.gameObject.SetActive(isVisible);
        }
    }
}