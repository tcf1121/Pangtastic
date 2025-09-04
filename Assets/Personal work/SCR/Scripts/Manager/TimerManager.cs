using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : Singleton<TimerManager>
{

    public IEnumerator StartTimer(Coroutine cor, float CycleTime, float currnetTime = 0,
     Action<float> onTimerFinished = null)
    {
        float _cycle = CycleTime;
        while (true)
        {
            if (currnetTime < _cycle)
            {
                currnetTime += Time.unscaledDeltaTime;
                onTimerFinished?.Invoke(currnetTime);
                yield return null;
            }
            else
                break;
        }
        StopCoroutine(cor);
        cor = null;
    }

}
