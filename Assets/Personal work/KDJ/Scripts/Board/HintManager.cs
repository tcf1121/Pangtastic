using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KDJ.States;

namespace KDJ
{
    public class HintManager : MonoBehaviour
    {
        [SerializeField] private float _hintDelay = 3f;
        private float _idleTimer = 0f;
        private bool _isHintShowing = false;

        private BoardManager _boardManager;

        private void Awake()
        {
            _boardManager = GetComponent<BoardManager>();
        }

        private void Update()
        {
            // 힌트 타이머는 ReadyState이고, 콤보가 0일 때만 흘러가야 합니다.
            if (_boardManager.CurrentState is ReadyState && _boardManager.MatchCombo.CurCombo == 0)
            {
                _idleTimer += Time.deltaTime;

                if (_idleTimer >= _hintDelay && !_isHintShowing)
                {
                    ShowHintSequence();
                }
            }
            else
            {
                // ReadyState가 아니거나 콤보가 진행 중이면 타이머를 리셋합니다.
                if (_idleTimer > 0 || _isHintShowing)
                {
                    ResetHintTimer();
                }
            }
        }

        /// <summary>
        /// 외부에서 힌트 타이머와 상태를 리셋하기 위해 호출합니다.
        /// </summary>
        public void ResetHintTimer()
        {
            _idleTimer = 0f;
            if (_isHintShowing)
            {
                _boardManager.HideHint();
                _boardManager.CurHintPositions.Clear();
                _isHintShowing = false;
            }
        }

        private void ShowHintSequence()
        {
            _isHintShowing = true; // 힌트를 찾는 동안 중복 실행 방지
            List<Vector2Int> hintPositions = _boardManager.MatchChecker.OptimalMatchFind();
            if (hintPositions != null && hintPositions.Count > 0)
            {
                Debug.Log("힌트 표시");
                _boardManager.CurHintPositions = hintPositions;
                _boardManager.ShowHint(hintPositions);
            }
            else
            {
                Debug.Log("표시할 힌트를 찾지 못했습니다.");
            }
        }
    }
}
