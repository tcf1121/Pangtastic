using UnityEngine;

namespace SCR
{
    public class Ice : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
            _currentHP = 1;
            _score = 0;
            _canMove = false;
            _onCollider = true;
            _isSpawn = false;
        }
    }
}
