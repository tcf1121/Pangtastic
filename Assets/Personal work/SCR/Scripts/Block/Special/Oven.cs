using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class Oven : SpecialBlock
    {

        public Oven(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            GemType = GemType.Oven;
            IsObstacle = false;
            CanMove = true;
        }

    }
}
