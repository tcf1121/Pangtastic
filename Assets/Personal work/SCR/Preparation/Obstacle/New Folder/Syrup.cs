using KDJ;
using UnityEngine;

namespace SCR_O
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
        }

        public override void Broken(BoardManager boardManager, int x, int y)
        {
            Object.Destroy(boardManager.Spawner.GameBoardData.OverlayArray[y, x].BlockInstance);
        }
    }
}
