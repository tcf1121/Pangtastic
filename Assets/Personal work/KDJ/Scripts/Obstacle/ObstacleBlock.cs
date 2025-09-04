
using UnityEngine;

namespace KDJ
{
    public class ObstacleBlock : Block
    {
        public int CurrentHP { get; set; }
        public int X { get; set; }
        public int Y { get; set; }


        public virtual void TakeDamage()
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken();
            }
        }

        public virtual void Broken()
        {
            if (BlockInstance != null)
                Object.Destroy(BlockInstance);
            BoardManager.Instance.Spawner.GameBoardData.SetBlock(X, Y, null);
        }

        public virtual void SplashDamage()
        {

        }

        // public override Block Clone()
        // {
        //     return new ObstacleBlock
        //     {
        //         CurrentHP = this.CurrentHP,
        //         Pos = this.Pos,
        //         Score = this.Score,
        //         BlockInstance = this.BlockInstance,
        //         GemType = this.GemType,
        //         IsObstacle = this.IsObstacle,
        //         CanMove = this.CanMove,
        //     };
        // }
    }

}
