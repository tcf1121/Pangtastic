using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "ObstacleTile", menuName = "2D Match/Tile/Obstacle Tile")]
    public class ObstacleTile : TileBase
    {
        public Sprite PreviewEditorSprite;
        public Color PreviewEditorColor;
        public Obstacle ObstaclePrefab;

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


            var newObstacle = Instantiate(ObstaclePrefab);
            newObstacle.Init(position);

            return true;
        }
    }
}
