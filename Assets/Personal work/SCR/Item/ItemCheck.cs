
using UnityEngine;

namespace SCR
{
    public class ItemCheck : MonoBehaviour
    {
        private static ItemCheck instance;

        private ItemType? _selected = null;

        private bool _isSelect = false;

        public System.Action OnDeselected;

        public void Awake()
        {
            instance = this;
        }


        public static void Select(ItemType type)
        {
            if (instance == null)
                instance = GameObject.Find("ItemManager").GetComponent<ItemCheck>();
            instance._selected = type;
            instance._isSelect = true;
        }

        public static void Deselect()
        {
            if (instance == null)
                instance = GameObject.Find("ItemManager").GetComponent<ItemCheck>();
            instance._selected = null;
            instance._isSelect = false;
        }

        public static bool IsSelect()
        {
            if (instance == null)
                instance = GameObject.Find("ItemManager").GetComponent<ItemCheck>();
            return instance._isSelect;
        }

        public static ItemType? GetItemType()
        {
            if (instance == null)
                instance = GameObject.Find("ItemManager").GetComponent<ItemCheck>();
            return instance._selected;
        }

        private void ApplyScissor(Vector2Int pos)
        {
            //var sp = _board.Spawner;
            // int w = sp.BlockPlate.BlockPlateWidth;
            // int h = sp.BlockPlate.BlockPlateHeight;

            // for (int x = 0; x < w; x++)
            // {
            //     var blk = sp.BlockArray[pos.y, x];
            //     if (blk == null || blk.BlockInstance == null) continue;
            //     Object.Destroy(blk.BlockInstance);
            //     sp.BlockArray[pos.y, x].BlockInstance = null;
            // }

            // for (int y = 0; y < h; y++)
            // {
            //     var blk = sp.BlockArray[y, pos.x];
            //     if (blk == null || blk.BlockInstance == null) continue;
            //     Object.Destroy(blk.BlockInstance);
            //     sp.BlockArray[y, pos.x].BlockInstance = null;
            // }
        }

        private void ApplyWhisk(Vector2Int pos)
        {
            // var sp = _board.Spawner;
            // int w = sp.BlockPlate.BlockPlateWidth;
            // int h = sp.BlockPlate.BlockPlateHeight;

            // int r = 1;
            // for (int y = pos.y - r; y <= pos.y + r; y++)
            // {
            //     if (y < 0 || y >= h) continue;
            //     for (int x = pos.x - r; x <= pos.x + r; x++)
            //     {
            //         if (x < 0 || x >= w) continue;

            //         var blk = sp.BlockArray[y, x];
            //         if (blk == null || blk.BlockInstance == null) continue;

            //         Object.Destroy(blk.BlockInstance);
            //         sp.BlockArray[y, x].BlockInstance = null;
            //     }
            // }
        }
    }
}
