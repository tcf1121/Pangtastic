using SCR;
using UnityEngine;

namespace SCR_B
{
    public class Donut : Block
    {
        public Donut(int x, int y, GemType donutType)
        {
            Pos = new Vector2Int(x, y);
            Score = 10;
            GemType = donutType;
            IsObstacle = false;
            CanMove = true;
        }

        public override void Broken()
        {
            InGameManager.AddScore(Score);
            InGameManager.AddIngredientSta(GemType);

            base.Broken();
        }

        public override Block Clone()
        {
            return new Donut(Pos.x, Pos.y, GemType)
            {
                Score = this.Score,
                BlockInstance = this.BlockInstance,
                GemType = this.GemType,
                IsObstacle = this.IsObstacle,
                CanMove = this.CanMove,
                Pos = this.Pos
            };
        }
    }
}