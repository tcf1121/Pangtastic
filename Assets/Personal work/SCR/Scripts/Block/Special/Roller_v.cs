using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class Roller_v : SpecialBlock
    {

        public Roller_v(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            GemType = GemType.Roller_v;
            IsObstacle = false;
            CanMove = true;
        }
    }
}
