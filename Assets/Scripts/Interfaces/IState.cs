namespace Interfaces
{
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }
}