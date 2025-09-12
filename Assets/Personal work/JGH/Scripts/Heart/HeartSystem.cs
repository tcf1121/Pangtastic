// using Firebase.Auth;
// using Firebase.Database;
// using Firebase.Extensions;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class HeartSystem : MonoBehaviour
// {
//     public static HeartSystem Instance { get; private set; }

//     [SerializeField] private TMP_Text _timerText; // UI Text (MM:SS 표시)
//     [SerializeField] private TMP_Text _textHeart; // 
//     [SerializeField] private int _startSeconds = 1800; // 시작 시간 (기본 30분, 초 단위)
//     [SerializeField] private int _maxHearts = 5; // 최대 하트 개수
//     [SerializeField] private int _currentHearts = 0; // 현재 하트 개수

//     private int _remainingSeconds;
//     private int _lastTime;

//     private DateTime? _pauseStartTime; 


//     private void Start()
//     {
//         // 혹시 중복 실행된 코루틴이 있으면 정리
//         StopAllCoroutines();

//         InitHeartData();

//         HeartLoadData();
//         UpdateHeartUI();

//         // 초기 시간 설정
//         UpdateTimerUI();

//         // 타이머 시작
//         StartCoroutine(TimerCoroutine());
//     }
//     private void InitHeartData()
//     {
//         // 저장 불러오기 대신 기본값으로 시작
//         _currentHearts = _maxHearts;
//         _remainingSeconds = _startSeconds;

//         Debug.Log("저장 기능 제거됨: 기본값으로 시작");
//     }


//     private void HeartLoadData()
//     {
//         // 계정 & 경로
//         string authJson = Manager.DB.GetAuthInfo();
//         DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);
//         var heartRef = Manager.DB.dbRef.Child(info.type).Child(info.uid).Child("heart");

//         // 읽기
//         heartRef.GetValueAsync().ContinueWithOnMainThread(task =>
//         {
//             var s = task.Result;  

//             // 최초 생성
//             if (!s.Exists)
//             {
//                 _currentHearts = _maxHearts;
//                 _remainingSeconds = 0;

//                 var initTask = heartRef.UpdateChildrenAsync(new Dictionary<string, object>
//                 {
//                     ["currentHeart"]     = _currentHearts,
//                     ["remainingSeconds"] = 0,
//                     ["lastSaveTime"]     = DateTime.UtcNow.ToString() // 네 규칙 유지
//                 });
//                 if (initTask.Exception != null) Debug.LogError("heart 초기 생성 실패: " + initTask.Exception);
//             }
//             else
//             {
//                 _currentHearts = int.Parse(_currentHearts.ToString());
//                 _remainingSeconds = (_currentHearts >= _maxHearts) ? 0 : _startSeconds;

//                 DateTime lastTime = Convert.ToDateTime(s.Child("lastSaveTime").Value?.ToString());
//                 TimeSpan diff = DateTime.Now - lastTime;

//                 // 지난 시간만큼 하트 충전
//                 int recoveredHearts = (int)(diff.TotalSeconds / _startSeconds);
//                 _currentHearts = Mathf.Min(_currentHearts + recoveredHearts, _maxHearts);

//                 if (_currentHearts < _maxHearts)
//                 {
//                     // 남은 시간 계산 (기존 저장된 남은 시간에서 경과 시간 빼기)
//                     _remainingSeconds = _remainingSeconds - (int)diff.TotalSeconds;

//                     if (_remainingSeconds <= 0)
//                     {
//                         // 부족하면 추가로 하트 충전
//                         int extraHearts = Mathf.Abs(_remainingSeconds) / _startSeconds + 1;
//                         _currentHearts = Mathf.Min(_currentHearts + extraHearts, _maxHearts);

//                         // 남은 시간 재설정
//                         if (_currentHearts < _maxHearts)
//                             _remainingSeconds = _startSeconds - (Mathf.Abs(_remainingSeconds) % _startSeconds);
//                         else
//                             _remainingSeconds = 0;
//                     }
//                 }
//             }

//             UpdateHeartUI();
//             UpdateTimerUI();
//             StartCoroutine(TimerCoroutine());
//             HeartSaveData();
//         });
//     }

//     private void HeartSaveData()
//     {
//         string authJson = Manager.DB.GetAuthInfo();
//         DatabaseSystem.AuthInfo info = JsonUtility.FromJson<DatabaseSystem.AuthInfo>(authJson);
//         var heartRef = Manager.DB.dbRef.Child(info.type).Child(info.uid).Child("heart");

//         heartRef.Child("currentHeart").SetValueAsync(_currentHearts);
//         heartRef.Child("remainingSeconds").SetValueAsync(_remainingSeconds);
//         heartRef.Child("lastSaveTime").SetValueAsync(_lastTime);
//     }

//     public int GetMaxHearts()
//     {
//         return _maxHearts;
//     }


//     /// <summary>
//     /// 하트의 갯수를 파악하여 하트가 있을 시 스테이지를 시작합니다.
//     /// </summary>
//     /// <param name="requiredHearts"></param>
//     /// <returns></returns>
//     public bool TryStartStage()
//     {
//         if (_currentHearts != 0)
//         {
//             Debug.Log($"스테이지 시작!");
//             return true;
//         }
//         else
//         {
//             Debug.Log("하트 부족! 스테이지 시작 불가");
//             return false;
//         }
//     }

//     /// <summary>
//     /// 스테이지 실패시 하트를 소모합니다.
//     /// </summary>
//     public void UseHearts()
//     {
//         if (_currentHearts > 0)
//         {
//             _currentHearts--;
//         }
//         UpdateHeartUI();
//         HeartSaveData();
//     }

