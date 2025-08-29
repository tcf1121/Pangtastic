using SCR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace SCR_B
{
    public class GiftBox : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        public GiftBox(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
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

        public override void TakeDamage(BoardManager boardManager)
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken(boardManager);
            }
            else if (CurrentHP <= 2)
            {
                boardManager.BoardData.BlockArray[Pos.y, Pos.x].
                BlockInstance.GetComponent<Image>().sprite = _currentImage;
            }

        }

        public override void SplashDamage(BoardManager boardManager)
        {
            TakeDamage(boardManager);
        }

        public override void Broken(BoardManager boardManager)
        {
            Object.Destroy(boardManager.BoardData.BlockArray[Pos.y, Pos.x].BlockInstance);
            boardManager.Spawner.SpawnBlock(Pos.x, Pos.y, RandomGift());
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
                return GemType.Roller_v;
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
                return GemType.Oven;
            }
        }
    }
}
