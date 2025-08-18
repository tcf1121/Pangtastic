namespace SCR
{
    public class FruitBasket : Special
    {
        public override bool CheckCondition()
        {
            // 가로 새로 3,3 만족
            return false;
        }

        public override void Use()
        {
            // 설명이 이해가 안됨
        }

        public override void UseWith(Special special)
        {

        }
    }
}
