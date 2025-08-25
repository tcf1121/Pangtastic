using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "ObstacleTile", menuName = "Match/Tile/Obstacle Tile")]
    public class ObstacleTile : TileBase
    {
        public Sprite PreviewEditorSprite;
        public GemType Obstacle;

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


            Board.AddObject(position, Obstacle);



            return true;
        }
    }
}
