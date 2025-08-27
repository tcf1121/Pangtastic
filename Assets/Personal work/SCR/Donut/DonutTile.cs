using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "DonutTile", menuName = "Match/Tile/Donut Tile")]
    public class DonutTile : TileBase
    {
        public Sprite PreviewEditorSprite;
        public GemType Donut;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = PreviewEditorSprite;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif


            PuzzelEditBoard.DrawObject(position, Donut);

            return true;
        }
    }
}
