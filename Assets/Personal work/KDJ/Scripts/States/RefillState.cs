using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class RefillState : IGameState
    {
        private Coroutine _fallingCoroutine;

        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 재충전 상태");
            boardManager.Spawner.CheckBlockInArray();
            boardManager.Spawner.CheckBlockArray(boardManager);
        }

        public void OnUpdate(BoardManager boardManager)
        {
            if (boardManager.Spawner.HasEmptyBlocks())
            {
                if (_fallingCoroutine == null)
                {
                    _fallingCoroutine = boardManager.Spawner.StartCoroutine(FallingCoroutine(boardManager));
                }
            }
            else
            {
                // Board is full, transition to check for matches.
                boardManager.ChangeState(new ReadyState());
            }
        }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("블록 재충전 상태 종료");
            if (_fallingCoroutine != null)
            {
                boardManager.Spawner.StopCoroutine(_fallingCoroutine);
                _fallingCoroutine = null;
            }
            boardManager.Spawner.DestroyBlockData.Clear(); // 파괴된 블럭 데이터 초기화
            boardManager.BlockMover.StartPos = Vector2.zero; // 초기 시작 위치 설정
            boardManager.BlockMover.EndPos = Vector2.zero; // 초기 종료 위치 설정
        }

        private IEnumerator FallingCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.033f);
            boardManager.Spawner.SortBlockArray();
            yield return new WaitForSeconds(0.033f);
            _fallingCoroutine = null;
        }
    }
}