using KDJ;
using KDJ.States;
using SCR;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LHJ
{
    public class ItemCheck : MonoBehaviour
    {
        [SerializeField] private CopyBoardManager _board;

        private void Update()
        {
            if (_board == null || !_board.IsItemSelected) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                Vector3 mouse = Input.mousePosition;
                mouse.z = -Camera.main.transform.position.z;
                Vector3 world = Camera.main.ScreenToWorldPoint(mouse);

                int w = _board.Spawner.BlockPlate.BlockPlateWidth;
                int h = _board.Spawner.BlockPlate.BlockPlateHeight;
                Vector2 target = (Vector2)world + new Vector2(w / 2f, h / 2f);
                Vector2Int grid = _board.BlockMover.WorldToGrid(target);

                if (!InBounds(grid))
                {
                    _board.ClearItemSelection();
                    return;
                }

                // 선택된 아이템 발동
                UseCell(grid, _board.SelectedItemType);

                // 발동 후 해제
                _board.ClearItemSelection();
            }
        }
        
        // 커피 아이템
        public void UseCoffee(float amount = 30f)
        {
            var order = FindObjectOfType<OrderStateController>();
            if (order == null || order._curPatience <= 0f) return;

            // 현재 진행률 기반으로 남은 시간 추정
            float progress = 1f - (order._curPatience / 100f);
            float estimatedTimeToZero = (progress > 0.001f) ? (order._elapsed / progress) : 60f;

            // 인내심 증가
            float newCur = Mathf.Min(order._curPatience + amount, 100f);
            order._curPatience = newCur;

            float newProgress = 1f - (newCur / 100f);
            order._elapsed = Mathf.Clamp(newProgress * estimatedTimeToZero, 0f, estimatedTimeToZero);
        }

        // 좌표 확인
        private bool InBounds(Vector2Int p)
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;
            return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;
        }


        // 아이템 별 해당 아이템 효과 실행
        private void UseCell(Vector2Int pos, ItemType type)
        {
            int destroyed = 0;
            switch (type)
            {
                case ItemType.Scissor:
                    destroyed = ApplyScissor(pos);
                    break;
                case ItemType.Whisk:
                    destroyed = ApplyWhisk(pos);
                    break;
                case ItemType.DonutPan:
                    StartCoroutine(RegenSpecialBlockRoutine());
                    break;
            }

            if (destroyed > 0)
            {
                _board.UpdateUI(destroyed * 10);
                _board.ChangeState(new RefillState());
            }
        }

        // 가위: 선택 지점의 가로+세로 제거
        private int ApplyScissor(Vector2Int pos)
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;
            int destroyedCount = 0;

            // 가로
            for (int x = 0; x < w; x++)
            {
                var blk = sp.BlockArray[pos.y, x];
                if (blk == null || blk.BlockInstance == null) continue;

                var special = blk.BlockInstance.GetComponent<SpecialBlock>();
                if (special != null) special.Activate(_board);

                Object.Destroy(blk.BlockInstance);
                sp.BlockArray[pos.y, x].BlockInstance = null;
                destroyedCount++;
            }

            // 세로
            for (int y = 0; y < h; y++)
            {
                var blk = sp.BlockArray[y, pos.x];
                if (blk == null || blk.BlockInstance == null) continue;

                var special = blk.BlockInstance.GetComponent<SpecialBlock>();
                if (special != null) special.Activate(_board);

                Object.Destroy(blk.BlockInstance);
                sp.BlockArray[y, pos.x].BlockInstance = null;
                destroyedCount++;
            }

            return destroyedCount;
        }

        // 거품기: 선택 지점 기준 3x3 제거
        private int ApplyWhisk(Vector2Int pos)
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;
            int destroyedCount = 0;

            int r = 1;
            for (int y = pos.y - r; y <= pos.y + r; y++)
            {
                if (y < 0 || y >= h) continue;
                for (int x = pos.x - r; x <= pos.x + r; x++)
                {
                    if (x < 0 || x >= w) continue;

                    var blk = sp.BlockArray[y, x];
                    if (blk == null || blk.BlockInstance == null) continue;

                    var special = blk.BlockInstance.GetComponent<SpecialBlock>();
                    if (special != null) special.Activate(_board);

                    Object.Destroy(blk.BlockInstance);
                    sp.BlockArray[y, x].BlockInstance = null;
                    destroyedCount++;
                }
            }
            return destroyedCount;
        }

        // 도넛판 아이템 실행
        public void UseDonutPan()
        {
            if (_board != null) _board.ClearItemSelection();
            StartCoroutine(RegenSpecialBlockRoutine());
        }

        // 보드를 초기화 후 특수 블록 하나 생성하는 루틴
        private IEnumerator RegenSpecialBlockRoutine()
        {
            ClearBoard();
            _board.ChangeState(new RefillState());
            while (_board.Spawner.HasEmptyBlockObjects())
                yield return null;                       
            InjectRandomSpecial();

            yield break;
        }

        // 랜덤 위치에 특수 블록 생성
        private void InjectRandomSpecial()
        {
            var sp = _board.Spawner;

            // 매치 로직과 동일한 특수블록 아이디만 사용
            int[] specialIds = { 7, 8, 9, 10, 11 };
            int id = specialIds[Random.Range(0, specialIds.Length)];

            sp.RandomPosSpawnSpecialBlock(id);
        }

        // 보드 전체 클리어
        private void ClearBoard()
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var cell = sp.BlockArray[y, x];
                    if (cell == null) continue;

                    if (cell.BlockInstance != null)
                    {
                        Destroy(cell.BlockInstance);
                        cell.BlockInstance = null;
                    }
                }
            }
        }
    }
}
