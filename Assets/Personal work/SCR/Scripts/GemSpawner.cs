using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "GemSpawner", menuName = "Match/Tile/GemSpawner")]
    public class GemSpawner : TileBase
    {
        public Sprite PreviewEditorSprite;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = !Application.isPlaying ? PreviewEditorSprite : null;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif


            Board.AddSpawner(position);

            return base.StartUp(position, tilemap, go);
        }
    }
}
