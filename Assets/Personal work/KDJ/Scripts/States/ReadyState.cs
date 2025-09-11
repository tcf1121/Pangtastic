using System.Collections;
using UnityEngine;

namespace KDJ.States
{
    public class ReadyState : IGameState
    {
        private Coroutine _matchDelayCoroutine;
        private bool _isSwapping = false; // 중복 스왑 방지 플래그

        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("입력 준비 상태");
            boardManager.BlockMover.ResetPos();
            _isSwapping = false;

            // 매칭되는 블럭이 있을 경우 매칭 상태로 전환
            if (boardManager.MatchChecker.AllBlockMatchCheck(boardManager))
            {
                if (_matchDelayCoroutine == null)
                {
                    _matchDelayCoroutine = boardManager.StartCoroutine(MatchDelayCoroutine(boardManager));
                }
            }
        }

        public void OnUpdate(BoardManager boardManager)
        {
            // 아이템이 선택되었거나 터치 불가능 상태면 입력 무시
            if (boardManager.IsItemSelected && !BoardManager.CanTouch) return;

            // 스왑 중에는 다른 입력 및 로직을 처리하지 않음
            if (_isSwapping) return;

            if (boardManager.Spawner.HasEmptyBlocks())
            {
                if (boardManager.Spawner.CanBlockMoveInArray())
                {
                    boardManager.ChangeState(new RefillState());
                }
            }
            else
            {
                if (!boardManager.MatchChecker.AllBlockMatchPossibilityCheck(boardManager, out int possibleCount))
                {
                    boardManager.Spawner.Shuffle(boardManager);
                    boardManager.ChangeState(new ReadyState());
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                boardManager.Spawner.Shuffle(boardManager);
                boardManager.ChangeState(new ReadyState());
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = -Camera.main.transform.position.z;
                boardManager.BlockMover.StartPos = Camera.main.ScreenToWorldPoint(mousePosition);
                TestBlockInfo(boardManager);
            }

            if (Input.GetMouseButtonUp(0))
            {
                boardManager.ResetUI();
                if (boardManager.BlockMover.StartPos != Vector2.zero)
                {
                    boardManager.StartCoroutine(SwapAndChangeState(boardManager));
                }
            }
        }

        private IEnumerator SwapAndChangeState(BoardManager boardManager)
        {
            _isSwapping = true; // 스왑 시작, 입력 방지

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -Camera.main.transform.position.z;
            boardManager.BlockMover.EndPos = Camera.main.ScreenToWorldPoint(mousePosition);

            bool swapSuccess = false;
            yield return boardManager.StartCoroutine(
                boardManager.BlockMover.TrySwap(boardManager, result => swapSuccess = result)
            );

            if (swapSuccess)
            {
                boardManager.ChangeState(new MatchingState());
            }
            else
            {
                // 스왑에 실패하면 다시 입력 가능 상태로
                _isSwapping = false;
            }
        }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("입력 준비 상태 종료");
        }

        private void TestBlockInfo(BoardManager boardManager)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -Camera.main.transform.position.z;
            Vector2 targetPos = Camera.main.ScreenToWorldPoint(mousePosition);

            Vector2Int gridPos = boardManager.BlockMover.WorldToGrid(targetPos, boardManager.Spawner.GameBoardData.Width, boardManager.Spawner.GameBoardData.Height);

            if (gridPos.x < 0 || gridPos.y < 0 || gridPos.x >= boardManager.Spawner.GameBoardData.Width || gridPos.y >= boardManager.Spawner.GameBoardData.Height)
            {
                return;
            }

            if (!boardManager.Spawner.GameBoardData.BlockPlate.BlockPlateArray[gridPos.y, gridPos.x]) return;

            Block block = boardManager.Spawner.GameBoardData.GetBlock(gridPos.x, gridPos.y);
            if (block != null)
            {
                boardManager.UpdateUI(block, gridPos.x, gridPos.y);
            }
        }

        private IEnumerator MatchDelayCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(0.1f);
            boardManager.ChangeState(new MatchingState());
            _matchDelayCoroutine = null;
        }
    }
}