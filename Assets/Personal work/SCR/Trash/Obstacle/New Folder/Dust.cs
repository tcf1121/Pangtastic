using KDJ;
using UnityEngine;

namespace SCR_O
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
        }

        public override void Broken(BoardManager boardManager, int x, int y)
        {
            Object.Destroy(boardManager.Spawner.GameBoardData.OverlayArray[y, x].BlockInstance);
        }
    }
}
