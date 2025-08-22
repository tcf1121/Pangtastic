using UnityEngine;

namespace SCR
{
    public class Cloche : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            _currentHP = 1;
            _score = 0;
            _canMove = false;
            _onCollider = true;
            _isSpawn = false;
            _isSplashDamage = true;
            _blockType = GemType.Cloche;
            base.Init(cell);
        }

        public override void Clear()
        {
            base.Clear();
            //젤리 생성
        }

    }
}
