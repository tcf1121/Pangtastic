using UnityEngine;

namespace SCR_B
{
    public class Dust : ObstacleBlock
    {
        public Dust(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.Dust;
            IsObstacle = true;
            CanMove = false;
        }

        public override void Broken()
        {
            BlockInstance.Broken();
            BoardManager.GetBoard().BoardData.OverlayArray[Pos.y, Pos.x] = null;
        }
    }
}
