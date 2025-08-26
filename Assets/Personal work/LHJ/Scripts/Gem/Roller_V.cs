using KDJ.States;
using KDJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class Roller_V : SpecialBlock
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
            var plate = spawner.BlockPlate;
            int width = plate.BlockPlateWidth;
            int height = plate.BlockPlateHeight;
            int destroyedCount = 0;

            int myX = Mathf.RoundToInt(transform.position.x + width / 2f - 0.5f);
            int myY = Mathf.RoundToInt(transform.position.y + height / 2f - 0.5f);

            for (int y = 0; y < height; y++)
            {
                var blk = spawner.BlockArray[y, myX];
                if (blk == null || blk.BlockInstance == null) continue;

                var special = blk.BlockInstance.GetComponent<SpecialBlock>();
                if (special != null && blk.BlockInstance != this.gameObject)
                {
                    if (!_destroySpecial) continue;
                    special.Activate(board);
                }

                Object.Destroy(blk.BlockInstance);
                spawner.BlockArray[y, myX].BlockInstance = null;
                destroyedCount ++;
            }
            if (destroyedCount > 0)
            {
                int score = destroyedCount * 10;
                board.UpdateUI(score);
            }
        }
    }
}
