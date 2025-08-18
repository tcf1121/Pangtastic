using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlockPlate : MonoBehaviour
{
    [SerializeField] private Tilemap _blockPlate;
    [SerializeField] private TileBase _blockTile;
    [SerializeField] private GameObject _backPlate;

    public bool[,] BlockPlateArray;
    public int BlockPlateWidth;
    public int BlockPlateHeight;


    private void Awake()
    {
        BlockPlateArray = new bool[,]
        {
            { true, true, false, false, true, true },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { true, true, false, false, true, true }
        };

        // _backPlate.transform.localScale = new Vector3(BlockPlateWidth + 0.1f, BlockPlateHeight + 0.1f, 1);

        DrawTile();
    }
    private void DrawTile()
    {
        for (int x = 0; x < BlockPlateArray.GetLength(0); x++)
        {
            for (int y = 0; y < BlockPlateArray.GetLength(1); y++)
            {
                if (BlockPlateArray[y, x])
                {
                    _blockPlate.SetTile(new Vector3Int(x - BlockPlateWidth / 2, y - BlockPlateHeight / 2, 0), _blockTile);
                }
            }
        }
    }
}
