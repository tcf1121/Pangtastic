using UnityEngine;

namespace SCR
{
    public class Syrup : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            base.Init(cell);
            _currentHP = 1;
            _score = 10;
            _canMove = false;
            _onCollider = false;
            _isSpawn = false;
            _isSplashDamage = false;
        }

        public override void Clear()
        {
            base.Clear();
            //AddIngredient()
        }
    }
}
