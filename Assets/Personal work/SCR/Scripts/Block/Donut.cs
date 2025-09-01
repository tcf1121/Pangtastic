using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class Donut : Block
    {
        public Donut(int x, int y, GemType donutType)
        {
            Pos = new Vector2Int(x, y);
            Score = 10;
            GemType = donutType;
            IsObstacle = false;
            CanMove = true;
        }
    }
}