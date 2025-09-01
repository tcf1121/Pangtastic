using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class SpecialBlock : Block
    {

        public virtual void Activate()
        {
            Debug.Log($"{GemType}사용{Pos}");
            BoardManager.UseSpecial(Pos, GemType);
        }

        public override void TakeDamage()
        {
            Activate();
            Broken();
        }

        public override void Broken()
        {
            Object.Destroy(BlockInstance);
            BoardManager.GetBoard().BoardData.BlockArray[Pos.y, Pos.x] = null;
        }

        public override Block Clone()
        {
            return new SpecialBlock
            {
                Pos = this.Pos,
                Score = this.Score,
                BlockInstance = this.BlockInstance,
                GemType = this.GemType,
                IsObstacle = this.IsObstacle,
                CanMove = this.CanMove,
            };
        }
    }
}
