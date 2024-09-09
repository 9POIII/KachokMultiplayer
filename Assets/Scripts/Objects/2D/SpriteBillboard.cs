using UnityEngine;

namespace Objects._2D
{
    public class SpriteBillboard : MonoBehaviour
    {
        [SerializeField] private bool freezeXZAxis = true;
        void LateUpdate()
        {
            if (freezeXZAxis)
            {
                transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
            }
            else
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
