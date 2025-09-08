using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class PooledObject : MonoBehaviour
    {
        public ObjectPool Pool { get; set; }
        private Coroutine _returnCoroutine;

        private void OnDisable()
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
        }

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

        public void ReturnToPool(float delay)
        {
            if (_returnCoroutine == null)
            {
                _returnCoroutine = StartCoroutine(ReturnToPoolCoroutine(delay));
            }
        }

        public IEnumerator ReturnToPoolCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            _returnCoroutine = null;
            ReturnToPool();
        }
    }
}