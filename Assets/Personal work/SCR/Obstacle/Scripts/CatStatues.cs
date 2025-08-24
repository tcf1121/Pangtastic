using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class CatStatues : Obstacle
    {



        public override void Init(Vector3Int cell)
        {
            _fourCellPos = new()
            {
                new Vector3Int(cell.x + 1, cell.y),
                new Vector3Int(cell.x + 1, cell.y + 1),
                new Vector3Int(cell.x, cell.y + 1)
            };
            _currentHP = 2;
            _score = 0;
            _canMove = false;
            _onCollider = true;
            _isSpawn = false;
            _isSplashDamage = true;
            ObstaclType = GemType.CatStatues;


            base.Init(cell);
            Board.AddObject(_fourCellPos[0], GemType.CatStatues_s);
            Board.AddObject(_fourCellPos[1], GemType.CatStatues_s);
            Board.AddObject(_fourCellPos[2], GemType.CatStatues_s);
        }
    }
}
