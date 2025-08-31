using KDJ;
using SCR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

namespace SCR_B
{
    public class MatchData
    {
        public MatchType MatchType;
        public List<Vector2Int> MatchPos = new();
        public List<Vector2Int> SplashPos = new();
    }

    public enum MatchType
    {
        Nomal,
        Roller_h,
        Roller_v,
        Milk,
        DonutBox,
        Oven
    }

    public class BoardMatchChecker : MonoBehaviour
    {
        private BoardData _boardData;
        public List<MatchData> MatchDatas { get { return _matchDatas; } }
        List<MatchData> _matchDatas = new();
        List<Vector2Int> _matchPos = new();
        List<Vector2Int> _splashPos = new();
        public List<Vector2Int> SpecialDamage { get { return _specialDamage; } }
        List<Vector2Int> _specialDamage = new();

        public void SetBoardData(BoardData boardData)
        {
            _boardData = boardData;
        }

        public bool CheckArray(Vector2Int pos)
        {
            if (pos.y >= 0 && pos.y < _boardData.GetHeight() &&
                pos.x >= 0 && pos.x < _boardData.GetWidth())
            {
                if (_boardData.BlockArray[pos.y, pos.x] == null) return false;
                if (_boardData.BlockPlateArray[pos.y, pos.x]) return true;
            }

            return false;
        }

        public bool FistIsMatch()
        {
            _matchPos.Clear();
            _splashPos.Clear();
            _matchDatas.Clear();
            for (int x = 0; x < _boardData.GetWidth(); x++)
                for (int y = 0; y < _boardData.GetHeight(); y++)
                    CheckMatch(new Vector2Int(x, y));
            if (_matchDatas.Count > 0) return true;
            else return false;
        }

        public IEnumerator CheckAll()
        {
            _matchPos.Clear();
            _splashPos.Clear();
            _matchDatas.Clear();
            for (int x = 0; x < _boardData.GetWidth(); x++)
                for (int y = 0; y < _boardData.GetHeight(); y++)
                    CheckMatch(new Vector2Int(x, y));
            yield return new WaitForSeconds(0.2f);
        }

        public IEnumerator DamageAll()
        {
            CheckDamage();
            DamageMatch();
            yield return new WaitForSeconds(0.2f);
        }

        private void CheckDamage()
        {
            _matchPos.Clear();
            _splashPos.Clear();

            _matchDatas = MergeMatches(_matchDatas);
            foreach (var match in _matchDatas)
            {
                if (match.MatchType != MatchType.Nomal)
                {
                    Vector2Int matchPos;
                    if (BoardManager.GetFirstPos() != null &&
                    match.MatchPos.Contains((Vector2Int)BoardManager.GetFirstPos()))
                        matchPos = (Vector2Int)BoardManager.GetFirstPos();
                    else if (BoardManager.GetSecondPos() != null &&
                    match.MatchPos.Contains((Vector2Int)BoardManager.GetSecondPos()))
                        matchPos = (Vector2Int)BoardManager.GetSecondPos();
                    else
                    {
                        matchPos = match.MatchPos[Random.Range(0, match.MatchPos.Count)];
                        Debug.Log(matchPos);
                    }
                    BoardManager.MatchSpecial(matchPos, match.MatchType);
                }

                _matchPos.AddRange(match.MatchPos);
                _splashPos.AddRange(match.SplashPos);
            }

            _matchPos = _matchPos.Distinct().ToList();
            _splashPos = _splashPos.Distinct().ToList();

            _matchDatas.Clear();
        }