//     /// <summary>
//     /// 하트를 회복(채우기)합니다.
//     /// 최대치 제한 없음
//     /// </summary>
//     /// <param name="amount">회복할 하트 개수</param>
//     public void AddHearts(int amount)
//     {
//         if (amount <= 0)
//         {
//             Debug.LogWarning("회복할 하트 개수가 0 이하입니다.");
//             return;
//         }

//         int beforeHearts = _currentHearts;
//         _currentHearts += amount; // 제한 없이 누적

//         UpdateHeartUI();
//         HeartSaveData();

//         Debug.Log($"하트 {amount}개 회복! ({beforeHearts} → {_currentHearts})");
//     }

//     /// <summary>
//     /// 하트 UI를 업데이트합니다.
//     /// </summary>
//     private void UpdateHeartUI()
//     {
//         if (_textHeart != null)
//             _textHeart.text = $"{_currentHearts}";
//             // _textHeart.text = $"{_currentHearts}/{_maxHearts}";
//     }

//     /// <summary>
//     /// 하트 타이머
//     /// 시간이 다 되면 하트를 충전합니다.
//     /// </summary>
//     /// <returns></returns>
//     private IEnumerator TimerCoroutine()
//     {
//         while (true) // 무한 루프 → 내부에서 조건으로 제어
//         {
//             // 하트가 최대치라면 충전하지 않고 리턴
//             if (_currentHearts >= _maxHearts)
//             {
//                 // if (_timerText != null)
//                 // _timerText.gameObject.SetActive(false);

//                 yield return new WaitForSeconds(1f);
//                 continue;
//             }

//             if (_remainingSeconds > 0)
//             {
//                 yield return new WaitForSeconds(1f);

//                 // 1초 감소
//                 _remainingSeconds--;

//                 UpdateTimerUI();
//             }
//             else
//             {
//                 // 시간이 다 되었을 때
//                 if (_currentHearts < _maxHearts)
//                 {
//                     _currentHearts++;
//                     UpdateHeartUI();
//                     Debug.Log("하트 충전! 현재 하트: " + _currentHearts);

//                     // 다시 카운트다운 초기화
//                     _remainingSeconds = _startSeconds;
//                     UpdateTimerUI();

//                     HeartSaveData();
//                 }
//             }
//         }
//     }


//     /// <summary>
//     /// 타이머 UI를 업데이트합니다.
//     /// 하트가 가득찬경우 FULL로 표시합니다.
//     /// </summary>
//     private void UpdateTimerUI()
//     {
//         if (_timerText == null) return;

//         if (_currentHearts >= _maxHearts)
//         {
//             _timerText.text = "FULL";
//             return;
//         }

//         int minutes = _remainingSeconds / 60;
//         int seconds = _remainingSeconds % 60;
//         _timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
//     }

//     /// <summary>
//     /// 애플리케이션이 종료될 때 하트 데이터를 저장합니다.
//     /// </summary>
//     private void OnApplicationQuit()
//     {
//         HeartSaveData(); // 정상 종료 시 저장
//     }

//     /// <summary>
//     /// 애플리케이션이 백그라운드로 갔을 때 하트 데이터를 저장합니다.
//     /// </summary>
//     /// <param name="pause"></param>
//     // private void OnApplicationPause(bool pause)
//     // {
//     // if (pause)
//     // {
//     // Quit();
//     // }
//     // }

//     private void Quit(bool pause)
// // >>>>>>> Develop
//     {
//         if (pause)
//         {
//             // 앱이 백그라운드로 간 시각 기록
//             _pauseStartTime = DateTime.Now;
//         }
//         else
//         {
//             // 다시 돌아왔을 때
//             if (_pauseStartTime.HasValue)
//             {
//                 TimeSpan diff = DateTime.Now - _pauseStartTime.Value;

//                 if (diff.TotalSeconds >= 10)
//                 {
//                     // 10초 이상 지났으면 실행
//                     Debug.Log("앱이 5초 이상 백그라운드에 있었습니다. 동작 실행!");
//                     HeartSaveData(); // 원하는 동작 호출
//                 }
//                 else
//                 {
//                     Debug.Log("짧은 일시정지이므로 동작하지 않음");
//                 }

//                 _pauseStartTime = null; // 초기화
//             }
//         }
//     }



//     /// <summary>
//     /// 
//     /// 하트를 사용하여 스테이지를 시작합니다.
//     /// </summary>
//     /// <param name="requiredHearts"></param>
//     /// <returns></returns>
//     public bool TryUseHearts(int requiredHearts)
//     {
//         if (_currentHearts >= requiredHearts)
//         {
//             _currentHearts -= requiredHearts;
//             UpdateHeartUI();
//             HeartSaveData();
//             Debug.Log($"스테이지 시작! 하트 {requiredHearts}개 사용, 남은 하트: {_currentHearts}");
//             return true;
//         }
//         else
//         {
//             Debug.Log("하트 부족! 스테이지 시작 불가");
//             return false;
//         }
//     }

//     /// <summary>
//     /// 씬이 로드될 때마다 UI를 다시 찾아 연결합니다.
//     /// </summary>
//     private void OnEnable()
//     {
//         SceneManager.sceneLoaded += OnSceneLoaded;
//     }

//     /// <summary>
//     /// 씬이 비활성화될 때 이벤트 구독을 해제합니다.
//     /// </summary>
//     private void OnDisable()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         // 새 씬이 로드될 때마다 UI 다시 찾아 연결
//         _timerText = GameObject.Find("TextTimer")?.GetComponent<TMP_Text>();
//         _textHeart = GameObject.Find("TextHeart")?.GetComponent<TMP_Text>();

//         // UI 즉시 갱신
//         UpdateHeartUI();
//         UpdateTimerUI();
//     }
// }
