using KDJ;
using UnityEngine;

namespace SCR_B
{
    public class Syrup : ObstacleBlock
    {
        public Syrup(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.Syrup;
            IsObstacle = true;
            CanMove = false;
        }

        public override void Broken()
        {
            InGameManager.AddIngredientSta(GemType);
            BlockInstance.Broken();
            BoardManager.GetBoard().BoardData.OverlayArray[Pos.y, Pos.x] = null;
        }
    }
}
