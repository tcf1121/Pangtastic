using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR_B
{
    public class BlockMover : MonoBehaviour
    {
        private BoardData _boardData;
        // Start is called before the first frame update
        public void SetBoardData(BoardData boardData)
        {
            _boardData = boardData;
        }

        public void Move(Vector2Int pos, Vector2Int pos2)
        {
            Vector2Int firstPos = pos + new Vector2Int(_boardData.GetHeight(), _boardData.GetWidth()) + _boardData.ZeroPos;
            Vector2Int secondPos = pos2 + new Vector2Int(_boardData.GetHeight(), _boardData.GetWidth()) + _boardData.ZeroPos;
            Debug.Log($"{firstPos}{secondPos}");
            var block = _boardData.BlockArray[secondPos.y, secondPos.x].Clone();
            var block2 = _boardData.BlockArray[firstPos.y, firstPos.x].Clone();
            _boardData.BlockArray[firstPos.y, firstPos.x] = block;
            _boardData.BlockArray[secondPos.y, secondPos.x] = block2;
            block.BlockInstance.transform.position = BoardManager.GetWorldPos(firstPos.x, firstPos.y);
            block.Pos = firstPos;
            block2.BlockInstance.transform.position = BoardManager.GetWorldPos(secondPos.x, secondPos.y);
            block2.Pos = secondPos;

            StartCoroutine(BoardManager.HandleTurn());
        }
    }
}