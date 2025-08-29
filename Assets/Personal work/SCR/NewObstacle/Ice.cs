using SCR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SCR_B
{
    public class Ice : ObstacleBlock
    {
        private GemType _dount;
        private Sprite iceSprite;
        public Ice(int xpos, int ypos)
        {
            Pos = new Vector2Int(xpos, ypos);
            Score = 0;
            CurrentHP = 1;
            GemType = GemType.Ice;
            IsObstacle = true;
            CanMove = false;
            _dount = (GemType)Random.Range(0, 6);
            string path = $"Assets/Imports/Image/Donut/Iced_{_dount}.png";
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

        public override void SplashDamage(BoardManager boardManager)
        {
            base.TakeDamage(boardManager);
        }

        public override void Broken(BoardManager boardManager)
        {
            Object.Destroy(boardManager.BoardData.BlockArray[Pos.y, Pos.x].BlockInstance);
            boardManager.Spawner.SpawnBlock(Pos.x, Pos.y, _dount);
        }
    }
}
