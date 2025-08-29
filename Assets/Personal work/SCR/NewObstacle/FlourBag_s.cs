using KDJ;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_O
{
    public class FlourBag_s : ObstacleBlock
    {
        private FlourBag _owner;
        public FlourBag_s(FlourBag owner)
        {
            GemType = SCR.GemType.Flour_s;
            BlockType = (int)GemType + 1;
            _owner = owner;
            IsObstacle = true;
        }

        public override void TakeDamage(BoardManager boardManager)
        {

        }

        public Vector2Int OwnerPos()
        {
            return new Vector2Int(_owner.X, _owner.Y);
        }
    }
}
