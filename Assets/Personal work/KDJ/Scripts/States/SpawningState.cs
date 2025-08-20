using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class SpawningState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 생성 상태");
            boardManager.Spawner.SpawnBlockData();
            boardManager.Spawner.SpawnBlock();
        }

        public void OnUpdate(BoardManager boardManager)
        {
            if (boardManager.Spawner.BlankBlockCount == 0)
            {
                boardManager.Spawner.StopCoroutine();
                boardManager.ChangeState(new MatchingState());
            }
        }

        public void OnExit(BoardManager boardManager) 
        {
            Debug.Log("블록 생성 상태 종료");
        }

        private IEnumerator SpawningCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.25f);
            boardManager.Spawner.SpawnBlock();

            if (boardManager.Spawner.BlankBlockCount == 0)
            {
                boardManager.Spawner.StopCoroutine();
                boardManager.ChangeState(new MatchingState());
            }
        }
    }
}
