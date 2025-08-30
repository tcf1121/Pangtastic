using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SCR_B
{
    public class BlockPlate : MonoBehaviour
    {
        [SerializeField] private Tilemap _cellPlate;
        [SerializeField] private Tilemap _spawnerPlate;
        [SerializeField] private List<TileBase> _cell;
        [SerializeField] private TileBase _spawner;

        private void Awake()
        {
            //SetBackPlate(BlockPlateSize);

            // _backPlate.transform.localScale = new Vector3(BlockPlateWidth + 0.1f, BlockPlateHeight + 0.1f, 1);

            //DrawTile();
        }
        public void DrawTile(List<CellData> cellDatas, List<Vector3Int> spawnPos)
        {
            foreach (var data in cellDatas)
                _cellPlate.SetTile(data.Position, _cell[Random.Range(0, _cell.Count - 1)]);
            foreach (var pos in spawnPos)
                _spawnerPlate.SetTile(pos, _spawner);
        }
    }
}