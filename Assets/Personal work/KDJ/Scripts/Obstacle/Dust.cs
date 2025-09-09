using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class Dust : ObstacleBlock
    {
        public Dust(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 2;
            GemType = SCR.GemType.Dust;
            IsObstacle = true;
            CanMove = false;
        }

        private Coroutine _damageCoroutine;

        public override void TakeDamage()
        {
            CurrentHP--;

            if (_damageCoroutine == null)
            {
                _damageCoroutine = BoardManager.Instance.StartCoroutine(DamageAnimation());
            }
        }

        public override void Broken()
        {
            Object.Destroy(BlockInstance);
            BoardManager.Instance.Spawner.GameBoardData.OverlayArray[Y, X] = null;
        }

        private IEnumerator DamageAnimation()
        {
            if (CurrentHP == 1)
            {
                yield return BoardManager.Instance.StartCoroutine(BlockInstance.GetComponent<DustAnim>().SizeAnim(0.5f, 0.25f));
            }
            else if (CurrentHP == 0)
            {
                yield return BoardManager.Instance.StartCoroutine(BlockInstance.GetComponent<DustAnim>().SizeAnim(0.25f, 0f));
                Broken();
            }
            _damageCoroutine = null;
        }
    }
}
