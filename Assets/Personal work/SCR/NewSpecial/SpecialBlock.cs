using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class SpecialBlock : Block
    {

        public SpecialBlock(GemType gemType)
        {
            Score = 0;
            GemType = gemType;
            IsObstacle = false;
            CanMove = true;
        }

        public virtual void Activate(BoardManager boardManager)
        {

        }

        public override void TakeDamage(BoardManager boardManager)
        {
            Activate(boardManager);
            Broken(boardManager);
        }

        public override void SplashDamage(BoardManager boardManager)
        {

        }

        public override void Broken(BoardManager boardManager)
        {
            Object.Destroy(boardManager.Spawner._boardData.BlockArray[Pos.y, Pos.x].BlockInstance);
        }
    }
}
