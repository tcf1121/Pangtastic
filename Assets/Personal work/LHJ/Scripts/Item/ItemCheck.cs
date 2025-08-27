using KDJ;
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

        private ItemType? _selected = null;

        public System.Action OnDeselected;

        public void Select(ItemType type)
        {
            _selected = type;
        }

        public void Deselect()
        {
            _selected = null;
            if (OnDeselected != null) OnDeselected();
        }

        private void Update()
        {
            if (!_selected.HasValue) return;
            if (Input.GetMouseButtonDown(0))
            {
                // UI 위 클릭이면 무시
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                if (!CanUseNow()) return;
                Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                TryUseWorld(world);
            }
        }

        private bool CanUseNow()
        {
            return _board != null;
        }

        public void TryUseWorld(Vector3 world)
        {
            if (_board == null || !_selected.HasValue) return;

            Vector2Int cell = WorldToCell(world);
            if (!InBounds(cell))
            {
                Deselect();
                return;
            }

            UseCell(cell);
            Deselect();
        }

        private bool InBounds(Vector2Int p)
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;
            return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;
        }

        private Vector2Int WorldToCell(Vector3 world)
        {
            int w = _board.Spawner.BlockPlate.BlockPlateWidth;
            int h = _board.Spawner.BlockPlate.BlockPlateHeight;
            int x = Mathf.RoundToInt(world.x + w / 2f - 0.5f);
            int y = Mathf.RoundToInt(world.y + h / 2f - 0.5f);
            return new Vector2Int(x, y);
        }

        private void UseCell(Vector2Int pos)
        {
            switch (_selected.Value)
            {
                case ItemType.Scissor:
                    ApplyScissor(pos);
                    break;
                case ItemType.Whisk:
                    ApplyWhisk(pos);
                    break;
            }
        }

        private void ApplyScissor(Vector2Int pos)
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;

            for (int x = 0; x < w; x++)
            {
                var blk = sp.BlockArray[pos.y, x];
                if (blk == null || blk.BlockInstance == null) continue;
                Object.Destroy(blk.BlockInstance);
                sp.BlockArray[pos.y, x].BlockInstance = null;
            }

            for (int y = 0; y < h; y++)
            {
                var blk = sp.BlockArray[y, pos.x];
                if (blk == null || blk.BlockInstance == null) continue;
                Object.Destroy(blk.BlockInstance);
                sp.BlockArray[y, pos.x].BlockInstance = null;
            }
        }

        private void ApplyWhisk(Vector2Int pos)
        {
            var sp = _board.Spawner;
            int w = sp.BlockPlate.BlockPlateWidth;
            int h = sp.BlockPlate.BlockPlateHeight;

            int r = 1;
            for (int y = pos.y - r; y <= pos.y + r; y++)
            {
                if (y < 0 || y >= h) continue;
                for (int x = pos.x - r; x <= pos.x + r; x++)
                {
                    if (x < 0 || x >= w) continue;

                    var blk = sp.BlockArray[y, x];
                    if (blk == null || blk.BlockInstance == null) continue;

                    Object.Destroy(blk.BlockInstance);
                    sp.BlockArray[y, x].BlockInstance = null;
                }
            }
        }
    }
}