        private void CheckMatch(Vector2Int pos)
        {
            // 초기화
            _matchPos.Clear();
            _splashPos.Clear();


            // 가로 매치 확인
            int horizontalCount = CheckDirection(pos, Vector2Int.left) + CheckDirection(pos, Vector2Int.right) + 1;
            if (horizontalCount >= 3)
            {
                AddMatchToList(pos, Vector2Int.left);
                AddMatchToList(pos, Vector2Int.right);
            }
            // 세로 매치 확인
            int verticalCount = CheckDirection(pos, Vector2Int.up) + CheckDirection(pos, Vector2Int.down) + 1;
            if (verticalCount >= 3)
            {
                AddMatchToList(pos, Vector2Int.up);
                AddMatchToList(pos, Vector2Int.down);
            }
            bool squareMatch = CheckSquare(pos);
            // 매치된 블록이 있을 경우
            if (_matchPos.Count > 0)
            {
                MatchData matchData = new();

                // 중복 제거
                _matchPos = _matchPos.Distinct().OrderBy(v => v.y).ThenBy(v => v.x).ToList();
                matchData.MatchPos = new List<Vector2Int>(_matchPos);

                foreach (var matchedPos in _matchPos)
                {
                    AddSplashTargets(matchedPos);
                }

                // 스플래시 데미지 대상 리스트에서 매치 블록과 중복을 제거
                if (_splashPos.Count > 0)
                    _splashPos = _splashPos
                        .Distinct().Except(_matchPos).OrderBy(v => v.y).ThenBy(v => v.x).ToList();

                matchData.SplashPos = new List<Vector2Int>(_splashPos);
                if (horizontalCount >= 5 || verticalCount >= 5)
                {
                    matchData.MatchType = MatchType.Oven;
                }
                else
                {
                    if (horizontalCount >= 3 && verticalCount >= 3)
                    {
                        matchData.MatchType = MatchType.DonutBox;
                    }
                    else if (horizontalCount >= 4)
                    {
                        matchData.MatchType = MatchType.Roller_v;
                    }
                    else if (verticalCount >= 4)
                    {
                        matchData.MatchType = MatchType.Roller_h;
                    }
                    else if (squareMatch)
                    {
                        matchData.MatchType = MatchType.Milk;
                    }
                    else
                        matchData.MatchType = MatchType.Nomal;
                }
                AddMatchData(matchData);
            }
        }

        private void AddMatchData(MatchData newMatch)
        {
            for (int i = 0; i < _matchDatas.Count; i++)
            {
                if (newMatch.MatchPos.SequenceEqual(_matchDatas[i].MatchPos))
                {
                    if (newMatch.MatchType > _matchDatas[i].MatchType)
                    {
                        _matchDatas.RemoveAt(i);
                        _matchDatas.Add(newMatch);
                        return;
                    }
                    else return;
                }
            }
            _matchDatas.Add(newMatch);
        }

        private List<MatchData> MergeMatches(List<MatchData> originalMatches)
        {
            // 원본 리스트를 복사하여 작업합니다.
            var mergedMatches = new List<MatchData>(originalMatches);

            // 합쳐질 때까지 반복하는 루프입니다.
            bool mergedThisRound = true;
            while (mergedThisRound)
            {
                mergedThisRound = false;
                for (int i = 0; i < mergedMatches.Count; i++)
                {
                    for (int j = i + 1; j < mergedMatches.Count; j++)
                    {
                        // 두 MatchData의 MatchPos 리스트에 겹치는 요소가 있는지 확인합니다.
                        bool hasIntersection = mergedMatches[i].MatchPos.Intersect(mergedMatches[j].MatchPos).Any();
                        if (hasIntersection)
                        {
                            // 겹치는 부분이 있다면, 두 MatchData를 합칩니다.
                            // MatchPos와 SplashPos를 합치고 중복을 제거합니다.
                            var newMatch = new MatchData
                            {
                                MatchType = mergedMatches[i].MatchType > mergedMatches[j].MatchType ?
                                 mergedMatches[i].MatchType : mergedMatches[j].MatchType, // 또는 더 높은 우선순위로 결정
                                MatchPos = mergedMatches[i].MatchPos.Union(mergedMatches[j].MatchPos).ToList(),
                                SplashPos = mergedMatches[i].SplashPos.Union(mergedMatches[j].SplashPos).ToList()
                            };

                            // 합쳐진 두 MatchData를 제거하고 새로운 MatchData를 추가합니다.
                            mergedMatches.RemoveAt(j);
                            mergedMatches.RemoveAt(i);
                            mergedMatches.Add(newMatch);

                            mergedThisRound = true; // 이번 라운드에 병합이 있었음을 표시

                            // 리스트가 변경되었으므로 처음부터 다시 검사합니다.
                            i = -1; // 다음 루프에서 i가 0부터 시작하도록 초기화
                            break;
                        }
                    }
                }
            }
            return mergedMatches;
        }


        private int CheckDirection(Vector2Int startPos, Vector2Int direction)
        {
            int count = 0;
            Vector2Int currentPos = startPos + direction;
            if (!CheckArray(startPos)) return 0;
            var startGemType = _boardData.BlockArray[startPos.y, startPos.x].GemType;
            if (startGemType > GemType.Sugar) return 0;
            while (CheckArray(currentPos) &&
           _boardData.BlockArray[currentPos.y, currentPos.x].GemType == _boardData.BlockArray[startPos.y, startPos.x].GemType)
            {
                count++;
                currentPos += direction;
            }

            return count;
        }

