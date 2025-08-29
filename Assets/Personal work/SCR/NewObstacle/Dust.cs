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
            BlockType = (int)GemType + 1;
            IsObstacle = true;
        }

        public override void Broken(BoardManager boardManager, int x, int y)
        {
            Object.Destroy(boardManager.Spawner.OverlayArray[y, x].BlockInstance);
        }
    }
}
