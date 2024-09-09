using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Objects.Door
{
    public class LeverController : MonoBehaviourPun
    {
        public DoorController door;
        public LevelGenerator.LevelGenerator levelGenerator;
        private bool isInteracting = false;
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && !isInteracting)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isInteracting = true;
                    bool newDoorState = !door.isOpen;
                    photonView.RPC("InteractWithLever", RpcTarget.All, newDoorState);
                }
            }
        }

        [PunRPC]
        public void InteractWithLever(bool newDoorState)
        {
            door.photonView.RPC("SetDoorState", RpcTarget.All, newDoorState);
            StartCoroutine(ResetInteraction());
        }

        private IEnumerator ResetInteraction()
        {
            yield return new WaitForSeconds(0.5f);
            isInteracting = false;
        }
    }
}