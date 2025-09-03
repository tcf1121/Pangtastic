using KDJ;
using UnityEngine;

namespace KDJ
{
    public class DonutBag : ObstacleBlock
    {
        public DonutBag(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
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
            BoardManager.Instance.Spawner.SpawnRandomBlock(X, Y);
        }
    }
}
