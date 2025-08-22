using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class CatStatues : Obstacle
    {



        public override void Init(Vector3Int cell)
        {

            _fourCellPos.Add(new Vector3Int(cell.x + 1, cell.y));
            _fourCellPos.Add(new Vector3Int(cell.x + 1, cell.y + 1));
            _fourCellPos.Add(new Vector3Int(cell.x, cell.y + 1));
            _currentHP = 2;
            _score = 0;
            _canMove = false;
            _onCollider = true;
            _isSpawn = false;
            _isSplashDamage = true;
            transform.position = new Vector3(cell.x + 1, cell.y + 1, 0);
            _blockType = GemType.CatStatues;
            _fourCellPos = new();

            base.Init(cell);
            Board.SetCatStatues(cell);
        }
    }
}
