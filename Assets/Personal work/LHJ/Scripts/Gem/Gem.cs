using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class Gem : MonoBehaviour
    {
        private Vector2Int _cellPos;
        private TestMatchMap _map;

        public BlockNum BlockType { get; private set; }

        public void Init(TestMatchMap map, Vector2Int pos, BlockNum type)
        {
            _map = map;
            _cellPos = pos;
            BlockType = type;
        }

        public Vector2Int GetPosition() => _cellPos;

        public void SetPosition(Vector2Int newPos)
        {
            _cellPos = newPos;
        }
    }
}