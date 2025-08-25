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
            ObstaclType = GemType.Cloche;
            base.Init(cell);
        }

        public override void Clear()
        {
            base.Clear();
            Board.AddObject(_cellPos, GemType.Random);
            //젤리 생성
        }

    }
}
