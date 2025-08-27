using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "GridCell", menuName = "Match/Tile/GridCell")]
    public class GridCell : TileBase
    {

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.colliderType = Tile.ColliderType.Grid;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif



            return base.StartUp(position, tilemap, go);
        }
    }
}
