using UnityEngine;

namespace KDJ.States
{
    public class MatchingState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블럭 매칭 상태");
            if (boardManager.Spawner.HasEmptyBlocks())
            {
                boardManager.ChangeState(new SpawningState());
            }
            else
            {
                boardManager.ChangeState(new ReadyState());
            }
        }

        public void OnUpdate(BoardManager boardManager) { }

        public void OnExit(BoardManager boardManager) 
        {
            Debug.Log("블럭 매칭 상태 종료");
        }
    }
}