        private void AddMatchToList(Vector2Int startPos, Vector2Int direction)
        {
            _matchPos.Add(startPos); // 시작 블록 추가
            Vector2Int currentPos = startPos + direction;

            while (CheckArray(currentPos) &&
                   _boardData.BlockArray[currentPos.y, currentPos.x].GemType == _boardData.BlockArray[startPos.y, startPos.x].GemType)
            {
                _matchPos.Add(currentPos);
                currentPos += direction;
            }
        }

        private bool CheckSquare(Vector2Int startPos)
        {
            bool isSquare = false;
            if (!CheckArray(startPos)) return false;
            var startGemType = _boardData.BlockArray[startPos.y, startPos.x].GemType;
            if (startGemType > GemType.Sugar) return false;
            bool upPos = CheckArray(startPos + Vector2Int.up) &&
            _boardData.BlockArray[startPos.y + 1, startPos.x].GemType == startGemType;
            bool downPos = CheckArray(startPos + Vector2Int.down) &&
            _boardData.BlockArray[startPos.y - 1, startPos.x].GemType == startGemType;
            bool leftPos = CheckArray(startPos + Vector2Int.left) &&
            _boardData.BlockArray[startPos.y, startPos.x - 1].GemType == startGemType;
            bool rightPos = CheckArray(startPos + Vector2Int.right) &&
            _boardData.BlockArray[startPos.y, startPos.x + 1].GemType == startGemType;

            if (upPos)
            {
                if (leftPos)
                    if (CheckArray(startPos + Vector2Int.up + Vector2Int.left) &&
            _boardData.BlockArray[startPos.y + 1, startPos.x - 1].GemType == startGemType)
                    {
                        isSquare = true;
                        _matchPos.Add(startPos);
                        _matchPos.Add(startPos + Vector2Int.up);
                        _matchPos.Add(startPos + Vector2Int.left);
                        _matchPos.Add(startPos + Vector2Int.up + Vector2Int.left);
                    }
                if (rightPos)
                    if (CheckArray(startPos + Vector2Int.up + Vector2Int.right) &&
            _boardData.BlockArray[startPos.y + 1, startPos.x + 1].GemType == startGemType)
                    {
                        isSquare = true;
                        _matchPos.Add(startPos);
                        _matchPos.Add(startPos + Vector2Int.up);
                        _matchPos.Add(startPos + Vector2Int.right);
                        _matchPos.Add(startPos + Vector2Int.up + Vector2Int.right);
                    }
            }

            if (downPos)
            {
                if (leftPos)
                    if (CheckArray(startPos + Vector2Int.down + Vector2Int.left) &&
            _boardData.BlockArray[startPos.y - 1, startPos.x - 1].GemType == startGemType)
                    {
                        isSquare = true;
                        _matchPos.Add(startPos);
                        _matchPos.Add(startPos + Vector2Int.down);
                        _matchPos.Add(startPos + Vector2Int.left);
                        _matchPos.Add(startPos + Vector2Int.down + Vector2Int.left);
                    }
                if (rightPos)
                    if (CheckArray(startPos + Vector2Int.down + Vector2Int.right) &&
            _boardData.BlockArray[startPos.y - 1, startPos.x + 1].GemType == startGemType)
                    {
                        isSquare = true;
                        _matchPos.Add(startPos);
                        _matchPos.Add(startPos + Vector2Int.down);
                        _matchPos.Add(startPos + Vector2Int.right);
                        _matchPos.Add(startPos + Vector2Int.down + Vector2Int.right);
                    }
            }

            return isSquare;
        }

        private void AddSplashTargets(Vector2Int centerPos)
        {
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            };

            foreach (var direction in directions)
            {
                Vector2Int targetPos = centerPos + direction;
                if (CheckArray(targetPos))
                {
                    if (_boardData.BlockArray[targetPos.y, targetPos.x].GemType == GemType.Flour_s)
                    {
                        AddFlour(_splashPos, targetPos);
                    }
                    else
                    {
                        _splashPos.Add(targetPos);
                    }

                }
            }
        }

        public void AddFlour(List<Vector2Int> damagePos, Vector2Int pos)
        {
            Vector2Int flourPos = (_boardData.BlockArray[pos.y, pos.x] as FlourBag_s).OwnerPos();
            damagePos.Add(flourPos);
        }

