using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR
{
    [CreateAssetMenu(fileName = "EmptyCell", menuName = "Match/Tile/EmptyCell")]
    public class EmptyCell : TileBase
    {
        public Sprite PreviewEditorSprite;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = PreviewEditorSprite;
            //tileData.colliderType = Tile.ColliderType.Grid;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif

            if (GameObject.Find("Puzzle").GetComponent<PuzzelEditBoard>())
                PuzzelEditBoard.AddCell(position);
            else
                Board.AddCell(position);
            return base.StartUp(position, tilemap, go);
        }
    }
}
