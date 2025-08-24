using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "DonutTile", menuName = "Match/Tile/Donut Tile")]
    public class DonutTile : TileBase
    {
        public Sprite PreviewEditorSprite;
        public Color PreviewEditorColor;
        public GemType Donut;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = !Application.isPlaying ? PreviewEditorSprite : null;
            tileData.color = !Application.isPlaying ? PreviewEditorColor : Color.white;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif


            Board.AddObject(position, Donut);

            return true;
        }
    }
}
