using SCR;
using UnityEngine;

namespace SCR_B
{
    [System.Serializable]
    public class Block
    {
        public int Score { get; protected set; }
        public GameObject BlockInstance { get; set; } = null;
        public GemType GemType { get; set; }
        public bool IsObstacle { get; set; }
        public bool CanMove { get; set; }
        public Vector2Int Pos;

        public virtual void TakeDamage()
        {
            Broken();
        }

        public virtual void SplashDamage()
        {

        }

        public virtual void Broken()
        {
            Object.Destroy(BlockInstance);
            BoardManager.GetBoard().BoardData.BlockArray[Pos.y, Pos.x] = null;
        }

        public virtual Block Clone()
        {
            return new Block
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