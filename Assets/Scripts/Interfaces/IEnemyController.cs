namespace Interfaces
{
    public interface IEnemyController
    {
        void Patrol();
        void Chase();
        void AttackCurrentTarget();
        bool HasReachedDestination();
    }
}