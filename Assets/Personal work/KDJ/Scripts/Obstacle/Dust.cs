using UnityEngine;

namespace KDJ
{
    public class Dust : ObstacleBlock
    {
        public Dust(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.Dust;
            IsObstacle = true;
            CanMove = false;
        }

        public override void Broken()
        {
            Object.Destroy(BlockInstance);
            BoardManager.Instance.Spawner.GameBoardData.OverlayArray[Y, X] = null;
        }
    }
}
