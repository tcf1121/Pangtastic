using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public class SpecialBlockEffect : MonoBehaviour
    {
        private System.Random _rand = new System.Random();
        public void UseSpecial(Vector2Int pos, GemType specialType, GameBoardData gameBoard, List<Vector2Int> outDamage)
        {
            if (gameBoard == null || outDamage == null) return;

            // Milk
            if (specialType == GemType.Milk)
            {
                AddCell(gameBoard, pos.x, pos.y, outDamage);
                var targetGems = InGameManager.GetTagetGem();
                var candidates = GetTargetPos(gameBoard, targetGems);
                Shuffle(candidates);

                // 최대 3개 선택, 부족하면 랜덤 보충
                List<Vector2Int> picks = new List<Vector2Int>(3);
                int take = Mathf.Min(3, candidates.Count);
                for (int i = 0; i < take; i++) picks.Add(candidates[i]);

                while (picks.Count < 3)
                {
                    int x = Random.Range(0, gameBoard.Width);
                    int y = Random.Range(0, gameBoard.Height);
                    Vector2Int v = new Vector2Int(x, y);
                    if (!ContainsVec(picks, v)) picks.Add(v);
                }

                for (int i = 0; i < picks.Count; i++)
                    outDamage.Add(picks[i]);
                return;
            }

            // 세로 밀대
            if (specialType == GemType.Roller_v)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                    AddCell(gameBoard, pos.x, y, outDamage);
                return;
            }

            // 가로 밀대
            if (specialType == GemType.Roller_h)
            {
                for (int x = 0; x < gameBoard.Width; x++)
                    AddCell(gameBoard, x, pos.y, outDamage);
                return;
            }

            // 도넛 박스 (5x5)
            if (specialType == GemType.DonutBox)
            {
                for (int x = pos.x - 2; x <= pos.x + 2; x++)
                    for (int y = pos.y - 2; y <= pos.y + 2; y++)
                        if (x >= 0 && x < gameBoard.Width && y >= 0 && y < gameBoard.Height)
                            AddCell(gameBoard, x, y, outDamage);
                return;
            }

            // 오븐
            if (specialType == GemType.Oven)
            {
                AddCell(gameBoard, pos.x, pos.y, outDamage);
                Vector2Int? other = null;
                // 1) BoardManager의 스왑 좌표 사용
                var bm = KDJ.BoardManager.Instance; // 싱글턴 사용
                if (bm != null && bm.BlockMover != null)
                {
                    Vector2Int start = bm.BlockMover.StartBlockPos;
                    Vector2Int end = bm.BlockMover.EndBlockPos;

                    if (pos == start) other = end;
                    else if (pos == end) other = start;
                }

                if (other == null)
                {
                    Vector2Int[] dirs = new Vector2Int[]
                    {
                        new Vector2Int(-1, 0),
                        new Vector2Int( 1, 0),
                        new Vector2Int( 0, 1),
                        new Vector2Int( 0,-1),
                    };

                    for (int i = 0; i < dirs.Length; i++)
                    {
                        int nx = pos.x + dirs[i].x;
                        int ny = pos.y + dirs[i].y;
                        if (nx < 0 || nx >= gameBoard.Width || ny < 0 || ny >= gameBoard.Height)
                            continue;

                        var nb = gameBoard.GetBlock(nx, ny);
                        if (nb != null && nb.BlockInstance != null && nb.GemType < GemType.Milk)
                        {
                            other = new Vector2Int(nx, ny);
                            break;
                        }
                    }
                }

                // 스왑 상대 타입 전체 누적
                if (other != null)
                {
                    var ob = gameBoard.GetBlock(other.Value.x, other.Value.y);
                    if (ob != null && ob.BlockInstance != null && ob.GemType < GemType.Milk)
                    {
                        var targetType = ob.GemType;

                        for (int y = 0; y < gameBoard.Height; y++)
                        {
                            for (int x = 0; x < gameBoard.Width; x++)
                            {
                                var b = gameBoard.GetBlock(x, y);
                                if (b != null && b.BlockInstance != null && b.GemType == targetType)
                                {
                                    AddCell(gameBoard, x, y, outDamage);
                                }
                            }
                        }
                    }
                }
                return;
            }
        }

        // 요구재료 좌표 목록 
        private List<Vector2Int> GetTargetPos(GameBoardData gameBoard, List<GemType> gemTypes)
        {
            var list = new List<Vector2Int>();
            for (int y = 0; y < gameBoard.Height; y++)
            {
                for (int x = 0; x < gameBoard.Width; x++)
                {
                    var blk = gameBoard.GetBlock(x, y);
                    if (blk != null && gemTypes.Contains(blk.GemType))
                        list.Add(new Vector2Int(x, y));
                }
            }
            return list;
        }

        // 점수 및 콤보 시간
        public int ApplyDamageAndScore(KDJ.BoardManager board, List<Vector2Int> hits)
        {
            if (board == null || hits == null) return 0;

            var gameBoard = board.Spawner.GameBoardData;

            // 중복 좌표 제거
            List<Vector2Int> unique = new List<Vector2Int>();
            for (int i = 0; i < hits.Count; i++)
            {
                var v = hits[i];
                bool exists = false;
                for (int j = 0; j < unique.Count; j++)
                {
                    if (unique[j].x == v.x && unique[j].y == v.y)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists) unique.Add(v);
            }

            int destroyedCount = 0;

            // 실제 파괴
            for (int i = 0; i < unique.Count; i++)
            {
                var c = unique[i];
                var b = gameBoard.GetBlock(c.x, c.y);
                if (b != null && b.BlockInstance != null)
                {
                    if (b.GemType < GemType.Milk)
                        InGameManager.AddIngredientSta(b.GemType);

                    if (b is ObstacleBlock ob)
                    {
                        ob.TakeDamage();
                        continue;
                    }
                    
                    Object.Destroy(b.BlockInstance);
                    gameBoard.SetBlock(c.x, c.y, null);
                    destroyedCount++;
                }
            }

            // 점수: 부숴진 개수 × 10
            if (destroyedCount > 0)
            {
                int score = destroyedCount * 10;
                board.UpdateUI(score);
            }

            // 콤보 타이머 유지
            if (board.MatchCombo != null)
                board.MatchCombo.ResetTimer();

            return destroyedCount;
        }

        private void Shuffle(List<Vector2Int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rand.Next(i + 1);
                Vector2Int tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        private bool ContainsVec(List<Vector2Int> list, Vector2Int v)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].x == v.x && list[i].y == v.y) return true;
            return false;
        }

        private void AddCell(GameBoardData gb, int x, int y, List<Vector2Int> outDamage, bool includeWaitingRow = false)
        {
            if (x < 0 || x >= gb.Width) return;
            int hLimit = includeWaitingRow ? gb.Height + 1 : gb.Height; // 대기열 포함 여부
            if (y < 0 || y >= hLimit) return;

            // 플레이 영역(y < Height)에서는 플레이트가 true인 칸만
            if (y < gb.Height && !gb.BlockPlate.BlockPlateArray[y, x]) return;

            outDamage.Add(new Vector2Int(x, y));
        }
    }
}
