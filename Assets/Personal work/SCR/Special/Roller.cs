using UnityEngine;

namespace SCR
{
    public class Roller : Special
    {

        public override void Init(Vector3Int cell, bool isHorizon)
        {
            base.Init(cell, isHorizon);
            if (_isHorizon) transform.GetChild(0).rotation = Quaternion.Euler(0, 0, 90);
        }

        public override void Use(Special special = null)
        {

        }
    }
}
