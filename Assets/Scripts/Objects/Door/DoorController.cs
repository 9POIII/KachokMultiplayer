using Photon.Pun;
using UnityEngine;

public class DoorController : MonoBehaviourPun
{
    public bool isOpen = false;
    public Transform openPosition;
    public Transform closedPosition;
    public float openSpeed = 2f;

    private void Update()
    {
        if (isOpen)
        {
            transform.position = Vector3.Lerp(transform.position, openPosition.position, Time.deltaTime * openSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, closedPosition.position, Time.deltaTime * openSpeed);
        }
    }

    [PunRPC]
    public void SetDoorState(bool state)
    {
        isOpen = state;
    }
}