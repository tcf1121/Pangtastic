using UnityEngine;

namespace KDJ.States
{
    public class ReadyState : IGameState
    {
        public void OnEnter(BoardManager boardManager)
        {
            Debug.Log("입력 준비 상태");
            boardManager.BlockMover.StartPos = Vector2.zero; // 초기 시작 위치 설정
            boardManager.BlockMover.EndPos = Vector2.zero; // 초기 종료 위치 설정

            // 매칭되는 블럭이 있을 경우 매칭 상태로 전환
            if (boardManager.MatchChecker.AllBlockMatchCheck(boardManager))
            {
                boardManager.ChangeState(new MatchingState());
            }
        }

        public void OnUpdate(BoardManager boardManager)
        {
            if (boardManager.Spawner.HasEmptyBlockObjects())
            {
                // 빈 블럭이 있으면 RefillState로 전환
                boardManager.ChangeState(new RefillState());
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("마우스 클릭 감지");
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
                    // 마우스 버튼을 떼면 EndPos를 설정하고 블록 이동 시작
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = -Camera.main.transform.position.z;
                    boardManager.BlockMover.EndPos = Camera.main.ScreenToWorldPoint(mousePosition);
                    if (boardManager.BlockMover.MoveBlock(boardManager))
                    {
                        boardManager.ChangeState(new MatchingState());
                    }
                }
                else
                {
                    boardManager.BlockMover.StartPos = Vector2.zero;
                    boardManager.BlockMover.EndPos = Vector2.zero;
                }

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
            targetPos += new Vector2(boardManager.Spawner.BlockPlate.BlockPlateWidth / 2, boardManager.Spawner.BlockPlate.BlockPlateHeight / 2);

            if (targetPos.x < 0 || targetPos.y < 0)
            {
                return;
            }

            Vector2Int gridPos = boardManager.BlockMover.WorldToGrid(targetPos);

            if (gridPos.x >= boardManager.Spawner.BlockPlate.BlockPlateWidth || gridPos.y >= boardManager.Spawner.BlockPlate.BlockPlateHeight)
            {
                return;
            }

            Block block = boardManager.Spawner.BlockArray[gridPos.y, gridPos.x];
            if (block != null)
            {
                boardManager.UpdateUI(block);
            }
        }
    }
}
