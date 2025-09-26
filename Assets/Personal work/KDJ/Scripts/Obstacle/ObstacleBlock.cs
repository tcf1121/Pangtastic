
using SCR;
using UnityEngine;

namespace KDJ
{
    public class ObstacleBlock : Block
    {
        public int CurrentHP { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        protected Coroutine _brokenCoroutine;


        public override Block Clone()
        {
            return (ObstacleBlock)this.MemberwiseClone();
        }

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

        public virtual void SplashDamage(GemType type)
        {

        }

        public virtual void OnLand(int y)
        {

        }
    }

}
