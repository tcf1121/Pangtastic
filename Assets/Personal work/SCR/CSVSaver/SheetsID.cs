using System.Collections.Generic;
using UnityEngine;


namespace SCR
{
    [CreateAssetMenu(fileName = "SheetsID", menuName = "CSV/SheetsID")]
    public class SheetsID : ScriptableObject
    {
        [System.Serializable]
        public class SheetInfo
        {
            public string sheetname;
            public string name;
            public string id;
        }

        public List<SheetInfo> sheetInfos;
    }
}