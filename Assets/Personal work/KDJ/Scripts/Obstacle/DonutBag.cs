using SCR;
using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class DonutBag : ObstacleBlock
    {
        public DonutBag(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.DonutBag;
            CanMove = false;
            IsObstacle = true;
        }

        public override void SplashDamage()
        {
            base.TakeDamage();
        }

        public override void Broken()
        {
            _brokenCoroutine = BoardManager.Instance.StartCoroutine(BrokenAnimation(X, Y));
        }

        private IEnumerator BrokenAnimation(int x, int y)
        {
            BoardManager.Instance.IsWaitingForAnimation = true;
            GameObject blockObject = BoardManager.Instance.Spawner.GameBoardData.GetBlock(x, y).BlockInstance;
            float timer = 0f;
            while (timer < 0.15f)
            {
                timer += Time.deltaTime;
                blockObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer / 0.1f);
                yield return null;
            }

            base.Broken();
            BoardManager.Instance.StartCoroutine(BoardManager.Instance.Spawner.SpawnWithAnimation(x, y,
            (GemType)Random.Range(0, BoardManager.Instance.Spawner.GetMaxDonutSpawnRange() + 1)));
            BoardManager.Instance.IsWaitingForAnimation = false;
            _brokenCoroutine = null;
        }
    }
}
