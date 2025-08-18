using UnityEngine;

namespace SCR
{
    public class GiftBox : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
            _currentHP = 4;
            _score = 0;
            _canMove = true;
            _onCollider = true;
            _isSpawn = true;
        }
    }
}
