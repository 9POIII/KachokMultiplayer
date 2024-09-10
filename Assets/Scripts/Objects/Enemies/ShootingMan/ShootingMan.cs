using Photon.Pun;
using UnityEngine;

namespace Objects.Enemies.ShootingMan
{
    public class ShootingMan : BaseEnemy
    {
        [Header("Gunman Specific Settings")]
        [SerializeField] private float shootingRange = 15f;
        [SerializeField] private Transform gunBarrel;
        [SerializeField] private float fieldOfViewAngle = 90f;
        [SerializeField] private AudioClip shotSound;
        [SerializeField] private GameObject hitEffectPrefab;

        private AudioSource audioSource;
        private PhotonView photonView;
        private LayerMask ignoreLayerMask;

        protected override void Start()
        {
            base.Start();
            maxHealth = 75;
            agent.speed = 2f;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;

            photonView = GetComponent<PhotonView>();

            ignoreLayerMask = LayerMask.GetMask("Ragdoll");
        }

        protected override void Update()
        {
            base.Update();

            if (CanSeePlayer())
            {
                if (Vector3.Distance(transform.position, currentTarget.transform.position) <= shootingRange)
                {
                    AttackCurrentTarget();
                }
            }
        }
        
        private bool CanSeePlayer()
        {
            if (currentTarget == null) return false;

            Vector3 directionToPlayer = currentTarget.transform.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= fieldOfViewAngle / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out hit, shootingRange, ~ignoreLayerMask))
                {
                    if (hit.collider.gameObject == currentTarget)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public override void AttackCurrentTarget()
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                photonView.RPC("FireRay", RpcTarget.All);
                attackTimer = attackInterval;
            }
        }
        
        [PunRPC]
        private void FireRay()
        {
            if (currentTarget == null) return;

            Ray ray = new Ray(gunBarrel.position, currentTarget.transform.position - gunBarrel.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, shootingRange, ~ignoreLayerMask))
            {
                if (hit.collider.gameObject == currentTarget)
                {
                    PhotonView targetPhotonView = hit.collider.gameObject.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("TakeDamage", RpcTarget.All, damage);
                    }
                }
                
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }
                if (shotSound != null)
                {
                    audioSource.PlayOneShot(shotSound);
                }
            }
        }

        public override void Die()
        {
            base.Die();
            Debug.Log("Gunman died");
        }
    }
}
