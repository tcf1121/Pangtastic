using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class FallingState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 낙하 상태");
            boardManager.Spawner.StartCoroutine(FallingCoroutine(boardManager));
            // boardManager.Spawner.SortBlockDataArray();
            // boardManager.Spawner.SortViewBlockArray();
            // boardManager.Spawner.RefreshBlock();
            // boardManager.ChangeState(new ReadyState());
        }

        public void OnUpdate(BoardManager boardManager) { }

        public void OnExit(BoardManager boardManager) 
        {
            Debug.Log("블록 낙하 상태 종료");
        }

        private IEnumerator FallingCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.5f);
            boardManager.Spawner.SortBlockDataArray();
            boardManager.Spawner.SortViewBlockArray();
            boardManager.Spawner.RefreshBlock();
            boardManager.ChangeState(new ReadyState());
        }
    }
}
