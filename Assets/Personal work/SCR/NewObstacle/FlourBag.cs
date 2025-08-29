using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace SCR_B
{
    public class FlourBag : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        private List<Vector2Int> fourPos = new();
        public FlourBag(BoardData boardData, int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
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
            fourPos.Add(Pos + Vector2Int.up);
            fourPos.Add(Pos + Vector2Int.right);
            fourPos.Add(Pos + Vector2Int.up + Vector2Int.right);
            foreach (var pos in fourPos)
                boardData.BlockArray[pos.y, pos.x] = new FlourBag_s(this);
        }

        public override void TakeDamage(BoardManager boardManager)
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken(boardManager);
            }
            else
            {
                boardManager.Spawner._boardData.BlockArray[Pos.y, Pos.x].
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
            foreach (var pos in fourPos)
            {
                Object.Destroy(boardManager.BoardData.BlockArray[pos.y, pos.x].BlockInstance);
            }
        }
    }
}
