using KDJ;
using UnityEngine;

namespace KDJ
{
    public class Syrup : ObstacleBlock
    {
        public Syrup(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.Syrup;
            IsObstacle = true;
            CanMove = false;
        }

        public override void Broken()
        {
            InGameManager.AddIngredientSta(GemType);
            Object.Destroy(BlockInstance);
            BoardManager.Instance.Spawner.GameBoardData.OverlayArray[Y, X] = null;
        }
    }
}
