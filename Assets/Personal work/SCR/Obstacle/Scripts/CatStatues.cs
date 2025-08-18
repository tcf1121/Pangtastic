using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class CatStatues : Obstacle
    {

        private List<Vector3Int> _fourCellPos = new();

        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
            _fourCellPos.Add(new Vector3Int(cell.x + 1, cell.y));
            _fourCellPos.Add(new Vector3Int(cell.x + 1, cell.y - 1));
            _fourCellPos.Add(new Vector3Int(cell.x, cell.y - 1));
            _currentHP = 2;
            _score = 0;
            _canMove = false;
            _onCollider = true;
            _isSpawn = false;
        }
    }
}
