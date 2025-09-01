using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class Milk : SpecialBlock
    {

        public Milk(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            GemType = GemType.Milk;
            IsObstacle = false;
            CanMove = true;
        }

    }
}
