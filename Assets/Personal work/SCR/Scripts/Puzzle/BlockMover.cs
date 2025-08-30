using SCR;
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

        public IEnumerator Move(Vector2Int firstPos, Vector2Int secondPos, bool check = true)
        {
            Debug.Log($"{firstPos}{secondPos}");
            var block = _boardData.BlockArray[secondPos.y, secondPos.x].Clone();
            var block2 = _boardData.BlockArray[firstPos.y, firstPos.x].Clone();
            _boardData.BlockArray[firstPos.y, firstPos.x] = block;
            _boardData.BlockArray[secondPos.y, secondPos.x] = block2;
            block.Pos = firstPos;
            block2.Pos = secondPos;
            StartCoroutine(MovePosCor(block.BlockInstance.transform,
                 BoardManager.GetWorldPos(firstPos.x, firstPos.y), 0.3f));
            StartCoroutine(MovePosCor(block2.BlockInstance.transform,
            BoardManager.GetWorldPos(secondPos.x, secondPos.y), 0.3f));
            yield return new WaitForSeconds(0.3f);
            if (!BoardManager.FistIsMatch() && check)
                StartCoroutine(Move(firstPos, secondPos, false));
            else if (check)
                StartCoroutine(BoardManager.HandleTurn());
        }

        private IEnumerator MovePosCor(Transform gameObject, Vector3 targetPos, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                if (gameObject != null)
                    gameObject.position = Vector3.Lerp(gameObject.position, targetPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            if (gameObject != null)
                gameObject.position = targetPos;
        }

        public void Select(Vector2Int pos, bool select)
        {
            _boardData.BlockArray[pos.y, pos.x].BlockInstance.GetComponent<GemPrefab>().HighLight(select);
        }

        public void DobleSelect(Vector2Int pos)
        {

            if (_boardData.BlockArray[pos.y, pos.x].GemType > GemType.Sugar &&
            _boardData.BlockArray[pos.y, pos.x].GemType < GemType.Dust)
            {
                Debug.Log("아이템 사용");
                _boardData.BlockArray[pos.y, pos.x].TakeDamage();
            }

            StartCoroutine(BoardManager.HandleTurn());
        }


    }
}