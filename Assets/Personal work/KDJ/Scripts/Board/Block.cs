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
    }
}
