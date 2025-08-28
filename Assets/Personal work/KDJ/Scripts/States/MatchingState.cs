using LHJ;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace KDJ.States
{
    public class MatchingState : IGameState
    {
        private Coroutine _matchingCoroutine;

        public void OnEnter(BoardManager boardManager)
        {
            // 블럭 매칭 체크
            Debug.Log("블럭 매칭 상태");
            // boardManager.MatchChecker.BlockMatchCheck(boardManager.BlockMover.EndBlockPos, boardManager);
            if (_matchingCoroutine == null)
            {
                _matchingCoroutine = boardManager.StartCoroutine(MatchingCoroutine(boardManager));
            }
        }

        public void OnUpdate(BoardManager boardManager)
        {

        }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("블럭 매칭 상태 종료");
            boardManager.BlockMover.StartPos = Vector2.zero; // 초기 시작 위치 설정
            boardManager.BlockMover.EndPos = Vector2.zero; // 초기 종료 위치 설정
        }

        private IEnumerator MatchingCoroutine(BoardManager boardManager)
        {
            SpecialBlock startSpecialBlock = null;
            SpecialBlock endSpecialBlock = null;
            Vector2 SPos = boardManager.BlockMover.StartPos;
            Vector2 EPos = boardManager.BlockMover.EndPos;

            Debug.Log("블럭 매칭 시작");
            yield return new WaitForSeconds(0.5f);

            if (SPos.x < 0 || SPos.y < 0 || SPos.x >= boardManager.Spawner.BlockPlate.BlockPlateWidth || SPos.y >= boardManager.Spawner.BlockPlate.BlockPlateHeight ||
                EPos.x < 0 || EPos.y < 0 || EPos.x >= boardManager.Spawner.BlockPlate.BlockPlateWidth || EPos.y >= boardManager.Spawner.BlockPlate.BlockPlateHeight)
            {
                Debug.Log("StartPos 또는 EndPos가 보드 영역을 벗어났습니다.");
                boardManager.ChangeState(new ReadyState());
                yield break;
            }

            if (boardManager.Spawner.BlockPlate.BlockPlateArray[(int)SPos.y, (int)SPos.x] &&
                boardManager.Spawner.BlockPlate.BlockPlateArray[(int)EPos.y, (int)EPos.x])
            {
                bool isStartSpecialBlock = boardManager.Spawner.BlockArray[(int)SPos.y, (int)SPos.x].BlockInstance.TryGetComponent<SpecialBlock>(out startSpecialBlock);
                bool isEndSpecialBlock = boardManager.Spawner.BlockArray[(int)EPos.y, (int)EPos.x].BlockInstance.TryGetComponent<SpecialBlock>(out endSpecialBlock);
                if (isStartSpecialBlock || isEndSpecialBlock)
                {
                    startSpecialBlock?.Activate(boardManager);
                    endSpecialBlock?.Activate(boardManager);
                }
            }

            if (SPos != Vector2.zero)
            {
                boardManager.MatchChecker.BlockMatchCheck(boardManager.BlockMover.StartBlockPos, boardManager);
                boardManager.MatchChecker.BlockMatchCheck(boardManager.BlockMover.EndBlockPos, boardManager);
                boardManager.Spawner.CheckBlockArray(boardManager);

                if (boardManager.Spawner.HasEmptyBlocks())
                {
                    // 빈 블록이 있을경우 먼저 이동이 가능한 상태인지 확인
                    if (boardManager.Spawner.CanBlockMoveInArray())
                    {
                        Debug.Log("이동 가능");
                        // 이동이 가능하다면 RefillState로 이동
                        boardManager.ChangeState(new RefillState());
                    }
                    else
                    {
                        Debug.Log("이동 불가능");
                        // 이동이 불가능하다면 블록을 원래 위치로 되돌림
                        boardManager.BlockMover.ReturnBlock(boardManager);
                        boardManager.ChangeState(new ReadyState());
                    }
                }
                else
                {
                    Debug.Log("모든 블록이 매치되지 않았습니다.");
                    // 모든 블록이 매치되지 않았을 경우
                    boardManager.BlockMover.ReturnBlock(boardManager);
                    boardManager.ChangeState(new ReadyState());
                }
            }
            else if (SPos == Vector2.zero)
            {
                // 모든 매치된 블럭을 파괴
                boardManager.MatchChecker.AllMatchBlockDestroy(boardManager);
                boardManager.ChangeState(new RefillState());
            }

            _matchingCoroutine = null;

        }
    }
}
