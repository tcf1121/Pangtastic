using KDJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHJ
{
    public abstract class SpecialBlock : MonoBehaviour
    {
        public abstract void Activate(BoardManager board);
    }
}
