using SCR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SCR_B
{
    public class Ice : ObstacleBlock
    {
        public GemType DountType;
        private Sprite iceSprite;
        public Ice(int xpos, int ypos, BoardData boardData = null)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            CurrentHP = 1;
            GemType = GemType.Ice;
            IsObstacle = true;
            CanMove = false;
            DountType = boardData.RespawnDount();
            string path = $"Assets/Imports/Image/Donut/Iced_{DountType}.png";
            Debug.Log(path);
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(path);
            handle.Completed += OnSpriteLoadCompleted;

        }

        public override GemType GetDonut()
        {
            return DountType;
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
            BlockInstance.Broken();
            BoardManager.GetBoard().Spawner.SpawnBlock(Pos.x, Pos.y, DountType);
        }
    }
}
