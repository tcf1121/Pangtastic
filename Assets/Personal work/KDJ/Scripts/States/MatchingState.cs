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
            if (_matchingCoroutine != null)
            {
                boardManager.StopCoroutine(_matchingCoroutine);
            }
            _matchingCoroutine = boardManager.StartCoroutine(MatchingCoroutine(boardManager));
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

            if (wasSwap)
            {
                var startBlock = boardManager.Spawner.GameBoardData.GetBlock(startPos.x, startPos.y);
                var endBlock = boardManager.Spawner.GameBoardData.GetBlock(endPos.x, endPos.y);

                bool startIsSpecial = startBlock != null && startBlock.GemType > GemType.Sugar && startBlock.GemType < GemType.Dust;
                bool endIsSpecial = endBlock != null && endBlock.GemType > GemType.Sugar && endBlock.GemType < GemType.Dust;

                if (startIsSpecial || endIsSpecial)
                {
                    var effect = boardManager.GetComponent<SpecialBlockEffect>(); // BoardManager 오브젝트에 붙어있음
                    var gameBoard = boardManager.Spawner.GameBoardData;           // 보드 데이터 접근
                    var hits = new List<Vector2Int>();

                    if (effect != null && gameBoard != null)
                    {
                        if (startIsSpecial && endIsSpecial)
                        {
                            effect.UseCombo(startPos, endPos, startBlock.GemType, endBlock.GemType, gameBoard, hits);
                            usedSpecial = true;
                        }
                        else if (startIsSpecial)
                        {
                            effect.UseSpecial(startPos, startBlock.GemType, gameBoard, hits);
                            usedSpecial = true;
                        }
                        else if (endIsSpecial)
                        {
                            effect.UseSpecial(endPos, endBlock.GemType, gameBoard, hits);
                            usedSpecial = true;
                        }

                        // 공통 처리: 파괴/점수/콤보
                        int destroyed = effect.ApplyDamageAndScore(boardManager, hits);
                        // usedSpecial = destroyed > 0;
                    }
                }
            }

            // 2. 매치 처리
            var (coordsToDestroy, specialToCreate, specialSpawnPos) = boardManager.MatchChecker.ProcessMatches(boardManager, boardManager.InitialSwapPosition);

            // 3. 후속 처리
            if (coordsToDestroy.Count > 0)
            {
                // 매치가 발생 시 힌트 숨기기
                boardManager.HintManager.StopHintTimer();
                boardManager.CurHintPositions.Clear();

                // 애니메이션 후 리필 상태로 이동
                boardManager.BlockMover.ResetCoordMoved();
                yield return boardManager.StartCoroutine(boardManager.AnimateAndDestroyMatches(coordsToDestroy, specialToCreate, specialSpawnPos, boardManager.InitialSwapPosition));
            }
            else if (usedSpecial)
            {
                // 특수 블록만 사용되었으면 refill 상태로 이동
                Debug.Log("특수 블록 사용");
                boardManager.HintManager.StopHintTimer();
                boardManager.BlockMover.ResetCoordMoved();
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