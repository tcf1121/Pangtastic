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

        public override void SplashDamage()
        {
            base.TakeDamage();
        }

        public override void Broken()
        {
            base.Broken();
            BoardManager.GetBoard().Spawner.SpawnDonut(Pos.x, Pos.y);
        }
    }
}
