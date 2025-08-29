using KDJ;
using KDJ.States;
using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LHJ
{
    public class ItemCheck : MonoBehaviour
    {
        [SerializeField] private BoardManager _board;

        private void Update()
        {
            var manager = CopyBoardManager.Instance;
            if (_board == null || manager == null || !manager.IsItemSelected) return;

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
                    manager.ClearItemSelection();
                    return;
                }

                // 선택된 아이템 발동
                UseCell(grid, manager.SelectedItemType);

                // 발동 후 해제
                manager.ClearItemSelection();
            }
        }
        public void UseCoffee(float amount = 30f)
        {
            var order = FindObjectOfType<OrderStateController>();
            if (order == null || order._curPatience <= 0f) return;
            order._curPatience = Mathf.Min(order._curPatience + amount, 100f);

            float timeToZero = 60f;
            if (order._curCustomer != null)
            {
                if (order._curCustomer.Type == CustomerType.Unique)
                    timeToZero = 50f;
                else if (order._curCustomer.Type == CustomerType.Special)
                    timeToZero = 30f;
            }

            float progress = 1f - (order._curPatience / 100f);
            order._elapsed = progress * timeToZero;
        }
        private bool InBounds(Vector2Int p)
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;
            return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;
        }

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
        public void UseDonutPan()
        {
            var manager = CopyBoardManager.Instance;
            if (manager != null) manager.ClearItemSelection();
            StartCoroutine(RegenSpecialBlockRoutine());
        }
        private IEnumerator RegenSpecialBlockRoutine()
        {
            ClearBoard();
            _board.ChangeState(new RefillState());
            yield return new WaitUntil(IsBoardFilled);
            InjectRandomSpecial();

            yield break;
        }

        private void InjectRandomSpecial()
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;

            for (int t = 0; t < 100; t++)
            {
                int x = Random.Range(0, w);
                int y = Random.Range(0, h);

                var cell = sp.BlockArray[y, x];
                if (cell == null || cell.BlockInstance == null) continue;

                Destroy(cell.BlockInstance);
                sp.BlockArray[y, x] = null;

                int[] specialIds = { 7, 8, 9, 10, 11 };
                int id = specialIds[Random.Range(0, specialIds.Length)];
                sp.SpawnBlock(x, y, id);
                break;
            }
        }
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
        private bool IsBoardFilled()
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var cell = sp.BlockArray[y, x];
                    if (cell == null || cell.BlockInstance == null)
                        return false;
                }
            }
            return true;
        }
    }
}
