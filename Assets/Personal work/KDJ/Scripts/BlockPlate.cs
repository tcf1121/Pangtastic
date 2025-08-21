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
        [SerializeField] private TileBase _blockTile;
        [SerializeField] private GameObject _backPlate;

        public bool[,] BlockPlateArray;
        public int BlockPlateWidth;
        public int BlockPlateHeight;
        [Header("블록 판 설정 (1 = 6x6, 2 = 7x7, 3 = 8x8)")]
        public int BlockPlateSize;


        private void Awake()
        {
            SetBackPlate(BlockPlateSize);

            // _backPlate.transform.localScale = new Vector3(BlockPlateWidth + 0.1f, BlockPlateHeight + 0.1f, 1);

            DrawTile();
        }
        private void DrawTile()
        {
            for (int x = 0; x < BlockPlateArray.GetLength(1); x++)
            {
                for (int y = 0; y < BlockPlateArray.GetLength(0); y++)
                {
                    // 블록판 가로칸이 짝수일때 생성
                    if (BlockPlateArray.GetLength(1) % 2 == 0)
                    {
                        if (BlockPlateArray[y, x])
                        {
                            _blockPlate.SetTile(new Vector3Int(x - BlockPlateWidth / 2, y - BlockPlateHeight / 2, 0), _blockTile);
                            _blockPlate.transform.position = new Vector3(0, 0, 0);
                        }
                    }
                    else
                    {
                        if (BlockPlateArray[y, x])
                        {
                            _blockPlate.SetTile(new Vector3Int(x - BlockPlateWidth / 2, y - BlockPlateHeight / 2, 0), _blockTile);
                            _blockPlate.transform.position = new Vector3(-0.5f, -0.5f, 0);
                        }
                    }
                }
            }
        }

        private void SetBackPlate(int value)
        {
            switch (value)
            {
                case 1:
                    BlockPlateArray = new bool[,]
                    {
                        { true, true, true, true, true, true },
                        { true, true, true, true, true, true },
                        { true, true, true, true, true, true },
                        { true, true, true, true, true, true },
                        { true, true, true, true, true, true },
                        { true, true, true, true, true, true }
                    };
                    break;
                case 2:
                    BlockPlateArray = new bool[,]
                    {
                        { true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true }
                    };
                    break;
                case 3:
                    BlockPlateArray = new bool[,]
                    {
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true }
                    };
                    break;
                case 4:
                    BlockPlateArray = new bool[,]
                    {
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true },
                        { true, true, true, true, true, true, true, true }
                    };
                    break;
            }

            BlockPlateWidth = BlockPlateArray.GetLength(1);
            BlockPlateHeight = BlockPlateArray.GetLength(0);
        }

        public bool IsBlockTile(int x, int y)
        {
            Vector3 position = new Vector3(x - BlockPlateWidth / 2, y - BlockPlateHeight / 2, 0);
            Vector3Int cellPos = _blockPlate.WorldToCell(position);
            return _blockPlate.HasTile(cellPos);
        }
    }
}