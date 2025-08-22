using KDJ;
using LHJ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialBlockTest : MonoBehaviour
{
    private BoardManager _board;

    private void Awake()
    {
        _board = FindObjectOfType<BoardManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 씬에 존재하는 모든 SpecialBlock 찾아서 발동
            var specials = FindObjectsOfType<SpecialBlock>();
            foreach (var sp in specials)
            {
                sp.Activate(_board);
            }
            Debug.Log($"[Trigger] 1번 키 입력 → {specials.Length}개의 특수블럭 Activate 실행");
        }
    }
}
