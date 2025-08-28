using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _score = 0;
    public void AddScore(int score)
    {
        _score += score;
        Debug.Log($"현재 점수: {_score}");
    }

    public void ResetScore()
    {
        _score = 0;
        Debug.Log($"점수 초기화. 현재 점수: {_score}");
    }
}
