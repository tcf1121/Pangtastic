using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KDJ.States;

namespace KDJ
{
    public class HintManager : MonoBehaviour
    {
        [SerializeField] private float _hintDelay = 3f;
        private Coroutine _hintCoroutine;
        private BoardManager _boardManager;

        private void Awake()
        {
            _boardManager = GetComponent<BoardManager>();
        }

        public void StartHintTimer()
        {
            if (_hintCoroutine == null)
            {
                _hintCoroutine = StartCoroutine(HintLifecycleCoroutine());
            }
        }

        public void StopHintTimer()
        {
            if (_hintCoroutine != null)
            {
                StopCoroutine(_hintCoroutine);
                _hintCoroutine = null;
            }
            _boardManager.HideHint();
        }

        private IEnumerator HintLifecycleCoroutine()
        {
            while (true)
            {
                if (!(_boardManager.CurrentState is ReadyState) || _boardManager.MatchCombo.CurCombo != 0)
                {
                    yield return null;
                    continue;
                }

                Debug.Log("힌트 도는 중");

                yield return new WaitForSeconds(_hintDelay);

                List<Vector2Int> hintPositions = _boardManager.MatchChecker.OptimalMatchFind();
                if (hintPositions != null && hintPositions.Count > 0)
                {
                    Debug.Log("힌트 표시");
                    _boardManager.CurHintPositions = hintPositions;
                    _boardManager.ShowHint(hintPositions);

                    yield return new WaitForSeconds(_hintDelay);
                    
                    _boardManager.HideHint();
                }
                else
                {
                    Debug.Log("표시할 힌트를 찾지 못했습니다. 힌트 시스템 중지.");
                    yield break;
                }
            }
        }
    }
}
