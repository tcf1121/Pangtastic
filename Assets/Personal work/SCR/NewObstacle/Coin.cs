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

        public override void SplashDamage(BoardManager boardManager)
        {
            base.TakeDamage(boardManager);
        }

        public override void Broken(BoardManager boardManager)
        {
            InGameManager.AddCoin(1);
            base.Broken(boardManager);
        }
    }
}
