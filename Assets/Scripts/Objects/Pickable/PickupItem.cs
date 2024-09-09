using Objects.PlayerScripts;
using Photon.Pun;
using UnityEngine;

namespace Objects.Pickable
{
    public class PickupItem : MonoBehaviourPun
    {
        public enum PickupType
        {
            Health,
            Armor
        }

        [Header("Object settings")]
        [SerializeField] private PickupType pickupType;
        [SerializeField] private int amount = 10;

        [Header("Sound settings")]
        [SerializeField] private AudioClip pickupSound;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PhotonView playerPhotonView = other.GetComponent<PhotonView>();
                CharacterModel characterModel = other.GetComponent<CharacterModel>();

                if (playerPhotonView != null && playerPhotonView.IsMine && characterModel != null)
                {
                    bool canPickUp = false;

                    switch (pickupType)
                    {
                        case PickupType.Health:
                            if (characterModel.Health + amount <= 100)
                            {
                                playerPhotonView.RPC("Heal", RpcTarget.All, amount);
                                canPickUp = true;
                            }
                            break;
                        case PickupType.Armor:
                            if (characterModel.Armor + amount <= 100)
                            {
                                playerPhotonView.RPC("AddArmor", RpcTarget.All, amount);
                                canPickUp = true;
                            }
                            break;
                    }

                    if (canPickUp)
                    {
                        photonView.RPC("PlayPickupSoundAndDestroy", RpcTarget.All);
                    }
                }
            }
        }

        [PunRPC]
        private void PlayPickupSoundAndDestroy()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}