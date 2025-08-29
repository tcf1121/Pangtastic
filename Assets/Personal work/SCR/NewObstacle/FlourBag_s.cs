using UnityEngine;

namespace SCR_B
{
    public class FlourBag_s : ObstacleBlock
    {
        private FlourBag _owner;
        public FlourBag_s(FlourBag owner)
        {
            GemType = SCR.GemType.Flour_s;
            _owner = owner;
            IsObstacle = true;
            CanMove = false;
        }

        public override void TakeDamage(BoardManager boardManager)
        {

        }

        public Vector2Int OwnerPos()
        {
            return _owner.Pos;
        }
    }
}
