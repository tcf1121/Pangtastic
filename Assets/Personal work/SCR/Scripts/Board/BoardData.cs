using SCR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCR_B
{
    public class BoardData
    {
        public Vector2Int ZeroPos;
        public Vector2Int Size;
        public bool[,] BlockPlateArray;
        public Block[,] BlockArray;
        public Block[,] OverlayArray;
        public List<Vector2Int> RespawnPos;
        private List<GemType> _respawnDonut = new();

        public void SetArray(int x, int y, GemType gemType)
        {
            if (gemType == GemType.Dust)
            {
                OverlayArray[y, x] = new Dust(x, y);
                int donutNum = Random.Range(0, 6);
                BlockArray[y, x] = new Donut(x, y, (GemType)donutNum);
            }
            else if (gemType == GemType.Syrup)
            {
                OverlayArray[y, x] = new Syrup(x, y);
                int donutNum = Random.Range(0, 6);
                BlockArray[y, x] = new Donut(x, y, (GemType)donutNum);
            }
            else if (gemType == GemType.Ice) BlockArray[y, x] = new Ice(x, y);
            else if (gemType == GemType.DonutBag) BlockArray[y, x] = new DonutBag(x, y);
            else if (gemType == GemType.Coin) BlockArray[y, x] = new Coin(x, y);
            else if (gemType == GemType.GiftBox) BlockArray[y, x] = new GiftBox(x, y);
            else if (gemType == GemType.Egg) BlockArray[y, x] = new Egg(x, y);
            else if (gemType == GemType.FlourBag) BlockArray[y, x] = new FlourBag(this, x, y);
            else if (gemType == GemType.Flour_s) { }
            else if (gemType == GemType.Random) BlockArray[y, x] = new Donut(x, y, (GemType)Random.Range(0, 6));
            else if (gemType == GemType.Milk) BlockArray[y, x] = new Milk(x, y);
            else if (gemType == GemType.Roller_v) BlockArray[y, x] = new Roller_v(x, y);
            else if (gemType == GemType.Roller_h) BlockArray[y, x] = new Roller_h(x, y);
            else if (gemType == GemType.DonutBox) BlockArray[y, x] = new DonutBox(x, y);
            else if (gemType == GemType.Oven) BlockArray[y, x] = new Oven(x, y);
            else BlockArray[y, x] = new Donut(x, y, gemType);
        }

        public void SetDonut(GemType gemType)
        {
            if (gemType == GemType.Random)
            {
                for (int i = 0; i < 6; i++)
                    if (!_respawnDonut.Contains((GemType)i))
                        _respawnDonut.Add((GemType)i);
            }
            else if (gemType == GemType.Lavender)
            {
                if (!_respawnDonut.Contains(GemType.Lavender))
                    _respawnDonut.Add(GemType.Lavender);
            }
            else if (gemType == GemType.Chocolate)
            {
                if (!_respawnDonut.Contains(GemType.Chocolate))
                    _respawnDonut.Add(GemType.Chocolate);
            }
            else if (gemType == GemType.Blueberry)
            {
                if (!_respawnDonut.Contains(GemType.Blueberry))
                    _respawnDonut.Add(GemType.Blueberry);
            }
            else if (gemType == GemType.Cheese)
            {
                if (!_respawnDonut.Contains(GemType.Cheese))
                    _respawnDonut.Add(GemType.Cheese);
            }
            else if (gemType == GemType.Strawberry)
            {
                if (!_respawnDonut.Contains(GemType.Strawberry))
                    _respawnDonut.Add(GemType.Strawberry);
            }
            else if (gemType == GemType.Sugar)
            {
                if (!_respawnDonut.Contains(GemType.Sugar))
                    _respawnDonut.Add(GemType.Sugar);
            }
            else if (gemType == GemType.Egg)
            {
                if (!_respawnDonut.Contains(GemType.Egg))
                    _respawnDonut.Add(GemType.Egg);
            }
            else if (gemType == GemType.Coin)
            {
                if (!_respawnDonut.Contains(GemType.Coin))
                    _respawnDonut.Add(GemType.Coin);
            }

        }

        public void DelArray(int x, int y)
        {
            BlockArray[y, x] = null;
        }

        public GemType RespawnDount()
        {
            var obstacleTypes = _respawnDonut.FindAll(v => v == GemType.Coin || v == GemType.Egg);
            if (obstacleTypes.Count > 0)
            {
                int num = Random.Range(0, 10);
                if (num < 1)
                {
                    return obstacleTypes[Random.Range(0, obstacleTypes.Count)];
                }
            }

            var normalTypes = _respawnDonut.FindAll(v => v < GemType.Milk);

            return normalTypes[Random.Range(0, normalTypes.Count)];
        }

        public int GetWidth()
        {
            return Size.x;
        }

        public int GetHeight()
        {
            return Size.y;
        }

        public List<Vector2Int> GetTargetPos(List<GemType> gemTypes)
        {
            List<Vector2Int> targetPos = new();

            List<Block> ingredentblocks = BlockArray.Cast<Block>().
            Where(data => data != null &&
            (data.GemType < GemType.Milk))
            .ToList();

            foreach (var gemType in gemTypes)
            {
                List<Vector2Int> blocks = GetGemTypePos(gemType);
                if (blocks.Count > 0)
                {
                    int cycle = 0;
                    while (cycle < 10)
                    {
                        int num = Random.Range(0, blocks.Count);
                        if (!targetPos.Contains(blocks[num]))
                        {
                            targetPos.Add(blocks[num]);
                            break;
                        }
                        cycle++;
                    }
                    if (cycle == 10)
                    {
                        while (true)
                        {
                            int num = Random.Range(0, ingredentblocks.Count);
                            if (!targetPos.Contains(ingredentblocks[num].Pos))
                            {
                                if (ingredentblocks[num].Pos.y < Size.y)
                                {
                                    targetPos.Add(ingredentblocks[num].Pos);
                                    break;
                                }

                            }
                        }
                    }
                }

            }

            return targetPos;
        }

        public List<Vector2Int> GetGemTypePos(GemType gemType)
        {
            List<Vector2Int> targetPos = new();
            List<Block> GemTypeblocks = new();
            if (gemType != GemType.Syrup && gemType != GemType.Dust)
                GemTypeblocks = BlockArray.Cast<Block>().
                Where(data => data != null && data.GemType == gemType)
                .ToList();
            else
                GemTypeblocks = OverlayArray.Cast<Block>().
                    Where(data => data != null && data.GemType == gemType)
                    .ToList();

            if (GemTypeblocks.Count > 0)
            {
                foreach (var b in GemTypeblocks)
                {
                    if (b.Pos.y < Size.y)
                        targetPos.Add(b.Pos);
                }

            }

            return targetPos;
        }

        public BoardData Clone()
        {
            var newBoardData = new BoardData
            {
                ZeroPos = this.ZeroPos,
                Size = this.Size,
                BlockPlateArray = new bool[this.BlockPlateArray.GetLength(0), this.BlockPlateArray.GetLength(1)],
                BlockArray = new Block[this.BlockArray.GetLength(0), this.BlockArray.GetLength(1)],
                OverlayArray = new Block[this.OverlayArray.GetLength(0), this.OverlayArray.GetLength(1)],
                RespawnPos = new List<Vector2Int>(this.RespawnPos),
                _respawnDonut = new List<GemType>(this._respawnDonut)
            };

            for (int y = 0; y < this.Size.y; y++)
            {
                for (int x = 0; x < this.Size.x; x++)
                {
                    newBoardData.BlockPlateArray[y, x] = this.BlockPlateArray[y, x];
                    if (this.BlockArray[y, x] != null)
                    {
                        newBoardData.BlockArray[y, x] = this.BlockArray[y, x].Clone();
                    }
                    if (this.OverlayArray[y, x] != null)
                    {
                        newBoardData.OverlayArray[y, x] = this.OverlayArray[y, x].Clone();
                    }
                }
            }

            return newBoardData;
        }
    }
}

