using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : Singleton<TimerManager>
{
    public Coroutine ADCor;
    public Action ADFin;
    private bool _isGame;

    protected override void Awake()
    {
        base.Awake();
        _isGame = false;
        ADFin += EndADTime;
    }

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

    public void StartGame()
    {
        _isGame = true;
        if (ADCor == null) StartADTimer(180, ADFin);
    }

    public void EndGame()
    {
        _isGame = false;
    }

    private void EndADTime()
    {
        if (ADCor != null)
        {
            StopCoroutine(ADCor);
            ADCor = null;
        }
    }

    private IEnumerator StartADTimer(float CycleTime,
     Action onTimerFinished = null)
    {
        float _cycle = CycleTime;
        while (true)
        {
            if (_cycle > 0)
            {
                if (_isGame)
                {
                    _cycle -= Time.unscaledDeltaTime;
                }
                yield return null;
            }
            else
                break;
        }
        onTimerFinished?.Invoke();
    }

}
