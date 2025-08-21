using UnityEngine;

namespace KDJ.States
{
    public class ReadyState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("입력 준비 상태");
        }

        public void OnUpdate(BoardManager boardManager)
        {
            // 입력 대기. 플레이어의 입력이 있어야 MatchingState로 전환
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 테스트 코드. 하단에서 블럭을 ㅗ자 형태로 파괴
                boardManager.Spawner.TestDeleteBlock();
                boardManager.ChangeState(new MatchingState());
                return;
            }
        }

        public void OnExit(BoardManager boardManager) 
        {
            Debug.Log("입력 준비 상태 종료");
        }
    }
}
