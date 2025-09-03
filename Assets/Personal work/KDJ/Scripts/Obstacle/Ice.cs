using SCR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KDJ
{
    public class Ice : ObstacleBlock
    {
        public GemType DountType;
        private Sprite iceSprite;
        public Ice(int xpos, int ypos, BoardData boardData = null)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 1;
            GemType = GemType.Ice;
            IsObstacle = true;
            CanMove = false;
            DountType = (GemType)Random.Range(0, 6);
            string path = $"Assets/Imports/Image/Donut/Iced_{DountType}.png";
            Debug.Log(path);
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(path);
            handle.Completed += OnSpriteLoadCompleted;

        }

        private void OnSpriteLoadCompleted(AsyncOperationHandle<Sprite> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("가져오기 완료");
                iceSprite = handle.Result;
                Debug.Log(iceSprite);
            }
        }

        public Sprite GetIceImage()
        {
            Debug.Log($"파일명{iceSprite}");
            return iceSprite;
        }

        public override void SplashDamage()
        {
        }

        public override void Broken()
        {
            Object.Destroy(BlockInstance);
            BoardManager.Instance.Spawner.SpawnBlock(X, Y, DountType, BoardManager.Instance.BlockMover);
        }
    }
}
