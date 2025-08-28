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
        [SerializeField] private GameObject[] _specialPrefabs;

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
                _board.UpdateUI(destroyed * 10);
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
            if (_specialPrefabs == null || _specialPrefabs.Length == 0) return;

            for (int t = 0; t < 100; t++)
            {
                int x = Random.Range(0, w);
                int y = Random.Range(0, h);

                var cell = sp.BlockArray[y, x];
                if (cell == null || cell.BlockInstance == null) continue;

                Destroy(cell.BlockInstance);
                var prefab = _specialPrefabs[Random.Range(0, _specialPrefabs.Length)];
                var go = Instantiate(prefab);

                go.transform.position = new Vector3(
                    x - (w / 2f) + 0.5f,
                    y - (h / 2f) + 0.5f,
                    0f
                );

                sp.BlockArray[y, x].BlockInstance = go;
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
