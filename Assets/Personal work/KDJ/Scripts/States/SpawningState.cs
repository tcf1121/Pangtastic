using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class SpawningState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 생성 상태");
            boardManager.Spawner.StartCoroutine(SpawningCoroutine(boardManager));
        }

        public void OnUpdate(BoardManager boardManager) { }

        public void OnExit(BoardManager boardManager) 
        {
            Debug.Log("블록 생성 상태 종료");
        }

        private IEnumerator SpawningCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.25f);
            boardManager.Spawner.SpawnBlock();
            
            // 생성이 끝나면 매칭 상태로 전환
            boardManager.ChangeState(new MatchingState());
        }
    }
}
