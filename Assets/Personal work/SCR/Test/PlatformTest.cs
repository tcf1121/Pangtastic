using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCR.Test
{
    public class PlatformTest : MonoBehaviour
    {
#if UNITY_EDITOR
        public string path = Application.dataPath;

#else
        public string path = Application.persistentDataPath;
#endif

        private void Start()
        {
            Debug.Log($"{path} 에서 데이터를 로딩합니다.");
        }
    }
}

