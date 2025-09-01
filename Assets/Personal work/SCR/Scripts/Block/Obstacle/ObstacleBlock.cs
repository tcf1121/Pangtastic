using UnityEngine;

namespace SCR_B
{
    public class ObstacleBlock : Block
    {
        public int CurrentHP { get; set; }

        public override void TakeDamage()
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken();
            }
        }

        public override void SplashDamage()
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
