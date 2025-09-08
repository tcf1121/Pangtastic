using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{

    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool instance;
        public GameObject overlappingPrefab;
        public GameObject container;
        public int PoolCount;
        public Queue<GameObject> Pool = new Queue<GameObject>();


        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);

        }

        void Start()
        {
            Init();
        }

        private void Init()
        {
            for (int i = 0; i < PoolCount; i++)
            {
                CreatePoolObject();
            }

        }

        // 생성
        private void CreatePoolObject()
        {
            GameObject poolGO = Instantiate(overlappingPrefab);
            poolGO.transform.parent = container.transform;
            poolGO.SetActive(false);
            Pool.Enqueue(poolGO);
        }

        // 생성
        private void CreatePoolObject(GameObject Prefab)
        {
            GameObject poolGO = Instantiate(Prefab);
            poolGO.transform.parent = container.transform;
            poolGO.SetActive(false);
            Pool.Enqueue(poolGO);
        }

        // 사용
        public static GameObject TakeFromPool()
        {
            GameObject objInstance = null;
            if (instance.Pool.Count > 0)
            {
                objInstance = instance.Pool.Dequeue();
            }
            else
            {
                instance.CreatePoolObject();
                objInstance = instance.Pool.Dequeue();
            }
            objInstance.gameObject.SetActive(true);
            return objInstance;
        }

        public static void PushPool(GameObject Prefab)
        {
            instance.CreatePoolObject(Prefab);
        }

        // 반환
        public static void ReturnPool(GemPrefab poolGo)
        {
            instance.Pool.Enqueue(poolGo.gameObject);
            poolGo.gameObject.SetActive(false);
        }

    }
}

