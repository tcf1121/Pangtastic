using LHJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KDJ.States
{
    public class MatchingState : IGameState
    {
        private Coroutine _matchingCoroutine;

        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("블럭 매칭 상태");
            if (_matchingCoroutine == null)
            {
                _matchingCoroutine = boardManager.StartCoroutine(MatchingCoroutine(boardManager));
            }
        }

        public void OnUpdate(BoardManager boardManager) { }

        public void OnExit(BoardManager boardManager)
        {
            Debug.Log("블럭 매칭 상태 종료");
            boardManager.BlockMover.ResetPos();
        }

        private IEnumerator MatchingCoroutine(BoardManager boardManager)
        {
            yield return new WaitForSeconds(boardManager.MatchDelay);

            Vector2Int startPos = boardManager.BlockMover.StartBlockPos;
            Vector2Int endPos = boardManager.BlockMover.EndBlockPos;
            bool wasSwap = startPos != endPos; // 스왑에 의한 매치인지 확인
            bool usedSpecial = false;

            // 1. 특수 블록 활성화 체크 (스왑 시에만)
            Debug.Log($"스왑 여부: {wasSwap}, 시작 위치: {startPos}, 종료 위치: {endPos}");
            if (wasSwap)
            {
                var startBlock = boardManager.Spawner.GameBoardData.GetBlock(startPos.x, startPos.y);
                var endBlock = boardManager.Spawner.GameBoardData.GetBlock(endPos.x, endPos.y);

                bool startIsSpecial = startBlock != null && startBlock.GemType > GemType.Sugar && startBlock.GemType < GemType.Dust;
                bool endIsSpecial = endBlock != null && endBlock.GemType > GemType.Sugar && endBlock.GemType < GemType.Dust;

                if (startIsSpecial || endIsSpecial)
                {
                    var effect = boardManager.GetComponent<SpecialBlockEffect>(); // BoardManager 오브젝트에 붙어있음
                    var gameBoard = boardManager.Spawner.GameBoardData;              // 보드 데이터 접근
                    var hits = new List<Vector2Int>();

                    if (effect != null && gameBoard != null)
                    {
                        if (startIsSpecial) effect.UseSpecial(startPos, startBlock.GemType, gameBoard, hits);
                        if (endIsSpecial) effect.UseSpecial(endPos, endBlock.GemType, gameBoard, hits);

                        // 한방에 파괴 + 점수 (+콤보타이머)
                        int destroyed = effect.ApplyDamageAndScore(boardManager, hits);
                        usedSpecial = destroyed > 0;
                    }
                }
            }

            // 2. 매치 처리
            bool matchFound = boardManager.MatchChecker.ProcessMatches(boardManager, wasSwap ? endPos : (Vector2Int?)null);

            // 3. 후속 처리
            if (matchFound)
            {
                // 매치가 발생했으면, 연쇄 반응을 위해 RefillState로 이동
                boardManager.ChangeState(new RefillState());
            }
            else if (usedSpecial)
            {
                // 특수 블록이 사용되었으면 refill 상태로 이동
                Debug.Log("특수 블록 사용");
                boardManager.ChangeState(new RefillState());
            }
            else if (wasSwap)
            {
                // 스왑으로 매치가 없었으면, 블록을 원위치
                Debug.Log("매치 실패, 블록 원위치");
                yield return boardManager.StartCoroutine(boardManager.BlockMover.ReturnBlock(boardManager));
                boardManager.ChangeState(new ReadyState());
            }
            else
            {
                // 연쇄 반응 확인 후 더 이상 매치가 없으면, ReadyState로 전환
                Debug.Log("연쇄 반응 종료");
                boardManager.ChangeState(new ReadyState());
            }

            _matchingCoroutine = null;
        }
    }
}