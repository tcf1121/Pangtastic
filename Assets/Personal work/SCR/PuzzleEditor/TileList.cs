using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace SCR
{
    [CreateAssetMenu(fileName = "Tile List", menuName = "Match/Tile List")]
    public class TileList : ScriptableObject
    {
        public List<TileInfo> tiles;
        public TileBase Eraser()
        {
            return tiles[tiles.Count - 1].Tile;
        }

        public TileBase Spawner()
        {
            return tiles[tiles.Count - 4].Tile;
        }

        public TileBase Cell()
        {
            return tiles[tiles.Count - 3].Tile;
        }

        public TileBase CatStatues()
        {
            return tiles[tiles.Count - 5].Tile;
        }


    }
    [Serializable]
    public class TileInfo
    {
        public TileBase Tile;
        public Sprite Sprite;
    }
}
