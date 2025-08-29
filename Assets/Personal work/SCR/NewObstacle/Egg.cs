using SCR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace SCR_B
{
    public class Egg : ObstacleBlock
    {
        [SerializeField] private Sprite _currentImage;
        public Egg(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
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
            else
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
        }

    }
}
