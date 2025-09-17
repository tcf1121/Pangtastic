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
        public bool IsCoordMoved { get; private set; }

        private const float SWAP_DURATION = 0.1f;
        private int _width = BoardManager.Instance != null ? BoardManager.Instance.Spawner.GameBoardData.Width : 0;
        private int _height = BoardManager.Instance != null ? BoardManager.Instance.Spawner.GameBoardData.Height : 0;

        public void ResetCoordMoved()
        {
            IsCoordMoved = false;
            StartPos = Vector2.zero;
            EndPos = Vector2.zero;
            StartBlockPos = Vector2Int.zero;
            EndBlockPos = Vector2Int.zero;
        }

        public void SetStartPos(Vector2 inputPos)
        {
            StartPos = inputPos;
            StartBlockPos = WorldToGrid(StartPos, _width, _height);
        }

        public void UpdateCoord(Vector2 inputPos)
        {
            EndPos = inputPos;
            EndBlockPos = WorldToGrid(EndPos, _width, _height);
            if (StartPos != Vector2.zero && EndPos != Vector2.zero && StartBlockPos != EndBlockPos)
            {
                IsCoordMoved = true;
            }
        }

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

        public bool ValidateAndSetPositions(BoardManager boardManager)
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
