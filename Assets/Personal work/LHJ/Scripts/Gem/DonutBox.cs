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
            if (SpecialBlockCombo.Instance != null &&
                SpecialBlockCombo.Instance.TryResolveFromActivate(board, this.gameObject))
            {
                return;
            }
            var spawner = board.Spawner;
            var plate = spawner.GameBoardData.BlockPlate;
            int width = plate.BlockPlateWidth;
            int height = plate.BlockPlateHeight;
            int destroyedCount = 0;

            Vector2Int center = board.BlockMover.StartBlockPos;
            if (spawner.GameBoardData.BlockArray[center.y, center.x].BlockInstance != this.gameObject)
            {
                center = board.BlockMover.EndBlockPos;
            }

            for (int y = center.y - 2; y <= center.y + 2; y++)
            {
                for (int x = center.x - 2; x <= center.x + 2; x++)
                {
                    if (y < 0 || y >= height || x < 0 || x >= width)
                        continue;

                    var blk = spawner.GameBoardData.BlockArray[y, x];
                    if (blk == null || blk.BlockInstance == null) continue;

                    var special = blk.BlockInstance.GetComponent<SpecialBlock>();
                    if (special != null && blk.BlockInstance != this.gameObject)
                    {
                        if (!_destroySpecial) continue;
                        special.Activate(board);
                    }
                    if (blk is ObstacleBlock obstacle)
                    {
                        obstacle.TakeDamage();
                        continue;
                    }

                    Object.Destroy(blk.BlockInstance);
                    spawner.GameBoardData.BlockArray[y, x].BlockInstance = null;
                    destroyedCount++;
                }
            }
            if (destroyedCount > 0)
            {
                int score = destroyedCount * 10;
                //board.UpdateUI(score);
            }
            if (board.MatchCombo != null)
            {
                board.MatchCombo.ResetTimer();
            }
        }
    }
}