using UnityEngine;

namespace SCR
{
    public class Egg : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
            _currentHP = 2;
            _score = 10;
            _canMove = true;
            _onCollider = true;
            _isSpawn = false;
        }

        public override void Clear()
        {
            base.Clear();
            //AddIngredient();
        }
    }
}
