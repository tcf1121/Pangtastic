using UnityEngine;

namespace KDJ.States
{
    public class InitializeState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("초기화 상태");
            boardManager.Spawner.InitBlockArray();
            boardManager.Spawner.DrawBlock();
            boardManager.ChangeState(new ReadyState());
        }

        public void OnUpdate(BoardManager boardManager) { }

        public void OnExit(BoardManager boardManager) 
        {
            Debug.Log("초기화 상태 종료");
        }
    }
}
