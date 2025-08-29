using KDJ;
using SCR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace SCR_O
{
    public class GiftBox : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        public GiftBox(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 4;
            GemType = GemType.GiftBox;
            BlockType = (int)GemType + 1;
            IsObstacle = true;
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
                Broken(boardManager, X, Y);
            }
            else if (CurrentHP <= 2)
            {
                boardManager.Spawner.BlockArray[Y, X].
                BlockInstance.GetComponent<Image>().sprite = _currentImage;
            }

        }

        public override void SplashDamage(BoardManager boardManager)
        {
            TakeDamage(boardManager);
        }

        public override void Broken(BoardManager boardManager, int x, int y)
        {
            Object.Destroy(boardManager.Spawner.BlockArray[y, x].BlockInstance);
            boardManager.Spawner.SpawnBlock(x, y, (int)RandomGift() + 1);
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
