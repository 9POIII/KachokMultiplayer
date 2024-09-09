using Photon.Pun;
using UnityEngine;

namespace Objects.Weapon.Fireball
{
    public class Fireball : MonoBehaviourPun
    {
        private GameObject explosionPrefab;
        private GameObject decalPrefab;
        private int damage;
        private float explosionRadius;
        private LayerMask damageableLayer;
        private LayerMask obstacleLayer;
        private float explosionForce = 5;
        
        private float minMultiplier = 0.1f;
        private float maxMultiplier = 1f;
        private float guaranteedFullDamageRadius = 1f;

        public void Initialize(GameObject explosion, GameObject decal, int damage, float radius, LayerMask damageLayer, LayerMask obstacleLayer)
        {
            explosionPrefab = explosion;
            decalPrefab = decal;
            this.damage = damage;
            this.explosionRadius = radius;
            this.damageableLayer = damageLayer;
            this.obstacleLayer = obstacleLayer;
        }

        private void OnCollisionEnter(Collision collision)
        {
            PhotonNetwork.Instantiate(explosionPrefab.name, transform.position, Quaternion.identity);

            if (photonView.IsMine)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayer);
                foreach (var hitCollider in hitColliders)
                {
                    if (Physics.Linecast(transform.position, hitCollider.transform.position, obstacleLayer))
                    {
                        continue;
                    }

                    PhotonView targetPhotonView = hitCollider.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        int targetLayer = targetPhotonView.gameObject.layer;

                        if (targetLayer == LayerMask.NameToLayer("Enemy") || targetLayer == LayerMask.NameToLayer("Player"))
                        {
                            float distance = Vector3.Distance(hitCollider.transform.position, gameObject.transform.position);
                            distance = Mathf.Clamp(distance, guaranteedFullDamageRadius, explosionRadius);
                            float t = Mathf.InverseLerp(guaranteedFullDamageRadius, explosionRadius, distance);
                            float damageModifier = Mathf.Lerp(maxMultiplier, minMultiplier, t);
                            targetPhotonView.RPC("TakeDamage", RpcTarget.All, (int)(damage * damageModifier));
                        }
                        
                        Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            Vector3 explosionDirection = hitCollider.transform.position - transform.position;
                            rb.AddForce(explosionDirection.normalized * explosionForce, ForceMode.Impulse);
                        }
                    }
                    else
                    {
                        ContactPoint contact = collision.contacts[0];
                        Quaternion rotation = Quaternion.LookRotation(contact.normal) * Quaternion.Euler(0, 180, 0);
                        Vector3 position = contact.point + contact.normal * 0.06f;
                        GameObject decal = PhotonNetwork.Instantiate(decalPrefab.name, position, rotation);
                        decal.transform.SetParent(collision.transform);
                    }
                }
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}