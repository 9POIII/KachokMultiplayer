using Photon.Pun;
using UnityEngine;

namespace Objects.Enemies
{
    public class EnemyModel : MonoBehaviourPun
    {
        private int damage;
        private int health;
        private float attackRange;
        private float attackInterval;

        private IEnemyView view;

        public float AttackRange { get; private set; }
        public float AttackInterval { get; private set; }
        public int Damage
        {
            get => damage;
            set
            {
                damage = value;
                view.UpdateDamageText(damage);
            }
        }
        public int Health
        {
            get => health;
            set
            {
                health = value;
                view.UpdateHealthText(health);
            }
        }
        
        public void Initialize(int health, int damage, float attackRange, float attackInterval, IEnemyView view)
        {
            this.health = health;
            this.damage = damage;
            this.attackRange = attackRange;
            this.attackInterval = attackInterval;
            this.view = view;

            UpdateView();
        }

        private void UpdateView()
        {
            view.UpdateHealthText(health);
            view.UpdateDamageText(damage);
        }

        [PunRPC]
        public void TakeDamage(int damage)
        {
            Health -= damage;
            Debug.Log("Enemy took " + damage + " damage. Health is now " + health);
            
            if (Health <= 0)
            {
                Die();
            }
        }

        [PunRPC]
        public void Heal(int heal)
        {
            Health += heal;
            Debug.Log("Enemy healed " + heal + " health. Health is now " + health);
        }

        private void Die()
        {
            GetComponent<EnemyController>().Die();
        }
    }
}