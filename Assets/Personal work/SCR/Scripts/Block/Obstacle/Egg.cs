using SCR;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        public override void Broken()
        {
            InGameManager.AddScore(Score);
            InGameManager.AddIngredientSta(GemType);
            base.Broken();
        }

        public override void SplashDamage()
        {
            TakeDamage();
        }

        public override Block Clone()
        {
            return new Egg(Pos.x, Pos.y)
            {
                CurrentHP = this.CurrentHP,
                Pos = this.Pos,
                Score = this.Score,
                BlockInstance = this.BlockInstance,
                GemType = this.GemType,
                IsObstacle = this.IsObstacle,
                CanMove = this.CanMove,
            };
        }
    }
}
