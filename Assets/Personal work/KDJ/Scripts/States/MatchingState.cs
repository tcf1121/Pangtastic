using UnityEngine;

namespace KDJ.States
{
    public class MatchingState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블럭 매칭 상태");
            // 빈블럭이 있는지 확인 후 상태 전환
            if (boardManager.Spawner.HasEmptyBlocks())
            {
                // 빈 블럭이 있다면 매칭 상태로 진입
                boardManager.ChangeState(new FallingState());
            }
            else
            {
                // 빈 블럭이 없다면 플레이어 입력 대기 상태로 전환
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
