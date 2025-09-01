using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class DonutBox : SpecialBlock
    {

        public DonutBox(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            GemType = GemType.DonutBox;
            IsObstacle = false;
            CanMove = true;
        }

    }
}
