using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : Singleton<TimerManager>
{

    public IEnumerator StartTimer(Coroutine cor, float CycleTime, float currnetTime = 0,
     Action<float> onCorTimer = null, Action onTimerFinished = null)
    {
        float _cycle = CycleTime;
        while (true)
        {
            if (currnetTime < _cycle)
            {
                currnetTime += Time.unscaledDeltaTime;
                onCorTimer?.Invoke(currnetTime);
                yield return null;
            }
            else
                break;
        }
        onTimerFinished?.Invoke();
    }

}
