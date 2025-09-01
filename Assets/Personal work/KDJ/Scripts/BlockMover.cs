using KDJ;
using SCR;
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

        /// <summary>
        /// 블록 스왑을 시도하고, 성공 시 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="boardManager">게임 보드 관리자</param>
        /// <param name="onResult">스왑 성공 여부를 비동기적으로 전달받는 콜백 함수</param>
        public IEnumerator TrySwap(BoardManager boardManager, Action<bool> onResult)
        {
            if (!ValidateAndSetPositions(boardManager))
            {
                onResult?.Invoke(false);
                yield break;
            }

            Block blockA = boardManager.Spawner.BlockArray[StartBlockPos.y, StartBlockPos.x];
            Block blockB = boardManager.Spawner.BlockArray[EndBlockPos.y, EndBlockPos.x];

            // 애니메이션 재생
            yield return StartCoroutine(AnimateSwapCoroutine(blockA, blockB));

            // 데이터 스왑
            SwapBlockData(boardManager, StartBlockPos, EndBlockPos);

            onResult?.Invoke(true);
        }

        /// <summary>
        /// 매치 실패 시 블록을 원래 위치로 되돌리는 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="boardManager">게임 보드 관리자</param>
        public IEnumerator ReturnBlock(BoardManager boardManager)
        {
            Block blockA = boardManager.Spawner.BlockArray[EndBlockPos.y, EndBlockPos.x];
            Block blockB = boardManager.Spawner.BlockArray[StartBlockPos.y, StartBlockPos.x];

            // 애니메이션 재생
            yield return StartCoroutine(AnimateSwapCoroutine(blockA, blockB));

            // 데이터 스왑
            SwapBlockData(boardManager, StartBlockPos, EndBlockPos);
        }

        /// <summary>
        /// 두 블록의 위치를 부드럽게 바꾸는 애니메이션 코루틴
        /// </summary>
        private IEnumerator AnimateSwapCoroutine(Block blockA, Block blockB)
        {
            if (blockA == null || blockB == null || blockA.BlockInstance == null || blockB.BlockInstance == null)
            {
                yield break;
            }

            Vector3 startPosA = blockA.BlockInstance.transform.position;
            Vector3 startPosB = blockB.BlockInstance.transform.position;

            float elapsedTime = 0f;

            while (elapsedTime < SWAP_DURATION)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / SWAP_DURATION);

                blockA.BlockInstance.transform.position = Vector3.Lerp(startPosA, startPosB, t);
                blockB.BlockInstance.transform.position = Vector3.Lerp(startPosB, startPosA, t);

                yield return null; // 다음 프레임까지 대기
            }

            // 최종 위치를 정확하게 설정
            blockA.BlockInstance.transform.position = startPosB;
            blockB.BlockInstance.transform.position = startPosA;
        }

        /// <summary>
        /// 입력 좌표의 유효성을 검사하고 스왑 위치를 설정합니다.
        /// </summary>
        private bool ValidateAndSetPositions(BoardManager boardManager)
        {
            var blockPlate = boardManager.Spawner.BlockPlate;
            var blockArray = boardManager.Spawner.BlockArray;

            if (StartPos == Vector2.zero || EndPos == Vector2.zero) return false;

            Vector2Int startGrid = WorldToGrid(StartPos, blockPlate.BlockPlateWidth, blockPlate.BlockPlateHeight);
            Vector2Int endGrid = WorldToGrid(EndPos, blockPlate.BlockPlateWidth, blockPlate.BlockPlateHeight);

            if (startGrid.x != endGrid.x && startGrid.y != endGrid.y) return false;
            if (startGrid.x == endGrid.x && startGrid.y == endGrid.y) return false;

            Vector2 direction = endGrid - startGrid;
            direction.Normalize();
            Vector2Int swapEndGrid = startGrid + new Vector2Int((int)direction.x, (int)direction.y);

            if (!IsOnBoard(startGrid, blockPlate) || !IsOnBoard(swapEndGrid, blockPlate))
            {
                return false;
            }

            if (blockArray[startGrid.y, startGrid.x] == null || blockArray[startGrid.y, startGrid.x].IsObstacle ||
                blockArray[swapEndGrid.y, swapEndGrid.x] == null || blockArray[swapEndGrid.y, swapEndGrid.x].IsObstacle)
            {
                return false;
            }

            StartBlockPos = startGrid;
            EndBlockPos = swapEndGrid;
            return true;
        }

        /// <summary>
        /// 두 위치의 블록 데이터를 스왑합니다.
        /// </summary>
        private void SwapBlockData(BoardManager boardManager, Vector2Int posA, Vector2Int posB)
        {
            var blockArray = boardManager.Spawner.BlockArray;
            Block tempBlock = blockArray[posA.y, posA.x];
            blockArray[posA.y, posA.x] = blockArray[posB.y, posB.x];
            blockArray[posB.y, posB.x] = tempBlock;
        }

        /// <summary>
        /// 해당 그리드 좌표가 보드 위에 있고, 블록이 놓일 수 있는 판인지 확인합니다.
        /// </summary>
        private bool IsOnBoard(Vector2Int gridPos, BlockPlate blockPlate)
        {
            if (gridPos.x < 0 || gridPos.y < 0 || gridPos.x >= blockPlate.BlockPlateWidth || gridPos.y >= blockPlate.BlockPlateHeight)
            {
                return false;
            }
            return blockPlate.BlockPlateArray[gridPos.y, gridPos.x];
        }

        /// <summary>
        /// 월드 좌표를 그리드 좌표로 변환합니다. 보드 중앙을 (0,0)으로 가정합니다.
        /// </summary>
        /// <param name="worldPosition">변환할 월드 좌표</param>
        /// <param name="plateWidth">보드 너비</param>
        /// <param name="plateHeight">보드 높이</param>
        /// <returns>변환된 그리드 좌표</returns>
        public Vector2Int WorldToGrid(Vector2 worldPosition, int plateWidth, int plateHeight)
        {
            float centeredX = worldPosition.x + plateWidth / 2.0f;
            float centeredY = worldPosition.y + plateHeight / 2.0f;
            return new Vector2Int(Mathf.FloorToInt(centeredX), Mathf.FloorToInt(centeredY));
        }

        /// <summary>
        /// 그리드 좌표를 월드 좌표로 변환합니다. 블록을 그리드 칸의 중앙에 위치시킵니다.
        /// </summary>
        /// <param name="gridPosition">변환할 그리드 좌표</param>
        /// <param name="plateWidth">보드 너비</param>
        /// <param name="plateHeight">보드 높이</param>
        /// <returns>변환된 월드 좌표</returns>
        public Vector3 GridToWorld(Vector2Int gridPosition, int plateWidth, int plateHeight)
        {
            float worldX = gridPosition.x - plateWidth / 2.0f + 0.5f;
            float worldY = gridPosition.y - plateHeight / 2.0f + 0.5f;
            return new Vector3(worldX, worldY, 0);
        }

        /// <summary>
        /// 모든 위치 정보를 초기화합니다.
        /// </summary>
        public void ResetPos()
        {
            StartPos = Vector2.zero;
            EndPos = Vector2.zero;
            StartBlockPos = Vector2Int.zero;
            EndBlockPos = Vector2Int.zero;
        }
    }
}