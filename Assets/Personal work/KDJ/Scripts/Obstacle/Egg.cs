using SCR;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KDJ
{
    public class Egg : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        public Egg(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 2;
            GemType = GemType.Egg;
            IsObstacle = true;
            CanMove = true;
            string path = $"Assets/Imports/Image/Obstacle/Egg_Damage.png";
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(path);
            handle.Completed += OnSpriteLoadCompleted;
        }

        private void OnSpriteLoadCompleted(AsyncOperationHandle<Sprite> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("가져오기 완료");
                _currentImage = handle.Result;
            }
        }

        public override void TakeDamage()
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken();
            }
            else
            {
                BlockInstance.GetComponent<SpriteRenderer>().sprite = _currentImage;
            }

        }

        public override void Broken()
        {
            InGameManager.AddScore(Score);
            InGameManager.AddIngredientSta(GemType);
            _brokenCoroutine = BoardManager.Instance.StartCoroutine(BrokenAnimation(X, Y));
        }

        public override void SplashDamage()
        {
            TakeDamage();
        }

        public override void OnLand(int y)
        {
            if (y == 0)
            {
                TakeDamage();
            }
        }

        private IEnumerator BrokenAnimation(int x, int y)
        {
            BoardManager.Instance.IsWaitingForAnimation = true;
            GameObject blockObject = BoardManager.Instance.Spawner.GameBoardData.GetBlock(x, y).BlockInstance;
            float timer = 0f;
            while (timer < 0.15f)
            {
                timer += Time.deltaTime;
                blockObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer / 0.1f);
                yield return null;
            }

            base.Broken();
            BoardManager.Instance.IsWaitingForAnimation = false;
            _brokenCoroutine = null;
        }
    }
}