        private void DamageMatch()
        {
            _specialDamage = _specialDamage.Distinct().ToList();
            foreach (var pos in _matchPos)
            {
                Debug.Log($"{pos}, {_boardData.BlockArray[pos.y, pos.x]?.GetType().Name}");
                _boardData.BlockArray[pos.y, pos.x]?.TakeDamage();
                _boardData.OverlayArray[pos.y, pos.x]?.TakeDamage();
            }
            foreach (var pos in _splashPos)
            {
                _boardData.BlockArray[pos.y, pos.x]?.SplashDamage();
            }
            foreach (var pos in _specialDamage)
            {
                Debug.Log(pos);
                _boardData.BlockArray[pos.y, pos.x]?.TakeDamage();
                _boardData.OverlayArray[pos.y, pos.x]?.TakeDamage();
            }
            _matchPos.Clear();
            _splashPos.Clear();
            _specialDamage.Clear();
        }

        public void UseSpecial(Vector2Int pos, GemType specialType)
        {
            if (specialType == GemType.Milk)
            {
                List<GemType> targetGems = InGameManager.GetTagetGem();
                List<Vector2Int> targetPosList = _boardData.GetTargetPos(targetGems);
                System.Random random = new();

                var shuffledPos = targetPosList.OrderBy(a => random.Next()).ToList();
                var randomThree = shuffledPos.Take(3).ToList();
                if (randomThree.Count < 3)
                {
                    int addCount = 3 - randomThree.Count;
                    while (addCount > 0)
                    {
                        int x = Random.Range(0, _boardData.Size.x);
                        int y = Random.Range(0, _boardData.Size.y);
                        if (randomThree.Contains(new Vector2Int(x, y)))
                        {
                            randomThree.Add(new Vector2Int(x, y));
                            addCount--;
                        }
                    }

                }
                foreach (var targetPos in randomThree)
                    _specialDamage.Add(targetPos);
            }
            else if (specialType == GemType.Roller_v)
            {// 세로로 없애기
                for (int y = 0; y < _boardData.GetHeight(); y++)
                {
                    _specialDamage.Add(new Vector2Int(pos.x, y));
                }

            }
            else if (specialType == GemType.Roller_h)
            {
                // 가로로 없애기
                for (int x = 0; x < _boardData.GetWidth(); x++)
                {
                    _specialDamage.Add(new Vector2Int(x, pos.y));
                }
            }
            else if (specialType == GemType.DonutBox)
            {
                for (int x = pos.x - 2; x <= pos.x + 2; x++)
                    for (int y = pos.y - 2; y <= pos.y + 2; y++)
                        if (x >= 0 && x < _boardData.GetWidth() &&
                         y >= 0 && y < _boardData.GetHeight())
                            _specialDamage.Add(new Vector2Int(x, y));
            }
            else if (specialType == GemType.Oven)
            {
                Vector2Int? movePos;
                if (pos == BoardManager.GetFirstPos())
                    movePos = BoardManager.GetSecondPos();
                else
                    movePos = BoardManager.GetFirstPos();
                if (movePos != null)
                {
                    Vector2Int movedPos = (Vector2Int)movePos;
                    if (_boardData.BlockArray[movedPos.y, movedPos.x].GemType < GemType.Milk)
                    {
                        List<Vector2Int> targetPosList =
                        _boardData.GetGemTypePos(_boardData.BlockArray[movedPos.y, movedPos.x].GemType);
                        foreach (var data in targetPosList)
                            _specialDamage.Add(data);
                    }
                }
                else
                {
                    List<GemType> targetGems = InGameManager.GetTagetGem();
                    List<Vector2Int> targetPosList = _boardData.GetTargetPos(targetGems);
                    foreach (var data in targetPosList)
                        _specialDamage.Add(data);
                }

            }
        }

