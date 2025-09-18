using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class Coin : ObstacleBlock
    {
        public Coin(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.Coin;
            IsObstacle = true;
            CanMove = true;
        }

        public override void SplashDamage()
        {
            TakeDamage();
        }

        public override void TakeDamage()
        {
            CurrentHP--;

            if (CurrentHP <= 0)
            {
                Debug.Log("파괴됨");
                Broken();
            }
        }

        public override void Broken()
        {
            InGameManager.AddCoin(10);
            _brokenCoroutine = BoardManager.Instance.StartCoroutine(BrokenAnimation(X, Y));
        }

        private IEnumerator BrokenAnimation(int x, int y)
        {
            var bm = BoardManager.Instance;
            CoinAnim coinAnim = BlockInstance.GetComponent<CoinAnim>();
            SpriteRenderer spriteRenderer = BlockInstance.GetComponent<SpriteRenderer>();

            if (coinAnim == null || coinAnim.CoinSprites == null || coinAnim.CoinSprites.Count == 0)
            {
                Debug.LogError("CoinAnim 컴포넌트, 코인 스프라이트 또는 페이드 커브가 설정되지 않았습니다.");
                base.Broken();
                yield break;
            }

            // If the curve is not set in the inspector, create a default linear fade-out curve.
            if (coinAnim.FadeCurve == null || coinAnim.FadeCurve.keys.Length == 0)
            {
                coinAnim.FadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            }

            bm.IsWaitingForAnimation = true;
            float timer = 0f;
            float animationDuration = 0.22f;
            int spriteCount = coinAnim.CoinSprites.Count;

            while (timer < animationDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / animationDuration;

                // Calculate sprite frame
                int count = Mathf.Min(Mathf.FloorToInt(progress * spriteCount), spriteCount - 1);
                
                // Evaluate alpha from the curve
                float alpha = coinAnim.FadeCurve.Evaluate(progress);

                spriteRenderer.sprite = coinAnim.CoinSprites[count];
                spriteRenderer.color = new Color(1f, 1f, 1f, alpha);

                yield return null;
            }

            base.Broken();
            PooledObject coinText = bm.CoinTextPool.GetObject();
            coinText.transform.position = bm.BlockMover.GridToWorld(new Vector2Int(x, y), bm.Spawner.GameBoardData.Width, bm.Spawner.GameBoardData.Height);
            yield return new WaitForSeconds(0.5f);
            bm.CoinTextPool.ReturnObject(coinText);
            bm.IsWaitingForAnimation = false;
            _brokenCoroutine = null;
        }
    }
}
