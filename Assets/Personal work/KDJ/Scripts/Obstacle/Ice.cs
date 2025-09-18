using SCR;
using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class Ice : ObstacleBlock
    {
        public GemType DountType;
        private Sprite iceSprite;
        public GameObject IceObject { get; set; }
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
        }

        public void SetSprite()
        {
            if (IceObject != null && iceSprite != null)
            {
                IceObject.GetComponent<SpriteRenderer>().sprite = iceSprite;
            }
        }

        public override void SplashDamage(GemType type)
        {
            Block block = BoardManager.Instance.Spawner.GameBoardData.BlockArray[Y, X];
            if (block != null && block.GemType == type)
            {
                TakeDamage();
            }
        }

        public override void Broken()
        {
            _brokenCoroutine = BoardManager.Instance.StartCoroutine(BrokenAnimation(X, Y));
            BoardManager.Instance.Spawner.GameBoardData.SetOverlayBlock(X, Y, null);
        }

        private IEnumerator BrokenAnimation(int x, int y)
        {
            BoardManager.Instance.IsWaitingForAnimation = true;
            GameObject blockObject = BoardManager.Instance.Spawner.GameBoardData.GetOverlayBlock(x, y).BlockInstance;
            float timer = 0f;
            while (timer < 0.15f)
            {
                timer += Time.deltaTime;
                blockObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer / 0.1f);
                yield return null;
            }

            if (blockObject != null)
                Object.Destroy(blockObject);
            
            BoardManager.Instance.Spawner.GameBoardData.BlockArray[y, x].IsNormal = true;
            BoardManager.Instance.Spawner.GameBoardData.BlockArray[y, x].CanMove = true;
            BoardManager.Instance.IsWaitingForAnimation = false;
            _brokenCoroutine = null;
        }
    }
}
