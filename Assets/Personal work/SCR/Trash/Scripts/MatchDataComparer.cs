using System.Collections.Generic;
using System.Linq;

namespace SCR
{
    public class MatchDataComparer : IEqualityComparer<MatchData>
    {
        // 두 MatchData 객체가 같은지 비교
        public bool Equals(MatchData x, MatchData y)
        {
            // 둘 다 null이면 true, 하나만 null이면 false
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

            // 핵심 로직: MatchPos 리스트의 내용이 같은지 확인
            return x.MatchPos.Count == y.MatchPos.Count &&
                   x.MatchPos.OrderBy(v => v.x).SequenceEqual(y.MatchPos.OrderBy(v => v.x));
        }

        // 객체의 해시 코드 생성
        public int GetHashCode(MatchData obj)
        {
            if (ReferenceEquals(obj, null)) return 0;
            // 리스트의 모든 좌표를 기반으로 고유한 해시 코드를 만듭니다.
            int hash = 17;
            foreach (var pos in obj.MatchPos.OrderBy(v => v.x))
            {
                hash = hash * 23 + pos.GetHashCode();
            }
            return hash;
        }
    }
}