using KDJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class RollerH : SpecialBlock
    {
        [SerializeField] private bool _destroySpecial; // 특수블럭도 함께 제거할지 옵션

        public override void Activate(BoardManager board)
        {
            if (board == null || board.Spawner == null) return;

            var spawner = board.Spawner;
            var plate = spawner.GetComponent<BlockPlate>();
            int width = plate.BlockPlateWidth;
            int height = plate.BlockPlateHeight;

            // 내 보드 좌표 찾기
            int myX = Mathf.RoundToInt(transform.position.x + width / 2f - 0.5f);
            int myY = Mathf.RoundToInt(transform.position.y + height / 2f - 0.5f);

            Debug.Log($" ({myX},{myY}) 가로줄 제거");

            // 가로줄 전부 제거
            //for (int x = 0; x < width; x++)
            //{
            //    var block = spawner.GetArray()[myY, x];
            //    if (block != null && block.BlockInstance != null)
            //    {
            //        Object.Destroy(block.BlockInstance);
            //    }
            //}

            // 정리 및 낙하/리필
            spawner.CheckBlockArray();
            spawner.SortBlockArray();
            spawner.SpawnBlock();
        }
    }
}
