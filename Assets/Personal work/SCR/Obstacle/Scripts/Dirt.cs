using UnityEngine;

namespace SCR
{
    public class Dirt : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
            _currentHP = 2;
            _score = 0;
            _canMove = false;
            _onCollider = false;
            _isSpawn = false;
            _isSplashDamage = false;
        }
    }
}
