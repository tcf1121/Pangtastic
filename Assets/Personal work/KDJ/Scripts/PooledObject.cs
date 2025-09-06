using UnityEngine;

namespace KDJ
{
    public class PooledObject : MonoBehaviour
    {
        public ObjectPool Pool { get; set; }

        public void ReturnToPool()
        {
            if (Pool != null)
            {
                Pool.ReturnObject(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}