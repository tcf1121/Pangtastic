using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class RefillState : IGameState
    {
        private Coroutine _fallingCoroutine;
        private int _fallingCount = 0;
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 재충전 상태");
        }

        public void OnUpdate(BoardManager boardManager)
        {
            if (_fallingCount >= 10)
            {
                boardManager.ChangeState(new ReadyState());
                return;
            }

            if (!boardManager.Spawner.HasEmptyBlocks())
            {
                boardManager.ChangeState(new ReadyState());
                return;
            }

            if (_fallingCoroutine == null)
            {
                _fallingCoroutine = boardManager.Spawner.StartCoroutine(FallingCoroutine(boardManager));
            }
        }

        public void OnExit(BoardManager boardManager) 
        {
            Debug.Log("블록 재충전 상태 종료");
        }

        private IEnumerator FallingCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.1f);
            boardManager.Spawner.SortBlockArray();
            _fallingCount++;
            _fallingCoroutine = null;
        }
    }
}
