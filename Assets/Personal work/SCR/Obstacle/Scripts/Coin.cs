using UnityEngine;

namespace SCR
{
    public class Coin : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
            _currentHP = 1;
            _score = 0;
            _canMove = true;
            _onCollider = true;
            _isSpawn = true;
            _isSplashDamage = true;
        }
    }
}
