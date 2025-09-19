using DG.Tweening;
using KDJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class Roller_H : SpecialBlock
    {
        [SerializeField] private bool _destroySpecial;
        [SerializeField] private float _perTileDuration = 0.15f; 
        [SerializeField] private float _trailFadeTime = 0.3f;
        private Tween _moveTween;
        public override void Activate(BoardManager board)
        {
            if (board == null || board.Spawner == null) return;
            if (SpecialBlockCombo.Instance != null &&
                SpecialBlockCombo.Instance.TryResolveFromActivate(board, this.gameObject))
            {
                return;
            }

            StartRoller(board);
        }
        private void StartRoller(BoardManager board)
        {
            var sp = board.Spawner;
            var plate = sp.GameBoardData.BlockPlate;
            int w = plate.BlockPlateWidth;
            int h = plate.BlockPlateHeight;

            // 현재 프리팹이 어느 칸에 있든 연출 고정
            int myX = Mathf.RoundToInt(transform.position.x + w / 2f - 0.5f);
            int myY = Mathf.RoundToInt(transform.position.y + h / 2f - 0.5f);
            Vector3 startPos = new Vector3(-w / 2f, myY - h / 2f + 0.5f, 0f);
            Vector3 endPos = new Vector3(w / 2f, myY - h / 2f + 0.5f, 0f);

            var startCell = sp.GameBoardData.BlockArray[myY, myX];
            if (startCell != null && startCell.BlockInstance == this.gameObject)
                startCell.BlockInstance = null;

            // 시작점을 왼쪽 끝으로 강제 세팅
            transform.position = startPos;

            var trail = GetComponent<TrailRenderer>();

            var processedX = new HashSet<int>();
            int destroyedCount = 0;
            float duration = _perTileDuration * w;

            // 트윈 시작
            _moveTween = transform.DOMoveX(endPos.x, duration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    // 현재 Fx 위치 → 보드 인덱스 변환
                    int curX = Mathf.RoundToInt(transform.position.x + w / 2f - 0.5f);
                    if (curX < 0 || curX >= w) return;
                    if (processedX.Contains(curX)) return;

                    var cell = sp.GameBoardData.BlockArray[myY, curX];
                    if (cell == null)
                    {
                        processedX.Add(curX);
                        return;
                    }

                    var inst = cell.BlockInstance;
                    if (inst == this.gameObject)
                    {
                        processedX.Add(curX);
                        return;
                    }

                    if (inst != null)
                    {
                        var special = inst.GetComponent<SpecialBlock>();
                        if (special != null)
                        {
                            if (_destroySpecial)
                            {
                                // 다른 특수블록은 즉시 발동
                                special.Activate(board);
                            }
                            return;
                        }

                        // 일반블록 제거
                        Object.Destroy(inst);
                        cell.BlockInstance = null;
                        destroyedCount++;
                    }

                    processedX.Add(curX);
                })
                .OnComplete(() =>
                {
                    if (trail != null) trail.emitting = false;
                    board.StartCoroutine(Cleanup(board, destroyedCount));
                });
        }

        private IEnumerator Cleanup(BoardManager board, int destroyedCount)
        { 
            yield return new WaitForSeconds(_trailFadeTime);

            // 롤러 오브젝트 제거
            if (gameObject != null) Destroy(gameObject);

            if (destroyedCount > 0)
                //board.UpdateUI(destroyedCount * 10);

            if (board.MatchCombo != null)
                board.MatchCombo.ResetTimer();
            board.ChangeState(new KDJ.States.RefillState());
        }
    }
}
