using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SCR.Test
{
    public class AddressableTest : MonoBehaviour
    {

        [SerializeField] GameObject prefab;
        void Start()
        {
            Addressables.LoadAssetAsync<GameObject>("Player").Completed += PlayerSpawner_Completed;

            prefab = Addressables.LoadAssetAsync<GameObject>("Player").WaitForCompletion();

            Addressables.LoadAssetAsync<IList<Material>>("Material").Completed += AllLoaded;
            Addressables.LoadAssetAsync<Material>("Material").Completed += OneLoaded;

        }

        private void PlayerSpawner_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> player)
        {
            Debug.Log("플레이어 소환");
        }

        private void AllLoaded(UnityEngine.ResourceManagement.
        AsyncOperations.AsyncOperationHandle<IList<Material>> mats)
        {

        }

        private void OneLoaded(UnityEngine.ResourceManagement.
        AsyncOperations.AsyncOperationHandle<Material> mat)
        {

        }

    }
}
