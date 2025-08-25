using KDJ;
using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class DonutBox : SpecialBlock
    {
        [SerializeField] private bool _destroySpecial;

        public override void Activate(BoardManager board)
        {
            if (board == null || board.Spawner == null) return;

            var spawner = board.Spawner;
            var plate = spawner.BlockPlate;
            int width = plate.BlockPlateWidth;
            int height = plate.BlockPlateHeight;

            Vector2Int center = board.BlockMover.StartBlockPos;
            if (spawner.BlockArray[center.y, center.x].BlockInstance != this.gameObject)
            {
                center = board.BlockMover.EndBlockPos;
            }

            for (int y = center.y - 2; y <= center.y + 2; y++)
            {
                for (int x = center.x - 2; x <= center.x + 2; x++)
                {
                    if (y < 0 || y >= height || x < 0 || x >= width)
                        continue;

                    var blk = spawner.BlockArray[y, x];
                    if (blk == null || blk.BlockInstance == null) continue;

                    if (_destroySpecial)
                    {
                        var special = blk.BlockInstance.GetComponent<SpecialBlock>();
                        if (special != null && blk.BlockInstance != this.gameObject)
                            continue;
                    }

                    Object.Destroy(blk.BlockInstance);
                    spawner.BlockArray[y, x].BlockInstance = null;
                }
            }
        }
    }
}