using KDJ.States;
using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class Milk : SpecialBlock
    {
        [SerializeField] private bool _destroySpecial;

        [Header("재료 매핑")]
        [SerializeField] private IngredientSO _strawberry;
        [SerializeField] private IngredientSO _cheese;
        [SerializeField] private IngredientSO _chocolate;
        [SerializeField] private IngredientSO _blueberry;
        [SerializeField] private IngredientSO _lavender;

        private Dictionary<GemType, IngredientSO> _ingredientMap;

        private void Awake()
        {
            _ingredientMap = new Dictionary<GemType, IngredientSO>
            {
                { GemType.Strawberry, _strawberry },
                { GemType.Cheese, _cheese },
                { GemType.Chocolate, _chocolate },
                { GemType.Blueberry, _blueberry },
                { GemType.Lavender, _lavender }
            };
        }

        public override void Activate(BoardManager board)
        {
            if (board == null || board.Spawner == null) return;
            if (SpecialBlockCombo.Instance != null &&
                SpecialBlockCombo.Instance.TryResolveFromActivate(board, this.gameObject))
            {
                return;
            }

            var spawner = board.Spawner;
            var plate = spawner.GameBoardData.BlockPlate;
            int width = plate.BlockPlateWidth;
            int height = plate.BlockPlateHeight;

            Vector2Int myPos = new Vector2Int(
                Mathf.RoundToInt(transform.position.x + width / 2f - 0.5f),
                Mathf.RoundToInt(transform.position.y + height / 2f - 0.5f)
            );

            if (myPos.y >= 0 && myPos.y < height && myPos.x >= 0 && myPos.x < width)
            {
                var selfBlk = spawner.GameBoardData.BlockArray[myPos.y, myPos.x];
                if (selfBlk != null && selfBlk.BlockInstance != null)
                {
                    Object.Destroy(selfBlk.BlockInstance);
                    spawner.GameBoardData.BlockArray[myPos.y, myPos.x].BlockInstance = null;
                }
            }

            List<Vector2Int> candidates = new List<Vector2Int>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var blk = spawner.GameBoardData.BlockArray[y, x];
                    if (blk == null || blk.BlockInstance == null) continue;
                    if (x == myPos.x && y == myPos.y) continue;
                    if (!_ingredientMap.ContainsKey(blk.GemType)) continue;

                    if (_destroySpecial)
                    {
                        var special = blk.BlockInstance.GetComponent<SpecialBlock>();
                        if (special != null) continue;
                    }

                    candidates.Add(new Vector2Int(x, y));
                }
            }

            var order = GameObject.FindObjectOfType<OrderStateController>();
            int destroyedCount = 0;
            int toRemove = Mathf.Min(3, candidates.Count);
            for (int i = 0; i < toRemove; i++)
            {
                int idx = Random.Range(0, candidates.Count);
                Vector2Int pos = candidates[idx];
                candidates.RemoveAt(idx);

                var blk = spawner.GameBoardData.BlockArray[pos.y, pos.x];

                if (order != null)
                    order.AddIngredient(_ingredientMap[blk.GemType]);

                Object.Destroy(blk.BlockInstance);
                spawner.GameBoardData.BlockArray[pos.y, pos.x].BlockInstance = null;
                destroyedCount++;
            }
            if (destroyedCount > 0)
            {
                int score = destroyedCount * 10;
                //board.UpdateUI(score);
            }
            if (board.MatchCombo != null)
            {
                board.MatchCombo.ResetTimer();
            }
        }
    }
}
