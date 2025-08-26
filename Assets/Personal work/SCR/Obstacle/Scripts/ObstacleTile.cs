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
            tileData.sprite = PreviewEditorSprite;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif


            PuzzelEditBoard.DrawObject(position, Obstacle);
            if (Obstacle == GemType.CatStatues)
            {
                PuzzelEditBoard.DrawObject(new Vector3Int(position.x + 1, position.y), GemType.CatStatues_s);
                PuzzelEditBoard.DrawObject(new Vector3Int(position.x + 1, position.y + 1), GemType.CatStatues_s);
                PuzzelEditBoard.DrawObject(new Vector3Int(position.x, position.y + 1), GemType.CatStatues_s);
            }



            return true;
        }
    }
}
