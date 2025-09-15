using LHJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KDJ.States
{
    public class ReadyState : IGameState
    {
        private Coroutine _matchDelayCoroutine;
        private Coroutine _hintCoroutine;
        private bool _isSwapping = false; // 중복 스왑 방지 플래그


        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("입력 준비 상태");
            boardManager.InitialSwapPosition = null; // 새 턴이 시작될 때 스왑 위치 초기화
            boardManager.BlockMover.ResetPos();
            _isSwapping = false;

            // 매칭되는 블럭이 있을 경우 매칭 상태로 전환
            if (boardManager.MatchChecker.AllBlockMatchCheck(boardManager))
            {
                if (_matchDelayCoroutine != null)
                {
                    boardManager.StopCoroutine(_matchDelayCoroutine);
                }

                _matchDelayCoroutine = boardManager.StartCoroutine(MatchDelayCoroutine(boardManager));
            }
        }

        public void OnUpdate(BoardManager boardManager)
        {
            // 특수 블록 효과가 실행 중이면 힌트 코루틴 및 힌트 숨김
            if (SpecialBlockEffect.effectRunning || boardManager.IsUseItem)
            {
                boardManager.HideHint(boardManager.CurHintPositions);
                boardManager.StopCoroutine(_hintCoroutine);
                _hintCoroutine = null;
                boardManager.CurHintPositions.Clear();
                boardManager.IsUseItem = false;
            }

            // 아이템이 선택되었거나 터치 불가능 상태면 힌트 제거 후 입력 무시
            if (boardManager.IsItemSelected && !BoardManager.CanTouch)
            {
                boardManager.HideHint(boardManager.CurHintPositions);
                boardManager.StopCoroutine(_hintCoroutine);
                _hintCoroutine = null;
                boardManager.CurHintPositions.Clear();
                return;
            }

            // 스왑 중에는 다른 입력 및 로직을 처리하지 않음
            if (_isSwapping) return;

            if (boardManager.MatchCombo.CurCombo == 0)
            {
                StartHint();
            }

            BlockCheck(boardManager);
            UserInput(boardManager);
        }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("입력 준비 상태 종료");
            if (_matchDelayCoroutine != null)
                boardManager.StopCoroutine(_matchDelayCoroutine);
            _matchDelayCoroutine = null;
        }

        private IEnumerator SwapAndChangeState(BoardManager boardManager)
        {
            _isSwapping = true; // 스왑 시작, 입력 방지
            bool isHintPosSwap = false;

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -Camera.main.transform.position.z;
            boardManager.BlockMover.EndPos = Camera.main.ScreenToWorldPoint(mousePosition);

            if (boardManager.CurHintPositions.Count > 0)
            {
                foreach (var pos in boardManager.CurHintPositions)
                {
                    if (boardManager.BlockMover.ValidateAndSetPositions(boardManager) && (pos == boardManager.BlockMover.StartBlockPos || pos == boardManager.BlockMover.EndBlockPos))
                    {
                        isHintPosSwap = true;
                        break;
                    }
                }
            }

            if (isHintPosSwap)
            {
                boardManager.HideHint(boardManager.CurHintPositions);
                boardManager.StopCoroutine(_hintCoroutine);
                _hintCoroutine = null;
                boardManager.CurHintPositions.Clear();
            }

            bool swapSuccess = false;
            yield return boardManager.StartCoroutine(
                boardManager.BlockMover.TrySwap(boardManager, result => swapSuccess = result)
            );

            if (swapSuccess)
            {
                // 스왑 위치를 기록하고 매칭 상태로 전환
                boardManager.InitialSwapPosition = boardManager.BlockMover.EndBlockPos;
                boardManager.ChangeState(new MatchingState());
            }
            else
            {
                // 스왑에 실패하면 다시 입력 가능 상태로
                _isSwapping = false;
            }
        }

        private void UserInput(BoardManager boardManager)
        {
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

        private void BlockCheck(BoardManager boardManager)
        {
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
        }

        private void StartHint()
        {
            if (_hintCoroutine == null)
            {
                _hintCoroutine = BoardManager.Instance.StartCoroutine(HintDelayCoroutine(BoardManager.Instance));
            }
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

        private IEnumerator HintDelayCoroutine(BoardManager boardManager)
        {
            Debug.Log("힌트 대기중");
            yield return new WaitForSeconds(3f);
            Debug.Log("힌트 표시");
            List<Vector2Int> hintPositions = boardManager.MatchChecker.OptimalMatchFind();
            boardManager.CurHintPositions = hintPositions;
            boardManager.ShowHint(hintPositions);
            yield return new WaitForSeconds(3f);
            Debug.Log("힌트 숨김");
            boardManager.HideHint(hintPositions);
            _hintCoroutine = null;
        }
    }
}