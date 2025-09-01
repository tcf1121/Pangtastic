using KDJ;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace SCR_O
{
    public class FlourBag : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        private List<Vector2Int> fourPos = new();
        public FlourBag(BoardData boardData, int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 2;
            GemType = SCR.GemType.FlourBag;
            BlockType = (int)GemType + 1;
            IsObstacle = true;
            SetFourPos(boardData);
            string path = $"Assets/Imports/Image/Obstacle/FlourBag_Damage.png";
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

        private void SetFourPos(BoardData boardData)
        {
            fourPos.Add(new Vector2Int(X + 1, Y));
            fourPos.Add(new Vector2Int(X, Y + 1));
            fourPos.Add(new Vector2Int(X + 1, Y + 1));
            foreach (var pos in fourPos)
                boardData.BlockArray[pos.y, pos.x] = new FlourBag_s(this);
        }

        public override void TakeDamage(BoardManager boardManager)
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken(boardManager, X, Y);
            }
            else
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
            foreach (var pos in fourPos)
            {
                Object.Destroy(boardManager.Spawner.BlockArray[pos.y, pos.x].BlockInstance);
            }
        }
    }
}
