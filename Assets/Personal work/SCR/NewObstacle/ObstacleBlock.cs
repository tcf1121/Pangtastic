using UnityEngine;

namespace SCR_B
{
    public class ObstacleBlock : Block
    {
        public int CurrentHP { get; set; }

        public override void TakeDamage(BoardManager boardManager)
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken(boardManager);
            }
        }

        public override void SplashDamage(BoardManager boardManager)
        {

        }

        public override void Broken(BoardManager boardManager)
        {

        }

        public override Block Clone()
        {
            return new ObstacleBlock
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
