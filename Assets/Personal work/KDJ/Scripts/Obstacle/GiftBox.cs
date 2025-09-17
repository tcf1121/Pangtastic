using SCR;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KDJ
{
    public class GiftBox : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        private Coroutine _damageCoroutine;
        public GiftBox(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 4;
            GemType = GemType.GiftBox;
            IsObstacle = true;
            CanMove = true;
            string path = $"Assets/Imports/Image/Obstacle/GiftBox_Damage.png";
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(path);
            handle.Completed += OnSpriteLoadCompleted;
        }

        private void OnSpriteLoadCompleted(AsyncOperationHandle<Sprite> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _currentImage = handle.Result;
            }
        }

        public override void TakeDamage()
        {
            CurrentHP--;

            if (CurrentHP > 0)
            {
                Manager.Audio.PlaySFX("GiftBox_Damage");
                _damageCoroutine = BoardManager.Instance.StartCoroutine(DamageAnimation(X, Y));
            }

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Manager.Audio.PlaySFX("GiftBox_Broken");
                Broken();
            }
            else if (CurrentHP <= 2)
            {
                BoardManager.Instance.Spawner.GameBoardData.BlockArray[Y, X].
                BlockInstance.GetComponent<SpriteRenderer>().sprite = _currentImage;
            }

        }

        public override void SplashDamage()
        {
            TakeDamage();
        }

        public override void Broken()
        {
            _brokenCoroutine = BoardManager.Instance.StartCoroutine(BrokenAnimation(X, Y));
        }

        private GemType RandomGift()
        {
            int random = Random.Range(0, 100);
            if (random < 25)
            {
                return GemType.Roller_h;
            }
            else if (random < 50)
            {
                return GemType.Milk;
            }
            else if (random < 75)
            {
                return GemType.Milk;
            }
            else if (random < 90)
            {
                return GemType.DonutBox;
            }
            else
            {
                return GemType.DonutBox;
            }
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

            base.Broken();
            BoardManager.Instance.StartCoroutine(BoardManager.Instance.Spawner.SpawnWithAnimation(x, y, RandomGift()));
            _brokenCoroutine = null;
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
