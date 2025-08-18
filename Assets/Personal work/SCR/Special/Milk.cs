namespace SCR
{
    public class Milk : Special
    {
        public override bool CheckCondition()
        {
            // 사각형 모양으로 4개
            return false;
        }

        public override void Use()
        {
            // 재료 블록 랜덤 3개 제거
        }

        public override void UseWith(Special special)
        {

        }
    }
}
