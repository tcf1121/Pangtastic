using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class CatStatues : Obstacle
    {



        public override void Init(Vector3Int cell)
        {
            _currentHP = 2;
            _score = 0;
            _canMove = false;
            _onCollider = true;
            _isSpawn = false;
            _isSplashDamage = true;
            ObstaclType = GemType.CatStatues;
            base.Init(cell);
        }
    }
}
