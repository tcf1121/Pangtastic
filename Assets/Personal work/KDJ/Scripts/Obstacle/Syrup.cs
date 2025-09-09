using System.Collections;
using UnityEngine;

namespace KDJ
{
    public class Syrup : ObstacleBlock
    {
        public Syrup(int xpos, int ypos)
        {
            X = xpos;
            Y = ypos;
            Score = 0;
            CurrentHP = 1;
            GemType = SCR.GemType.Syrup;
            IsObstacle = true;
            CanMove = false;
        }

        public override void TakeDamage()
        {
            CurrentHP--;
            //    메소드 내부 로직:
            if (CurrentHP <= 0)
            {
                SyrupAnim anim = BlockInstance.GetComponent<SyrupAnim>();

                if (anim != null)
                {
                    if (_brokenCoroutine == null)
                    {
                        _brokenCoroutine = BoardManager.Instance.StartCoroutine(BrokenCoroutine());
                    }
                }
            }
        }

        public override void Broken()
        {
            InGameManager.AddIngredientSta(GemType);
            Object.Destroy(BlockInstance);
            BoardManager.Instance.Spawner.GameBoardData.OverlayArray[Y, X] = null;
        }

        public IEnumerator BrokenCoroutine()
        {
            yield return BoardManager.Instance.StartCoroutine(BlockInstance.GetComponent<SyrupAnim>().SyrupAnimation());

            Broken();
            BoardManager.Instance.IsWaitingForAnimation = false;
            _brokenCoroutine = null;
        }
    }
}
