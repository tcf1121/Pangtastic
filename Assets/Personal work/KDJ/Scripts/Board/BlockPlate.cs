using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace KDJ
{
    public class BlockPlate : MonoBehaviour
    {
        [SerializeField] private Tilemap _blockPlate;
        [SerializeField] private TileBase _blockTiles;
        [SerializeField] private List<TileBase> _blockTileOutlines;
        [SerializeField] private TileBase _emptyTile;
        [SerializeField] private GameObject _backPlate;

        public bool[,] BlockPlateArray;
        public int BlockPlateWidth;
        public int BlockPlateHeight;
        [Header("블록 판 설정 (1 = 6x6, 2 = 7x7, 3 = 8x8)")]
        public int BlockPlateSize;


        private void Awake()
        {
            //SetBackPlate(BlockPlateSize);

            // _backPlate.transform.localScale = new Vector3(BlockPlateWidth + 0.1f, BlockPlateHeight + 0.1f, 1);

            //DrawTile();
        }
        public void DrawTile()
        {
            BlockPlateWidth = BlockPlateArray.GetLength(1);
            BlockPlateHeight = BlockPlateArray.GetLength(0);

            for (int x = 0; x < BlockPlateArray.GetLength(1); x++)
            {
                for (int y = 0; y < BlockPlateArray.GetLength(0); y++)
                {
                    TileBase tile;

                    if (!BlockPlateArray[y, x])
                        tile = _emptyTile;
                    else
                        tile = _blockTiles;

                    // 블록판 가로칸이 짝수일때 생성
                    if (BlockPlateArray.GetLength(1) % 2 == 0)
                    {

                        _blockPlate.SetTile(new Vector3Int(x - BlockPlateWidth / 2, y - BlockPlateHeight / 2, 0), tile);
                        _blockPlate.transform.position = new Vector3(0, 0, 0);

                    }
                    else
                    {

                        _blockPlate.SetTile(new Vector3Int(x - BlockPlateWidth / 2, y - BlockPlateHeight / 2, 0), tile);
                        _blockPlate.transform.position = new Vector3(-0.5f, -0.5f, 0);

                    }
                }
            }
        }

        public void SetOutlineTile()
        {
            // 현재 보드 크기에 맞게 테두리 타일 설정
            // 0 = UL, 1 = U, 2 = UR, 3 = R, 4 = DR, 5 = D, 6 = DL, 7 = L
            TileBase UL = _blockTileOutlines[0];
            TileBase U = _blockTileOutlines[1];
            TileBase UR = _blockTileOutlines[2];
            TileBase R = _blockTileOutlines[3];
            TileBase DR = _blockTileOutlines[4];
            TileBase D = _blockTileOutlines[5];
            TileBase DL = _blockTileOutlines[6];
            TileBase L = _blockTileOutlines[7];

            // 보드 크기가 짝수일 때
            if (BlockPlateWidth % 2 == 0)
            {
                // 각 배치는 보드 크기보다 1칸 더 크거나 작음(테두리 이기에)
                // 먼저 각 모서리에 해당하는 타일 배치
                _blockPlate.SetTile(new Vector3Int(-BlockPlateWidth / 2 - 1, BlockPlateHeight / 2, 0), UL);
                _blockPlate.SetTile(new Vector3Int(BlockPlateWidth / 2, BlockPlateHeight / 2, 0), UR);
                _blockPlate.SetTile(new Vector3Int(-BlockPlateWidth / 2 - 1, -BlockPlateHeight / 2 - 1, 0), DL);
                _blockPlate.SetTile(new Vector3Int(BlockPlateWidth / 2, -BlockPlateHeight / 2 - 1, 0), DR);
                // 그 다음 각 변에 해당하는 타일 배치
                for (int x = -BlockPlateWidth / 2; x < BlockPlateWidth / 2; x++)
                {
                    _blockPlate.SetTile(new Vector3Int(x, BlockPlateHeight / 2, 0), U);
                    _blockPlate.SetTile(new Vector3Int(x, -BlockPlateHeight / 2 - 1, 0), D);
                }
                for (int y = -BlockPlateHeight / 2; y < BlockPlateHeight / 2; y++)
                {
                    _blockPlate.SetTile(new Vector3Int(-BlockPlateWidth / 2 - 1, y, 0), L);
                    _blockPlate.SetTile(new Vector3Int(BlockPlateWidth / 2, y, 0), R);
                }
            }
            // 보드 크기가 홀수일 때
            else
            {
                // 각 배치는 보드 크기보다 1칸 더 크거나 작음(테두리 이기에)
                // 먼저 각 모서리에 해당하는 타일 배치
                _blockPlate.SetTile(new Vector3Int(-BlockPlateWidth / 2 - 1, BlockPlateHeight / 2 + 1, 0), UL);
                _blockPlate.SetTile(new Vector3Int(BlockPlateWidth / 2 + 1, BlockPlateHeight / 2 + 1, 0), UR);
                _blockPlate.SetTile(new Vector3Int(-BlockPlateWidth / 2 - 1, -BlockPlateHeight / 2 - 1, 0), DL);
                _blockPlate.SetTile(new Vector3Int(BlockPlateWidth / 2 + 1, -BlockPlateHeight / 2 - 1, 0), DR);
                // 그 다음 각 변에 해당하는 타일 배치
                for (int x = -BlockPlateWidth / 2; x <= BlockPlateWidth / 2; x++)
                {
                    _blockPlate.SetTile(new Vector3Int(x, BlockPlateHeight / 2 + 1, 0), U);
                    _blockPlate.SetTile(new Vector3Int(x, -BlockPlateHeight / 2 - 1, 0), D);
                }
                for (int y = -BlockPlateHeight / 2; y <= BlockPlateHeight / 2; y++)
                {
                    _blockPlate.SetTile(new Vector3Int(-BlockPlateWidth / 2 - 1, y, 0), L);
                    _blockPlate.SetTile(new Vector3Int(BlockPlateWidth / 2 + 1, y, 0), R);
                }
            }
        }

        public TileBase GetRandomBlockTile()
        {
            return _blockTiles;
        }

        public bool IsBlockTile(int x, int y)
        {
            Vector3 position = new Vector3(x - BlockPlateWidth / 2, y - BlockPlateHeight / 2, 0);
            Vector3Int cellPos = _blockPlate.WorldToCell(position);
            return _blockPlate.HasTile(cellPos);
        }
    }
}