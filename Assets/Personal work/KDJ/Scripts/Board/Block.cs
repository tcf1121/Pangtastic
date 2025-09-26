using UnityEngine;
using SCR;

namespace KDJ
{
    [System.Serializable]
    public class Block
    {
        
        public int Score { get; protected set; } = 10;
        public GameObject BlockInstance { get; set; } = null;
        public GemType GemType { get; set; }
        public bool IsObstacle { get; set; } = false;
        public bool IsNormal { get; set; } = true;
        public bool CanMove { get; set; } = true;

        /// <summary>
        /// 이 블록 객체의 얕은 복사본을 생성합니다.
        /// </summary>
        /// <returns>복제된 Block 객체</returns>
        public virtual Block Clone()
        {
            return (Block)this.MemberwiseClone();
        }
    }
}
