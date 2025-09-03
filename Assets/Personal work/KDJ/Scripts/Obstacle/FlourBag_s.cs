using UnityEngine;
using SCR;

namespace KDJ
{
    public class FlourBag_s : ObstacleBlock
    {
        private FlourBag _owner;
        public FlourBag_s(FlourBag owner)
        {
            GemType = GemType.Flour_s;
            _owner = owner;
            IsObstacle = true;
            CanMove = false;
        }

        public Vector2Int OwnerPos()
        {
            return new Vector2Int(_owner.X, _owner.Y);
        }
    }
}