        public IEnumerator UseTwoSpecial(Vector2Int firstPos, Vector2Int secondPos, GemType specialType, GemType specialType2)
        {
            _boardData.BlockArray[firstPos.y, firstPos.x].Broken();
            _boardData.BlockArray[secondPos.y, secondPos.x].Broken();
            // 둘 다 우유
            if (specialType == GemType.Milk && specialType2 == GemType.Milk)
            {
                List<GemType> targetGems = InGameManager.GetTagetGem();
                List<Vector2Int> targetPosList = _boardData.GetTargetPos(targetGems);
                System.Random random = new();

                var shuffledPos = targetPosList.OrderBy(a => random.Next()).ToList();
                var randomThree = shuffledPos.Take(5).ToList();
                if (randomThree.Count < 5)
                {
                    int addCount = 5 - randomThree.Count;
                    while (addCount > 0)
                    {
                        int x = Random.Range(0, _boardData.Size.x);
                        int y = Random.Range(0, _boardData.Size.y);
                        if (randomThree.Contains(new Vector2Int(x, y)))
                        {
                            randomThree.Add(new Vector2Int(x, y));
                            addCount--;
                        }
                    }

                }
                foreach (var targetPos in randomThree)
                    _specialDamage.Add(targetPos);
                yield break;
            }

            // 둘 다 롤러
            if ((specialType == GemType.Roller_v && specialType2 == GemType.Roller_v) ||
            (specialType == GemType.Roller_h && specialType2 == GemType.Roller_h) ||
            (specialType == GemType.Roller_h && specialType2 == GemType.Roller_v) ||
            (specialType == GemType.Roller_v && specialType2 == GemType.Roller_h))
            {
                for (int y = 0; y < _boardData.GetHeight(); y++)
                {
                    _specialDamage.Add(new Vector2Int(secondPos.x, y));
                }
                for (int x = 0; x < _boardData.GetWidth(); x++)
                {
                    _specialDamage.Add(new Vector2Int(x, secondPos.y));
                }
                yield break;
            }
            // 롤러 + 우유 (세로)
            if ((specialType == GemType.Roller_v && specialType2 == GemType.Milk) ||
            (specialType == GemType.Milk && specialType2 == GemType.Roller_v))
            {
                List<GemType> targetGems = InGameManager.GetTagetGem();
                List<Vector2Int> targetPosList = _boardData.GetTargetPos(targetGems);
                System.Random random = new();

                var shuffledPos = targetPosList.OrderBy(a => random.Next()).ToList();
                var randomThree = shuffledPos.Take(5).ToList();
                if (randomThree.Count < 5)
                {
                    int addCount = 5 - randomThree.Count;
                    while (addCount > 0)
                    {
                        int x = Random.Range(0, _boardData.Size.x);
                        int y = Random.Range(0, _boardData.Size.y);
                        if (randomThree.Contains(new Vector2Int(x, y)))
                        {
                            randomThree.Add(new Vector2Int(x, y));
                            addCount--;
                        }
                    }

                }
                foreach (var milkPos in randomThree)
                {
                    for (int y = 0; y < _boardData.GetHeight(); y++)
                    {
                        _specialDamage.Add(new Vector2Int(milkPos.x, y));
                    }
                }

                yield break;
            }
            // 롤러 + 우유 (가로)
            if ((specialType == GemType.Roller_h && specialType2 == GemType.Milk) ||
            (specialType == GemType.Milk && specialType2 == GemType.Roller_h))
            {
                List<GemType> targetGems = InGameManager.GetTagetGem();
                List<Vector2Int> targetPosList = _boardData.GetTargetPos(targetGems);
                System.Random random = new();

                var shuffledPos = targetPosList.OrderBy(a => random.Next()).ToList();
                var randomThree = shuffledPos.Take(5).ToList();
                if (randomThree.Count < 5)
                {
                    int addCount = 5 - randomThree.Count;
                    while (addCount > 0)
                    {
                        int x = Random.Range(0, _boardData.Size.x);
                        int y = Random.Range(0, _boardData.Size.y);
                        if (randomThree.Contains(new Vector2Int(x, y)))
                        {
                            randomThree.Add(new Vector2Int(x, y));
                            addCount--;
                        }
                    }

                }
                foreach (var milkPos in randomThree)
                {
                    for (int x = 0; x < _boardData.GetWidth(); x++)
                    {
                        _specialDamage.Add(new Vector2Int(x, milkPos.y));
                    }
                }

                yield break;
            }


            // 둘 다 도넛 박스
            if (specialType == GemType.DonutBox && specialType2 == GemType.DonutBox)
            {
                for (int x = secondPos.x - 4; x <= secondPos.x + 4; x++)
                    for (int y = secondPos.y - 4; y <= secondPos.y + 4; y++)
                        if (x >= 0 && x < _boardData.GetWidth() &&
                         y >= 0 && y < _boardData.GetHeight())
                            _specialDamage.Add(new Vector2Int(x, y));
                yield break;
            }

            // 도넛 박스 + 우유
            if ((specialType == GemType.Milk && specialType2 == GemType.DonutBox) ||
            (specialType == GemType.DonutBox && specialType2 == GemType.Milk))
            {
                List<GemType> targetGems = InGameManager.GetTagetGem();
                List<Vector2Int> targetPosList = _boardData.GetTargetPos(targetGems);
                System.Random random = new();

                var shuffledPos = targetPosList.OrderBy(a => random.Next()).ToList();
                var randomThree = shuffledPos.Take(5).ToList();
                if (randomThree.Count < 5)
                {
                    int addCount = 5 - randomThree.Count;
                    while (addCount > 0)
                    {
                        int x = Random.Range(0, _boardData.Size.x);
                        int y = Random.Range(0, _boardData.Size.y);
                        if (randomThree.Contains(new Vector2Int(x, y)))
                        {
                            randomThree.Add(new Vector2Int(x, y));
                            addCount--;
                        }
                    }

                }

                foreach (var milkPos in randomThree)
                {
                    for (int x = milkPos.x - 2; x <= milkPos.x + 2; x++)
                        for (int y = milkPos.y - 2; y <= milkPos.y + 2; y++)
                            if (x >= 0 && x < _boardData.GetWidth() &&
                             y >= 0 && y < _boardData.GetHeight())
                                _specialDamage.Add(new Vector2Int(x, y));
                }

                yield break;
            }

            // 도넛 박스 + 우유 (세로)
            if ((specialType == GemType.Roller_v && specialType2 == GemType.DonutBox) ||
            (specialType == GemType.DonutBox && specialType2 == GemType.Roller_v))
            {
                for (int x = secondPos.x - 2; x <= secondPos.x + 2; x++)
                    for (int y = 0; y < _boardData.GetHeight(); y++)
                        if (x >= 0 && x < _boardData.GetWidth())
                            _specialDamage.Add(new Vector2Int(x, y));

                yield break;
            }

            // 도넛 박스 + 우유 (가로)
            if ((specialType == GemType.Roller_h && specialType2 == GemType.DonutBox) ||
            (specialType == GemType.DonutBox && specialType2 == GemType.Roller_h))
            {
                for (int x = 0; x <= _boardData.GetWidth(); x++)
                    for (int y = secondPos.y - 2; y <= secondPos.y + 2; y++)
                        if (y >= 0 && y < _boardData.GetHeight())
                            _specialDamage.Add(new Vector2Int(x, y));

                yield break;
            }


            // 둘 다 오븐
            if (specialType == GemType.Oven && specialType2 == GemType.Oven)
            {
                for (int x = 0; x < _boardData.GetWidth(); x++)
                    for (int y = 0; y < _boardData.GetHeight(); y++)
                        _specialDamage.Add(new Vector2Int(x, y));
                yield break;
            }

            // 오븐 + 우유
            if ((specialType == GemType.Milk && specialType2 == GemType.Oven) ||
            (specialType == GemType.Oven && specialType2 == GemType.Milk))
            {
                yield return StartCoroutine(UseOvenWith(GemType.Milk));
            }

            // 오븐 + 밀대
            if ((specialType == GemType.Roller_h && specialType2 == GemType.Oven) ||
            (specialType == GemType.Oven && specialType2 == GemType.Roller_h) ||
            (specialType == GemType.Roller_v && specialType2 == GemType.Oven) ||
            (specialType == GemType.Oven && specialType2 == GemType.Roller_v))
            {
                yield return StartCoroutine(UseOvenWith(GemType.Roller_h));
            }

            // 오븐 + 도넛 박스
            if ((specialType == GemType.DonutBox && specialType2 == GemType.Oven) ||
            (specialType == GemType.Oven && specialType2 == GemType.DonutBox))
            {
                yield return StartCoroutine(UseOvenWith(GemType.DonutBox));
            }

        }

        private IEnumerator UseOvenWith(GemType gemType)
        {
            List<GemType> targetGems = InGameManager.GetTagetGem();
            List<Vector2Int> targetPosList = _boardData.GetTargetPos(targetGems);
            GemType special = gemType;
            foreach (var data in targetPosList)
            {
                if (gemType == GemType.Roller_h)
                    special = (GemType)Random.Range(7, 9);
                _boardData.BlockArray[data.y, data.x]?.TakeDamage();
                _boardData.OverlayArray[data.y, data.x]?.TakeDamage();
                _boardData.SetArray(data.x, data.y, special);
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.2f);

            foreach (var data in targetPosList)
            {
                _specialDamage.Add(data);
            }

            yield return null;
        }
    }
}