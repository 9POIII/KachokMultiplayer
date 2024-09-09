using Interfaces;
using UnityEngine;

namespace Objects.Enemies.StateMachine
{
    public abstract class EnemyState : IState
    {
        protected readonly IEnemyController controller;

        protected EnemyState(IEnemyController controller)
        {
            this.controller = controller;
        }

        public virtual void Enter() { }

        public abstract void Execute();

        public virtual void Exit() { }
    }

    public class PatrolState : EnemyState
    {
        public PatrolState(IEnemyController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            controller.Patrol();
        }

        public override void Execute()
        { 
            if (controller.HasReachedDestination())
            {
                controller.Patrol();
            }
        }
    }

    public class ChaseState : EnemyState
    {
        public ChaseState(IEnemyController controller) : base(controller) { }

        public override void Enter()
        {
            Debug.Log("Enter Chase State");
        }

        public override void Execute()
        {
            controller.Chase();
        }

        public override void Exit()
        {
            Debug.Log("Exit Chase State");
        }
    }

    public class AttackState : EnemyState
    {
        public AttackState(IEnemyController controller) : base(controller) { }

        public override void Enter()
        {
            Debug.Log("Enter Attack State");
        }

        public override void Execute()
        {
            controller.AttackCurrentTarget();
        }

        public override void Exit()
        {
            Debug.Log("Exit Attack State");
        }
    }
}