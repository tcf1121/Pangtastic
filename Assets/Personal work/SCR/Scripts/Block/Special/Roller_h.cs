using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class Roller_h : SpecialBlock
    {

        public Roller_h(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            GemType = GemType.Roller_h;
            IsObstacle = false;
            CanMove = true;
        }
    }
}
