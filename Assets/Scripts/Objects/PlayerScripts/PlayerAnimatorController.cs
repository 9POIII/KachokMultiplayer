using UnityEngine;

namespace Objects.PlayerScripts
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private static readonly int RaiseHand = Animator.StringToHash("raiseHand");
        private static readonly int Shoot = Animator.StringToHash("shoot");
        private static readonly int Speed = Animator.StringToHash("speed");

        public void SetWalking(float speed)
        {
            animator.SetFloat(Speed, speed);
        }

        public void PlayRaiseHand(bool isRaised)
        {
            animator.SetBool(RaiseHand, isRaised);
        }

        public void PlayShoot()
        {
            animator.SetTrigger(Shoot);
        }

        public void SetLayerweight(int layer, float weight)
        {
            animator.SetLayerWeight(layer, weight);            
        }
    }
}