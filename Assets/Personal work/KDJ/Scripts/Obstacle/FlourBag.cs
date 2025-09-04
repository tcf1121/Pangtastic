using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace KDJ
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
            IsObstacle = true;
            CanMove = false;
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
            fourPos.Add(new Vector2Int(X, Y + 1));
            fourPos.Add(new Vector2Int(X + 1, Y));
            fourPos.Add(new Vector2Int(X + 1, Y + 1));
            foreach (var pos in fourPos)
            {
                boardData.BlockPlateArray[pos.y, pos.x] = true;
                boardData.BlockArray[pos.y, pos.x] = new FlourBag_s(this);
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
                BlockInstance.GetComponent<GemPrefab>().SetSprite(_currentImage);
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
    }
}
