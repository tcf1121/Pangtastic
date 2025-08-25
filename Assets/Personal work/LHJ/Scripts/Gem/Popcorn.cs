using KDJ;
using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popcorn : SpecialBlock
{
    [SerializeField] private bool _destroySpecial;

    public override void Activate(BoardManager board)
    {
        if (board == null || board.Spawner == null) return;

        var spawner = board.Spawner;
        var plate = spawner.BlockPlate;
        int width = plate.BlockPlateWidth;
        int height = plate.BlockPlateHeight;

        Vector2Int myPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x + width / 2f - 0.5f),
            Mathf.RoundToInt(transform.position.y + height / 2f - 0.5f)
        );

        Vector2Int endPos = board.BlockMover.EndBlockPos;
        Vector2Int startPos = board.BlockMover.StartBlockPos;

        Vector2Int targetPos = (myPos == startPos) ? endPos : startPos;
        var targetBlock = spawner.BlockArray[targetPos.y, targetPos.x];
        if (targetBlock == null) return;

        var targetType = targetBlock.GemType;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var blk = spawner.BlockArray[y, x];
                if (blk == null || blk.BlockInstance == null) continue;

                if (blk.GemType != targetType && blk.BlockInstance != this.gameObject)
                    continue;

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
