using UnityEngine;

namespace SCR_B
{
    public class Coin : ObstacleBlock
    {
        public Coin(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
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
            InGameManager.AddCoin(1);
            base.Broken();
        }

        public override Block Clone()
        {
            return new Coin(Pos.x, Pos.y)
            {
                CurrentHP = this.CurrentHP,
                Pos = this.Pos,
                Score = this.Score,
                BlockInstance = this.BlockInstance,
                GemType = this.GemType,
                IsObstacle = this.IsObstacle,
                CanMove = this.CanMove,
            };
        }
    }
}
