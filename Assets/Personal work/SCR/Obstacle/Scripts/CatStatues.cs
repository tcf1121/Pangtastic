using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class CatStatues : Obstacle
    {

        [SerializeField] private List<Vector3Int> _fourCellPos = new();

        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
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
            Board.AddGem(_fourCellPos[0], _blockType);
            Board.AddGem(_fourCellPos[1], _blockType);
            Board.AddGem(_fourCellPos[2], _blockType);
        }
    }
}
