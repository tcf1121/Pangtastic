using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR
{
    public class BoardCell : MonoBehaviour
    {
        public Obstacle Obstacle;

        public void RemoveCell()
        {
            if (Obstacle != null)
                Obstacle = null;
        }

        public void Damage()
        {
            if (Obstacle != null)
                Obstacle.Damage(1);
        }
    }
}

