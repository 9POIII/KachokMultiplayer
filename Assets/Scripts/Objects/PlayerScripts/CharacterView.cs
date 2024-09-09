using System;
using TMPro;
using UnityEngine;

namespace Objects.PlayerScripts
{
    public class CharacterView : MonoBehaviour
    {
        public TMP_Text HealthText;
        public TMP_Text ManaText;
        public TMP_Text ArmorText;

        public void UpdateHealthText(int health)
        {
            HealthText.text = $"Health: {health}";
        }
        
        public void UpdateManaText(int mana)
        {
            ManaText.text = $"Mana: {mana}";
        }
        
        public void UpdateArmorText(int armor)
        {
            ArmorText.text = $"Armor: {armor}";
        }
    }
}