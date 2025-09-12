using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KDJ
{
    public class FlourBag : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        private List<Vector2Int> fourPos = new();
        public FlourBag(Block[,] array, int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 2;
            GemType = SCR.GemType.FlourBag;
            IsObstacle = true;
            CanMove = false;
            SetFourPos(array);
            string path = $"Assets/Imports/Image/Obstacle/FlourBag_Damage.png";
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(path);
            handle.Completed += OnSpriteLoadCompleted;
        }

        private Coroutine _damageCoroutine;

        private void OnSpriteLoadCompleted(AsyncOperationHandle<Sprite> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _currentImage = handle.Result;
            }
        }

        private void SetFourPos(Block[,] array)
        {
            fourPos.Add(new Vector2Int(X, Y + 1));
            fourPos.Add(new Vector2Int(X + 1, Y));
            fourPos.Add(new Vector2Int(X + 1, Y + 1));
            foreach (var pos in fourPos)
            {
                if (array[pos.y, pos.x] != null && array[pos.y, pos.x].BlockInstance != null)
                {
                    GameObject.Destroy(array[pos.y, pos.x].BlockInstance);
                }
                array[pos.y, pos.x] = new FlourBag_s(this);
            }

        }

        public override void TakeDamage()
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                if (_brokenCoroutine == null)
                {
                    _brokenCoroutine = BoardManager.Instance.StartCoroutine(BrokenAnimation(X, Y));
                }
            }
            else
            {
                BlockInstance.GetComponent<SpriteRenderer>().sprite = _currentImage;

                if (_damageCoroutine == null)
                {
                    _damageCoroutine = BoardManager.Instance.StartCoroutine(DamageAnimation(X, Y));
                }
            }
        }

        public override void SplashDamage()
        {
            TakeDamage();
        }

        public override void Broken()
        {
            foreach (var pos in fourPos)
                BoardManager.Instance.Spawner.GameBoardData.BlockArray[pos.y, pos.x] = null;

            base.Broken();
        }

        private IEnumerator BrokenAnimation(int x, int y)
        {
            BoardManager.Instance.IsWaitingForAnimation = true;
            float timer = 0f;
            while (timer < 0.15f)
            {
                timer += Time.deltaTime;
                BlockInstance.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer / 0.1f);
                yield return null;
            }

            Broken();
            _brokenCoroutine = null;
            BoardManager.Instance.IsWaitingForAnimation = false;
        }

        private IEnumerator DamageAnimation(int x, int y)
        {
            BoardManager.Instance.IsWaitingForAnimation = true;

            float timer = 0f;

            while (timer < 0.15f)
            {
                timer += Time.deltaTime;
                // 좌우로 빠르게 흔들림. 0.05초마다 좌우로 흔들리게
                if ((int)(timer / 0.025f) % 2 == 0)
                {
                    BlockInstance.transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, -20), Quaternion.Euler(0, 0, 20), (timer % 0.025f) / 0.025f);
                }
                else
                {
                    BlockInstance.transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, 20), Quaternion.Euler(0, 0, -20), (timer % 0.025f) / 0.025f);
                }
                yield return null;
            }

            BlockInstance.transform.rotation = Quaternion.Euler(0, 0, 0);

            BoardManager.Instance.IsWaitingForAnimation = false;
            _damageCoroutine = null;

        }
    }
}
