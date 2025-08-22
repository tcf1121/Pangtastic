using UnityEngine;

namespace SCR
{
    public class Egg : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            _currentHP = 2;
            _score = 10;
            _canMove = true;
            _onCollider = true;
            _isSpawn = false;
            _blockType = GemType.Egg;
            base.Init(cell);
        }

        public override void Clear()
        {
            base.Clear();
            //AddIngredient();
        }
    }
}
