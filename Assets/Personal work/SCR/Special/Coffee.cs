namespace SCR
{
    public class Coffe : Special
    {
        public override bool CheckCondition()
        {
            //직선 3개 매치시 3% 확률로 등장
            return false;
        }

        public override void Use()
        {
            // 손님 인내심 10% 증가
        }

        public override void UseWith(Special special)
        {

        }
    }
}
