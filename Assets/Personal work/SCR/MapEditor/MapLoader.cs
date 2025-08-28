using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public class MapLoader : MonoBehaviour
    {
        [SerializeField] GameObject canvas;
        private BoardCell[] sortedItemsArray;
        [SerializeField] private int _stageNum;
        private Dictionary<Vector3Int, GemType> _mapInfo = new();
        private List<Vector3Int> _spawnPoint = new();

        string path = "Personal work/SCR/StageInfo.csv";

        void Awake()
        {
            ReadCSV(_stageNum);
        }

        public void ReadCSV(int num)
        {
            try
            {
                var splitData = File.ReadLines(Application.dataPath + "/" + path).Skip(num).Take(1).FirstOrDefault();
                string[] values = splitData.ToString().Split(',');

                GetPuzzle(values[1]);
                GetSpawnPoint(values[2]);
                Board.SetPuzzleInfo(_mapInfo, _spawnPoint);

            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading CSV file: {e.Message}");
            }
        }

        private void GetPuzzle(string value)
        {
            _mapInfo.Clear();
            string[] gem = value.Split('|');
            for (int i = 0; i < gem.Length - 1; i++)
            {
                string cellInfo = gem[i];
                string[] info = cellInfo.Split(':');
                _mapInfo.Add(GetVec3Int(info[0], info[1], info[2]), (GemType)Enum.Parse(typeof(GemType), info[3]));
            }
        }

        private void GetSpawnPoint(string value)
        {
            _spawnPoint.Clear();
            string[] gem = value.Split('|');
            for (int i = 0; i < gem.Length - 1; i++)
            {
                string cellInfo = gem[i];
                string[] info = cellInfo.Split(':');
                _spawnPoint.Add(GetVec3Int(info[0], info[1], info[2]));
            }
        }

        private Vector3Int GetVec3Int(string stringX, string stringY, string stringZ)
        {
            int x, y, z;
            bool xSuccess = int.TryParse(stringX.Trim(), out x);
            bool ySuccess = int.TryParse(stringY.Trim(), out y);
            bool zSuccess = int.TryParse(stringZ.Trim(), out z);

            if (xSuccess && ySuccess && zSuccess)
            {
                return new Vector3Int(x, y, z);
            }
            else
            {
                Debug.LogError($"일부 값이 정수 형식이 아닙니다.");
                return Vector3Int.zero;
            }
        }
    }
}

