using UnityEngine;

namespace KDJ
{
    public class Coin : ObstacleBlock
    {
        public Coin(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.Coin;
            IsObstacle = true;
            CanMove = true;
        }

        public override void SplashDamage()
        {
            base.TakeDamage();
        }

        public override void Broken()
        {
            InGameManager.AddCoin(10);
            base.Broken();
        }
    }
}
