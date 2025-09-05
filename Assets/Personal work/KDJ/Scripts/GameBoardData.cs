using UnityEngine;
using SCR;

namespace KDJ
{
    [System.Serializable]
    public class GameBoardData
    {
        public Block[,] BlockArray { get; private set; }
        public Block[,] OverlayArray { get; set; }
        public GameObject[,] BlockMask { get; set; }
        public BlockPlate BlockPlate { get; private set; }

        public int Width => BlockPlate.BlockPlateWidth;
        public int Height => BlockPlate.BlockPlateHeight;

        /// <summary>
        /// BlockPlate를 기반으로 새로운 보드 데이터를 생성합니다.
        /// </summary>
        public GameBoardData(BlockPlate blockPlate)
        {
            this.BlockPlate = blockPlate;
            this.OverlayArray = new Block[Height, Width];
            // 보이지 않는 생성 행을 위해 높이에 +1을 합니다.
            this.BlockArray = new Block[Height + 1, Width];
            this.BlockMask = new GameObject[Height, Width];
        }

        /// <summary>
        /// 지정된 좌표의 블록을 가져옵니다.
        /// </summary>
        public Block GetBlock(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height + 1)
            {
                return null;
            }
            return BlockArray[y, x];
        }

        /// <summary>
        /// 지정된 좌표에 블록을 설정합니다.
        /// </summary>
        public void SetBlock(int x, int y, Block block)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height + 1)
            {
                BlockArray[y, x] = block;
            }
        }

        /// <summary>
        /// 지정된 좌표의 오버레이 블록을 가져옵니다.
        /// </summary>
        public Block GetOverlayBlock(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return null;
            }
            return OverlayArray[y, x];
        }

        /// <summary>
        /// 지정된 좌표에 오버레이 블록을 설정합니다.
        /// </summary>
        public void SetOverlayBlock(int x, int y, Block block)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                OverlayArray[y, x] = block;
            }
        }
    }
}
