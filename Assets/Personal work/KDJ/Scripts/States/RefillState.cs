using UnityEngine;
using System.Collections;

namespace KDJ.States
{
    public class RefillState : IGameState
    {
        private Coroutine _fallingCoroutine;
        private Coroutine _changeStateCoroutine;

        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블록 재충전 상태");
            boardManager.Spawner.CheckBlockArray(boardManager);
        }

        public void OnUpdate(BoardManager boardManager)
        {
            if (boardManager.Spawner.CanBlockMoveInArray())
            {
                if (_fallingCoroutine == null)
                {
                    _fallingCoroutine = boardManager.Spawner.StartCoroutine(FallingCoroutine(boardManager));
                }
            }
            else
            {
                if (_changeStateCoroutine == null)
                {
                    _changeStateCoroutine = boardManager.Spawner.StartCoroutine(ChangeStateDelay(boardManager));
                }
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
            yield return new WaitForSeconds(0.05f);
            boardManager.Spawner.SortBlockArray();
            _fallingCoroutine = null;
        }

        private IEnumerator ChangeStateDelay(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.1f);
            boardManager.ChangeState(new ReadyState());
            _changeStateCoroutine = null;
        }
    }
}