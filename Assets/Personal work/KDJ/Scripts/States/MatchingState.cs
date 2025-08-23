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
            Debug.Log("블럭 매칭 시작");
            yield return new WaitForSeconds(0.5f);
            bool StartBlockIsSpecialBlock = boardManager.Spawner.BlockArray[(int)boardManager.BlockMover.StartPos.y, (int)boardManager.BlockMover.StartPos.x].BlockInstance.TryGetComponent<SpecialBlock>(out startSpecialBlock);
            bool EndBlockIsSpecialBlock = boardManager.Spawner.BlockArray[(int)boardManager.BlockMover.EndPos.y, (int)boardManager.BlockMover.EndPos.x].BlockInstance.TryGetComponent<SpecialBlock>(out endSpecialBlock);
            if (StartBlockIsSpecialBlock || EndBlockIsSpecialBlock)
            {
                startSpecialBlock?.Activate(boardManager);
                endSpecialBlock?.Activate(boardManager);
                boardManager.ChangeState(new RefillState());
                yield break;
            }

            if (boardManager.BlockMover.StartPos != Vector2.zero)
            {
                boardManager.MatchChecker.BlockMatchCheck(boardManager.BlockMover.StartBlockPos, boardManager);
                boardManager.MatchChecker.BlockMatchCheck(boardManager.BlockMover.EndBlockPos, boardManager);

                if (boardManager.Spawner.HasEmptyBlockObjects())
                {
                    // 빈 블럭이 있으면 RefillState로 전환
                    boardManager.ChangeState(new RefillState());
                }
                else
                {
                    // 빈 블럭이 없으면 블록을 원상 복귀 한 뒤 ReadyState로 이동
                    boardManager.BlockMover.ReturnBlock(boardManager);
                    boardManager.ChangeState(new ReadyState());
                }
            }
            else if (boardManager.BlockMover.StartPos == Vector2.zero)
            {
                // 모든 매치된 블럭을 파괴
                boardManager.MatchChecker.AllMatchBlockDestroy(boardManager);
                boardManager.ChangeState(new RefillState());
            }

            _matchingCoroutine = null;

        }
    }
}
