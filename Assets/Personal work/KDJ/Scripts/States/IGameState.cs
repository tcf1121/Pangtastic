namespace KDJ
{
    public interface IGameState
    {
        void OnEnter(BoardManager boardManager);
        void OnUpdate(BoardManager boardManager);
        void OnExit(BoardManager boardManager);
    }
}
