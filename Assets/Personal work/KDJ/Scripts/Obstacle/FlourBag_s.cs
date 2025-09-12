using UnityEngine;
using SCR;

namespace KDJ
{
    public class FlourBag_s : ObstacleBlock
    {
        public FlourBag Owner { get; private set; }
        public FlourBag_s(FlourBag owner)
        {
            GemType = GemType.Flour_s;
            Owner = owner;
            IsObstacle = true;
            CanMove = false;
        }

        public Vector2Int OwnerPos()
        {
            return new Vector2Int(Owner.X, Owner.Y);
        }

        public override void SplashDamage()
        {
            Owner.TakeDamage();
        }

        public override void TakeDamage()
        {
            Debug.Log("밀가루 봉지 조각의 takeDamage 호출");
            Owner.TakeDamage();
        }
    }
}
