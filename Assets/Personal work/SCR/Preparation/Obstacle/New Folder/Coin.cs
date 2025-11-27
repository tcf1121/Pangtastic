using KDJ;
using UnityEngine;

namespace SCR_O
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
        }

        public override void SplashDamage(BoardManager boardManager)
        {
            base.TakeDamage(boardManager);
        }

        public override void Broken(BoardManager boardManager, int x, int y)
        {
            Object.Destroy(boardManager.Spawner.GameBoardData.BlockArray[y, x].BlockInstance);
            InGameManager.AddCoin(1);
        }
    }
}
