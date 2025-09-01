using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class GemToggle : MonoBehaviour
    {
        public Toggle Toggle { get => toggle; }
        [SerializeField] Toggle toggle;
        [SerializeField] TileList tile;
        [SerializeField] Image image;
        [SerializeField] int _num;

        public void SetToggle(int num)
        {
            _num = num;
            SetImage();
            toggle.onValueChanged.AddListener(ChageGem);
        }

        void ChageGem(bool value)
        {
            if (value)
            {
                PuzzelEditBoard.SelectTile(_num);
            }
        }

        void SetImage()
        {
            image.sprite = tile.tiles[_num].Sprite;
        }
    }
}