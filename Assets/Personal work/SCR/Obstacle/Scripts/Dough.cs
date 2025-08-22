using UnityEngine;

namespace SCR
{
    public class Dough : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            _currentHP = 2;
            _score = 0;
            _canMove = false;
            _onCollider = false;
            _isSpawn = false;
            _isSplashDamage = false;
            _blockType = GemType.Dough;
            base.Init(cell);
        }
    }
}
