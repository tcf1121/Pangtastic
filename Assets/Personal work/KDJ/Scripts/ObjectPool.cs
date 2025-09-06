using System.Collections.Generic;
using UnityEngine;

namespace KDJ
{
    public abstract class ObjectPool : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private int initialSize = 10;

        private Queue<PooledObject> pool = new Queue<PooledObject>();

        protected virtual void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private PooledObject CreateNewObject()
        {
            var newGO = Instantiate(prefab);
            var pooledObject = newGO.GetComponent<PooledObject>();
            if (pooledObject == null)
            {
                pooledObject = newGO.AddComponent<PooledObject>();
            }
            
            pooledObject.Pool = this;
            newGO.transform.SetParent(transform);
            newGO.SetActive(false);
            pool.Enqueue(pooledObject);
            return pooledObject;
        }

        public PooledObject GetObject()
        {
            if (pool.Count == 0)
            {
                CreateNewObject();
            }

            var pooledObject = pool.Dequeue();
            pooledObject.transform.SetParent(null);
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }

        public void ReturnObject(PooledObject objectToReturn)
        {
            objectToReturn.gameObject.SetActive(false);
            objectToReturn.transform.SetParent(transform);
            pool.Enqueue(objectToReturn);
        }
    }
}
