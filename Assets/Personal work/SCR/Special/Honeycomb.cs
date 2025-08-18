namespace SCR
{
    public class Honeycomb : Special
    {
        public override bool CheckCondition()
        {
            // 새로 또는 가로로 5줄
            return false;
        }

        public override void Use()
        {
            // 해당 재료 제거
        }

        public override void UseWith(Special special)
        {

        }
    }
}
