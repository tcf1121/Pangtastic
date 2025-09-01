using KDJ;
using UnityEngine;

namespace SCR_O
{
    public abstract class ObstacleBlock : Block
    {
        public int CurrentHP { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public virtual void TakeDamage(BoardManager boardManager)
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken(boardManager, X, Y);
            }
        }

        public virtual void SplashDamage(BoardManager boardManager)
        {

        }

        public virtual void Broken(BoardManager boardManager, int x, int y)
        {

        }
    }

}
