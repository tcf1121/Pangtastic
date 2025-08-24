using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "RandomDonutTile", menuName = "Match/Tile/Random Donut Tile")]
    public class RandomDonutTile : TileBase
    {
        public Sprite PreviewEditorSprite;
        public Color PreviewEditorColor;

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


            Board.AddObject(position, GemType.Random);

            return true;
        }
    }
}
