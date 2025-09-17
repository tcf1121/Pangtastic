using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : Singleton<TimerManager>
{

    public IEnumerator StartTimer(Coroutine cor, float CycleTime,
     Action<float> onCorTimer = null, Action onTimerFinished = null)
    {
        float _cycle = CycleTime;
        while (true)
        {
            if (_cycle > 0)
            {
                _cycle -= Time.unscaledDeltaTime;
                onCorTimer?.Invoke(_cycle);
                yield return null;
            }
            else
                break;
        }
        onTimerFinished?.Invoke();
    }

}
