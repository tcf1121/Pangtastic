using UnityEngine;

namespace SCR
{
    public class GiftBox : Obstacle
    {
        public override void Init(Vector3Int cell)
        {
            _currentHP = 4;
            _score = 0;
            _canMove = true;
            _onCollider = true;
            _isSpawn = true;
            _isSplashDamage = true;
            _blockType = GemType.GiftBox;
            base.Init(cell);
        }

        public override void Clear()
        {
            base.Clear();
            // 랜덤 재료 생성
            // 확률: 커피(30%), 가로 중식도(15%), 세로 중식도(15%), 우유(20%), 과일바구니(10%), 벌꿀집(5%)
            int random = Random.Range(0, 100);
            if (random < 30)
            {
                // 커피 생성
            }
            else if (random < 45)
            {
                // 가로 중식도 생성
            }
            else if (random < 60)
            {
                // 세로 중식도 생성
            }
            else if (random < 80)
            {
                // 우유 생성
            }
            else if (random < 90)
            {
                // 과일바구니 생성
            }
            else if (random < 65)
            {
                // 벌꿀집 생성
            }
        }
    }
}
