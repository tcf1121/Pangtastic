using System;
using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class BlockMover : MonoBehaviour
    {
        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }
        public Vector2Int StartBlockPos { get; private set; }
        public Vector2Int EndBlockPos { get; private set; }

        private const float SWAP_DURATION = 0.1f;


        // TODO: 터치 감도 개선 필요
        // LateUpdate 안에서 Input.GetTouch(0).phase == TouchPhase.Moved 인 경우에만 좌표 계산 및 스왑 로직을 처리하면,     
        // 손가락이 움직이지 않고 가만히 있을 때는 불필요한 계산을 줄여서 성능을 더 최적화할 수 있습니다.
        // 해당 부분을 잘 이용해서 성능 최적화.

        public IEnumerator TrySwap(BoardManager boardManager, Action<bool> onResult)
        {
            if (!ValidateAndSetPositions(boardManager))
            {
                onResult?.Invoke(false);
                yield break;
            }

            Block blockA = boardManager.Spawner.GameBoardData.GetBlock(StartBlockPos.x, StartBlockPos.y);
            Block blockB = boardManager.Spawner.GameBoardData.GetBlock(EndBlockPos.x, EndBlockPos.y);

            yield return StartCoroutine(AnimateSwapCoroutine(blockA, blockB));
            SwapBlockData(boardManager, StartBlockPos, EndBlockPos);
            onResult?.Invoke(true);
        }

        public IEnumerator ReturnBlock(BoardManager boardManager)
        {
            Block blockA = boardManager.Spawner.GameBoardData.GetBlock(EndBlockPos.x, EndBlockPos.y);
            Block blockB = boardManager.Spawner.GameBoardData.GetBlock(StartBlockPos.x, StartBlockPos.y);

            yield return StartCoroutine(AnimateSwapCoroutine(blockA, blockB));
            SwapBlockData(boardManager, StartBlockPos, EndBlockPos);
        }

        private IEnumerator AnimateSwapCoroutine(Block blockA, Block blockB)
        {
            if (blockA == null || blockB == null || blockA.BlockInstance == null || blockB.BlockInstance == null) yield break;

            Vector3 startPosA = blockA.BlockInstance.transform.position;
            Vector3 startPosB = blockB.BlockInstance.transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < SWAP_DURATION)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / SWAP_DURATION);
                blockA.BlockInstance.transform.position = Vector3.Lerp(startPosA, startPosB, t);
                blockB.BlockInstance.transform.position = Vector3.Lerp(startPosB, startPosA, t);
                yield return null;
            }

            blockA.BlockInstance.transform.position = startPosB;
            blockB.BlockInstance.transform.position = startPosA;
        }

        private bool ValidateAndSetPositions(BoardManager boardManager)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            if (gameBoard == null) return false;

            if (StartPos == Vector2.zero || EndPos == Vector2.zero) return false;

            Vector2Int startGrid = WorldToGrid(StartPos, gameBoard.Width, gameBoard.Height);
            Vector2Int endGrid = WorldToGrid(EndPos, gameBoard.Width, gameBoard.Height);

            if (startGrid.x != endGrid.x && startGrid.y != endGrid.y) return false;
            if (startGrid.x == endGrid.x && startGrid.y == endGrid.y) return false;

            Vector2 direction = endGrid - startGrid;
            direction.Normalize();
            Vector2Int swapEndGrid = startGrid + new Vector2Int((int)direction.x, (int)direction.y);

            if (!IsOnBoard(swapEndGrid, gameBoard) || !IsOnBoard(startGrid, gameBoard)) return false;

            Block startBlock = gameBoard.GetBlock(startGrid.x, startGrid.y);
            Block endBlock = gameBoard.GetBlock(swapEndGrid.x, swapEndGrid.y);

            if (startBlock == null || !startBlock.CanMove || startBlock.IsObstacle || endBlock == null || !endBlock.CanMove || endBlock.IsObstacle) return false;

            StartBlockPos = startGrid;
            EndBlockPos = swapEndGrid;
            return true;
        }

        private void SwapBlockData(BoardManager boardManager, Vector2Int posA, Vector2Int posB)
        {
            var gameBoard = boardManager.Spawner.GameBoardData;
            Block tempBlock = gameBoard.GetBlock(posA.x, posA.y);
            gameBoard.SetBlock(posA.x, posA.y, gameBoard.GetBlock(posB.x, posB.y));
            gameBoard.SetBlock(posB.x, posB.y, tempBlock);
        }

        private bool IsOnBoard(Vector2Int gridPos, GameBoardData gameBoard)
        {
            if (gridPos.x < 0 || gridPos.y < 0 || gridPos.x >= gameBoard.Width || gridPos.y >= gameBoard.Height) return false;
            return gameBoard.BlockPlate.BlockPlateArray[gridPos.y, gridPos.x];
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition, int plateWidth, int plateHeight)
        {
            float centeredX = worldPosition.x + plateWidth / 2.0f;
            float centeredY = worldPosition.y + plateHeight / 2.0f;
            return new Vector2Int(Mathf.FloorToInt(centeredX), Mathf.FloorToInt(centeredY));
        }

        public Vector3 GridToWorld(Vector2Int gridPosition, int plateWidth, int plateHeight)
        {
            float worldX = gridPosition.x - plateWidth / 2.0f + 0.5f;
            float worldY = gridPosition.y - plateHeight / 2.0f + 0.5f;
            return new Vector3(worldX, worldY, 0);
        }

        public void ResetPos()
        {
            StartPos = Vector2.zero;
            EndPos = Vector2.zero;
            StartBlockPos = Vector2Int.zero;
            EndBlockPos = Vector2Int.zero;
        }
    }
}
