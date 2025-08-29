using KDJ;
using UnityEngine;

namespace SCR_B
{
    public class DonutBag : ObstacleBlock
    {
        public DonutBag(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.DonutBag;
            CanMove = false;
            IsObstacle = true;
        }

        public override void SplashDamage(BoardManager boardManager)
        {
            base.TakeDamage(boardManager);
        }

        public override void Broken(BoardManager boardManager)
        {
            base.Broken(boardManager);
            boardManager.Spawner.SpawnDonut(Pos.x, Pos.y);
        }
    }
}
