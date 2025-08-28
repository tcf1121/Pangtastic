using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KDJ
{
    public class BoardData
    {
        public bool[,] BlockPlateArray;
        public Block[,] BlockArray;
    }

    public class BoardLoader : MonoBehaviour
    {
        private string _path = "Personal work/SCR/StageInfo.csv";
        private int _boardWidth;
        private int _boardHeight;

        public BoardData BoardDataArray;

        public BoardData LoadBoard(string value)
        {
            string[] values = value.Split('|');
            BoardData boardDataArray = new BoardData
            {
                BlockPlateArray = new bool[_boardHeight, _boardWidth],
                BlockArray = new Block[_boardHeight, _boardWidth]
            };
            for (int i = 0; i < values.Length; i++)
            {
                // csv 저장할 때 가로 세로 길이 같이 넣어줄 수 있는지 물어보기
                string[] cellValues = values[i].Split(':');
                int x = int.Parse(cellValues[0]);
                int y = int.Parse(cellValues[1]);
                if (cellValues[3] == "이곳에 비교할 데이터를 적으시오")
                {
                    boardDataArray.BlockPlateArray[y, x] = true;
                    boardDataArray.BlockArray[y, x] = new Block();
                    // 블록 데이터 설정
                }
            }
            return boardDataArray;
        }

        public BoardData ReadCSV(int num)
        {
            try
            {
                var splitData = File.ReadLines(Application.dataPath + "/" + _path).Skip(num).Take(1).FirstOrDefault();
                string[] values = splitData.ToString().Split(',');
                _boardWidth = int.Parse(values[0]);
                _boardHeight = int.Parse(values[1]);
                BoardDataArray = LoadBoard(values[2]);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading CSV file: {e.Message}");
            }

            return BoardDataArray;
        }
    }
}
