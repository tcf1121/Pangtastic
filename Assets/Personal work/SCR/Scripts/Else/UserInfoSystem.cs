using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

namespace JGH
{
    // public class UserInfoSystem : MonoBehaviour
    public class UserInfoSystem : Singleton<UserInfoSystem>
    {
        private UserData currentData;
        public Action<int> OnChangedHeart;
        public Action<int> OnChangedHeartTime;
        public Action<int> OnChangedStage;
        public Action<int> OnChangedStar;
        public Action<int> OnChangedCoin;
        public Action<int> OnChangedProfile;
        public Action OnUseHeart;
        public Action<float> OnInfinityHeart;
        private const string LastPurchaseDateKey = "LastPurchaseDate";
        private const string DailyCountKey = "DailyCount";
        private const string DailyGoodsListKey = "MyNumbers";
        private const int MaxDailyCount = 5;

        // log 쌓는데 게임 시작한 시간만 로그만 쌓도록
        public static bool IsStartGame = false;

        // 퀘스트
        private DailyQuestProgress quest;
        private WeeklyQuestProgress progress;
        private const float xpRatio = 0.95f;


        // 하트 시스템
        private Coroutine infinityRoutine;

        private void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            CheckDailyReset();
            RestoreHeartOnLaunch();
        }


        void CheckDailyReset()
        {
            string lastDateStr = PlayerPrefs.GetString(LastPurchaseDateKey, "");
            string today = DateTime.Now.ToString("yyyyMMdd");

            if (lastDateStr != today)
            {
                // 날짜가 다르면 카운트 초기화
                PlayerPrefs.SetInt(DailyCountKey, 0);
                PlayerPrefs.SetString(LastPurchaseDateKey, today);
                PlayerPrefs.Save();
                SetPurchaseList();
                if (!Manager.Ad.RemovedAD)
                    Manager.Ad.LoadAppOpenAd();
            }
        }

        public bool CanPurchase()
        {
            int count = PlayerPrefs.GetInt(DailyCountKey, 0);
            return count < MaxDailyCount;
        }

        public int GetPurchaseIndex()
        {
            return PlayerPrefs.GetInt(DailyCountKey, 0);
        }

        public void Purchase()
        {
            if (CanPurchase())
            {
                int count = PlayerPrefs.GetInt(DailyCountKey, 0);
                PlayerPrefs.SetInt(DailyCountKey, count + 1);
                PlayerPrefs.Save();

                Debug.Log("구매 완료! 오늘 구매 횟수: " + (count + 1));
            }
            else
            {
                Debug.Log("오늘은 더 이상 구매할 수 없습니다.");
            }
        }

        public void SetPurchaseList()
        {
            List<int> pool = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
            int[] numbers = new int[4];

            for (int i = 0; i < 4; i++)
            {
                int index = UnityEngine.Random.Range(0, pool.Count);
                numbers[i] = pool[index];
                pool.RemoveAt(index); // 중복 제거
            }

            // 배열을 문자열로 변환 (예: "3,0,6,2")
            string saveString = string.Join(",", numbers);

            // 저장
            PlayerPrefs.SetString(DailyGoodsListKey, saveString);
            PlayerPrefs.Save();

            Debug.Log("저장 완료: " + saveString);
        }

        public ItemType LoadNumbers(int index)
        {
            if (PlayerPrefs.HasKey("MyNumbers"))
            {
                string loadString = PlayerPrefs.GetString("MyNumbers");
                string[] split = loadString.Split(',');
                int[] numbers = new int[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    numbers[i] = int.Parse(split[i]);
                }
                return (ItemType)numbers[index];

            }
            return 0; // 저장된 값 없음
        }



        public void SetUser(UserData user)
        {
            currentData = user;
        }

        public int GetStage()
        {
            return currentData.Stage;
        }

        public void ClearStage()
        {
            currentData.Stage++;
        }

        public void SetStage(int value)
        {
            currentData.Stage = value;
        }

        public int GetStar()
        {
            return currentData.UserInfo.Star;
        }

        public void AddStar(int value)
        {
            currentData.UserInfo.Star += value;
            OnChangedStar?.Invoke(currentData.UserInfo.Star);
        }

        public bool CanUseStar(int value)
        {
            if (currentData.UserInfo.Star - value >= 0) return true;
            else return false;
        }

        public void UseStar(int value)
        {
            currentData.UserInfo.Star -= value;
            OnChangedStar?.Invoke(currentData.UserInfo.Star);
        }

        public int GetCoin()
        {
            return currentData.UserInfo.Coin;
        }

        public void AddCoin(int value)
        {
            currentData.UserInfo.Coin += value;
            if (currentData.UserInfo.Coin > 10000000)
                currentData.UserInfo.Coin = 10000000;
            OnChangedCoin?.Invoke(currentData.UserInfo.Coin);
        }

        public bool CanUseCoin(int value)
        {
            if (currentData.UserInfo.Coin - value >= 0) return true;
            else return false;
        }

        public void UseCoin(int value)
        {
            currentData.UserInfo.Coin -= value;
            OnChangedCoin?.Invoke(currentData.UserInfo.Coin);
        }


        public void SetHeart(int value)
        {
            currentData.UserInfo.Heart.currentHeart = value;
        }

        public int GetHeart()
        {
            return currentData.UserInfo.Heart.currentHeart;
        }

        public void InfinityHeart(float time, bool finish = false)
        {
            if (!finish)
            {
                currentData.UserInfo.Heart.currentHeart = 6;
                OnInfinityHeart?.Invoke(time);
            }
            else
            {
                currentData.UserInfo.Heart.currentHeart = 5;
                OnChangedHeart?.Invoke(currentData.UserInfo.Heart.currentHeart);
            }
        }

        public void AddHeart(int index = 1)
        {
            if (currentData.UserInfo.Heart.currentHeart == 6) return;
            if (currentData.UserInfo.Heart.currentHeart < 5)
            {
                currentData.UserInfo.Heart.currentHeart += index;
                if (GetHeart() > 5) currentData.UserInfo.Heart.currentHeart = 5;
                if (GetHeart() == 5) SetHeartTime(0);
            }
            OnChangedHeart?.Invoke(currentData.UserInfo.Heart.currentHeart);
        }

        public void UseHeart()
        {
            if (currentData.UserInfo.Heart.currentHeart == 6) return;
            if (currentData.UserInfo.Heart.currentHeart > 0)
            {
                currentData.UserInfo.Heart.currentHeart--;
                OnChangedHeart?.Invoke(currentData.UserInfo.Heart.currentHeart);
                OnUseHeart?.Invoke();
            }

        }

        public void SetLeaveTime(string value)
        {
            currentData.UserInfo.Heart.lastSaveTime = value;
        }

        public string GetLeaveTime()
        {
            return currentData.UserInfo.Heart.lastSaveTime;
        }

        public void SetHeartTime(int value)
        {
            currentData.UserInfo.Heart.remainingSeconds = value;
            OnChangedHeartTime?.Invoke(currentData.UserInfo.Heart.remainingSeconds);
        }

        public int GetHeartTime()
        {
            return currentData.UserInfo.Heart.remainingSeconds;
        }

        public bool CheckHeart()
        {
            if (currentData.UserInfo.Heart.currentHeart > 0) return true;
            else return false;
        }

        public void SetItem(ItemInfo itemInfo)
        {
            currentData.ItemInfo = itemInfo;
        }

        public bool CanUseItem(ItemType item)
        {
            if (item == ItemType.Roller) return currentData.ItemInfo.Roller > 0;
            else if (item == ItemType.DonutBox) return currentData.ItemInfo.DonutBox > 0;
            else if (item == ItemType.Oven) return currentData.ItemInfo.Oven > 0;
            else if (item == ItemType.Whisk) return currentData.ItemInfo.Whisk > 0;
            else if (item == ItemType.Scissors) return currentData.ItemInfo.Scissors > 0;
            else if (item == ItemType.DonutPan) return currentData.ItemInfo.DonutPan > 0;
            else return currentData.ItemInfo.Coffee > 0;
        }

        public void UseItem(ItemType item)
        {
            if (item == ItemType.Roller) currentData.ItemInfo.Roller--;
            else if (item == ItemType.DonutBox) currentData.ItemInfo.DonutBox--;
            else if (item == ItemType.Oven) currentData.ItemInfo.Oven--;
            else if (item == ItemType.Whisk) currentData.ItemInfo.Whisk--;
            else if (item == ItemType.Scissors) currentData.ItemInfo.Scissors--;
            else if (item == ItemType.DonutPan) currentData.ItemInfo.DonutPan--;
            else currentData.ItemInfo.Coffee--;
        }

        public void AddItem(ItemType item, int num = 1)
        {
            if (item == ItemType.Roller)
            {
                currentData.ItemInfo.Roller += num;
                if (currentData.ItemInfo.Roller >= 99)
                    currentData.ItemInfo.Roller = 99;
            }
            else if (item == ItemType.DonutBox)
            {
                currentData.ItemInfo.DonutBox += num;
                if (currentData.ItemInfo.DonutBox >= 99)
                    currentData.ItemInfo.DonutBox = 99;
            }
            else if (item == ItemType.Oven)
            {
                currentData.ItemInfo.Oven += num;
                if (currentData.ItemInfo.Oven >= 99)
                    currentData.ItemInfo.Oven = 99;
            }
            else if (item == ItemType.Whisk)
            {
                currentData.ItemInfo.Whisk += num;
                if (currentData.ItemInfo.Whisk >= 99)
                    currentData.ItemInfo.Whisk = 99;
            }
            else if (item == ItemType.Scissors)
            {
                currentData.ItemInfo.Scissors += num;
                if (currentData.ItemInfo.Scissors >= 99)
                    currentData.ItemInfo.Scissors = 99;
            }
            else if (item == ItemType.DonutPan)
            {
                currentData.ItemInfo.DonutPan += num;
                if (currentData.ItemInfo.DonutPan >= 99)
                    currentData.ItemInfo.DonutPan = 99;
            }
            else
            {
                currentData.ItemInfo.Coffee += num;
                if (currentData.ItemInfo.Coffee >= 99)
                    currentData.ItemInfo.Coffee = 99;
            }
        }

        public void SetName(string name)
        {
            currentData.PlayerName = name;
        }

        public ItemInfo GetItem()
        {
            return currentData.ItemInfo;
        }

        public UserData GetCurrentUserData()
        {
            return currentData;
        }

        public void SetCurPlace(MissionPlace curPlace)
        {
            currentData.PlaceInfo.CurPlace = (int)curPlace;
        }

        public void NewMissionList(int max)
        {
            currentData.PlaceInfo.CurMisson = new bool[max];
            for (int i = 0; i < max; i++)
            {
                currentData.PlaceInfo.CurMisson[i] = false;
            }
        }

        public MissionPlace GetCurPlace()
        {
            return (MissionPlace)currentData.PlaceInfo.CurPlace;
        }

        public void ClearCurMisson(int index)
        {
            currentData.PlaceInfo.CurMisson[index - 1] = true;
        }

        public bool MissionAllClear()
        {
            foreach (var tf in currentData.PlaceInfo.CurMisson)
                if (!tf) return false;
            return true;
        }

        public bool[] GetCurMisson()
        {
            return currentData.PlaceInfo.CurMisson;
        }

        public string GetCustomize()
        {
            return currentData.UserInfo.Customize;
        }

        public void SetCustomize(string customize)
        {
            currentData.UserInfo.Customize = customize;
        }

        public int GetScenario()
        {
            return currentData.Scenario;
        }

        public void ClearScenario()
        {
            currentData.Scenario++;
        }

        // 광고에서 시작이 기준으로 광고 표시하는거 있어서 추가 :: S
        // public string GetUtcStartDay()
        // {
        //     return currentData.StartUtcDay;
        // }
        public void SetLogAccessDates()
        {
            if (!IsStartGame)
            {
                IsStartGame = true;
                string today = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss");
                // 새로운 로그 객체 생성
                LogEntry entry = new LogEntry
                {
                    AccessDay = today,
                    OpenAd = false   // 상황에 맞게 true/false
                };

                // 리스트에 추가
                currentData.Logs.Add(entry);
            }
        }

        public List<string> GetLogAccessDates()
        {
            List<string> result = new List<string>();

            foreach (var log in currentData.Logs)
            {
                result.Add(log.AccessDay);
            }

            return result;
        }

        public void SetLastArrOpenAd(bool value)
        {
            if (currentData.Logs.Count > 0)
            {
                currentData.Logs[currentData.Logs.Count - 1].OpenAd = value;
            }
        }

        public bool GetLastArrOpenAd()
        {
            if (currentData.Logs.Count > 0)
            {
                return currentData.Logs[currentData.Logs.Count - 1].OpenAd;
            }

            return false; // 로그가 비어있을 경우 기본값
        }

        public LogEntry GetSearchOpenAdLogs()
        {
            return currentData.Logs.LastOrDefault(log => log.OpenAd);
        }
        // 광고에서 시작이 기준으로 광고 표시하는거 있어서 추가 :: E


        // 일일 퀘스트 :: S
        // 새로운 날인지 확인하고 초기화
        public void CheckAndResetIfNewDay()
        {
            string today = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd"); // 한국 시간 기준

            if (currentData.Quest.DailyQuest.lastUpdateDate != today)
            {
                currentData.Quest.DailyQuest.currentSlot = 0;
                currentData.Quest.DailyQuest.isRewardActive = false;
                currentData.Quest.DailyQuest.lastUpdateDate = today;
                Debug.Log("일일 퀘스트가 새로운 날로 초기화되었습니다.");
            }
        }

        // 슬롯 추가 (스테이지 클리어 시)
        public void DailyAddSlot()
        {
            CheckAndResetIfNewDay(); // 혹시 날짜 넘어갔으면 갱신

            if (currentData.Quest.DailyQuest.isRewardActive) return;

            currentData.Quest.DailyQuest.currentSlot++;
            if (currentData.Quest.DailyQuest.currentSlot >= 3)
            {
                currentData.Quest.DailyQuest.currentSlot = 3;
                currentData.Quest.DailyQuest.isRewardActive = true;

                // 보상 - 부스터 아이템
                ++currentData.ItemInfo.Oven;
                ++currentData.ItemInfo.DonutBox;
                ++currentData.ItemInfo.Roller;
            }
        }

        // 슬롯 감소 (스테이지 실패 시)
        public void DailyDropSlot()
        {
            CheckAndResetIfNewDay(); // 날짜 갱신

            var daily = currentData.Quest.DailyQuest;

            // 보상 활성 상태면 감소시키지 않고 무시
            if (daily.isRewardActive)
            {
                Debug.Log("보상 활성 상태에서는 슬롯이 감소하지 않습니다.");
                return;
            }

            // 슬롯 감소
            daily.currentSlot--;

            // 최소 0 보장
            if (daily.currentSlot < 0)
                daily.currentSlot = 0;

            Debug.Log($"일일 퀘스트 슬롯 감소 → 현재 슬롯: {daily.currentSlot}");
        }
        
        public int GetDailyProgressCount()
        {
            CheckAndResetIfNewDay();

            var d = currentData.Quest.DailyQuest;
            const int max = 3;

            if (d.isRewardActive) return max;          // 보상 활성 시 3 고정
            return Mathf.Clamp(d.currentSlot, 0, max); // 0~3로 안전 보정
        }
        // 일일 퀘스트 :: E


        // 주간 퀘스트 :: S
        // 주간 퀘스트 초기화 (주차 변경 체크)
        public void CheckAndResetIfNewWeek()
        {
            string currentWeekId = GetCurrentWeekId();

            if (currentData.Quest.WeeklyQuest.weekId != currentWeekId)
            {
                currentData.Quest.WeeklyQuest.level = 0;
                currentData.Quest.WeeklyQuest.currentXp = 0;
                currentData.Quest.WeeklyQuest.requiredXp = GetRequiredXp(1);
                currentData.Quest.WeeklyQuest.weekId = currentWeekId;

                Debug.Log($"[WeeklyQuest] 새로운 주차 시작 ({currentWeekId})");
            }
        }

        // 스테이지 클리어 시 경험치 지급
        public void WeeklyOnStageClear()
        {
            CheckAndResetIfNewWeek();

            var weekly = currentData.Quest.WeeklyQuest;
            float gainedXp = weekly.requiredXp * xpRatio; // 95% 지급
            weekly.currentXp += gainedXp;

            Debug.Log($"[WeeklyQuest] +{gainedXp}xp → {weekly.currentXp}/{weekly.requiredXp}");

            while (weekly.currentXp >= weekly.requiredXp)
            {
                weekly.currentXp -= weekly.requiredXp;
                WeeklyLevelUp();
            }
        }


        // 레벨업 처리
        private void WeeklyLevelUp()
        {
            var weekly = currentData.Quest.WeeklyQuest;

            weekly.level++;
            weekly.requiredXp = GetRequiredXp(weekly.level);

            // TODO: 보상 로직 추가 (예: 코인, 아이템 지급)
            switch (weekly.level)
            {
                case 1:
                    currentData.UserInfo.Coin += 100;
                    break;
                case 2:
                    StartInfinityByHours(0.25f);
                    break;
                case 3:
                    ++currentData.ItemInfo.Whisk;
                    break;
                case 4:
                    ++currentData.ItemInfo.Scissors;
                    break;
                case 5:
                    currentData.UserInfo.Coin += 300;
                    break;
                case 6:
                    StartInfinityByHours(0.333333f);
                    break;
                case 7:
                    ++currentData.ItemInfo.DonutPan;
                    break;
                case 8:
                    ++currentData.ItemInfo.Coffee;
                    break;
                case 9:
                    currentData.UserInfo.Coin += 500;
                    break;
                case 10:
                    StartInfinityByHours(0.5f);
                    break;
                case 11:
                    ++currentData.ItemInfo.Whisk;
                    ++currentData.ItemInfo.Scissors;
                    break;
                case 12:
                    ++currentData.ItemInfo.DonutPan;
                    ++currentData.ItemInfo.Coffee;
                    break;
                case 13:
                    currentData.UserInfo.Coin += 700;
                    break;
                case 14:
                    StartInfinityByHours(0.75f);
                    break;
                case 15:
                    currentData.ItemInfo.Whisk += 2;
                    currentData.ItemInfo.Scissors += 2;
                    break;
                case 16:
                    currentData.ItemInfo.DonutPan += 2;
                    currentData.ItemInfo.Coffee += 2;
                    break;
                case 17:
                    currentData.UserInfo.Coin += 1000;
                    break;
                case 18:
                    StartInfinityByHours(1f);
                    break;
                case 19:
                    SetAllItem(1);
                    // ++currentData.ItemInfo.Coffee;
                    // ++currentData.ItemInfo.DonutBox;
                    // ++currentData.ItemInfo.DonutPan;
                    // ++currentData.ItemInfo.Oven;
                    // ++currentData.ItemInfo.Roller;
                    // ++currentData.ItemInfo.Scissors;
                    // ++currentData.ItemInfo.Whisk;
                    break;
                case 20:
                    SetAllItem(2);
                    // currentData.ItemInfo.Coffee += 2;
                    // currentData.ItemInfo.DonutBox += 2;
                    // currentData.ItemInfo.DonutPan += 2;
                    // currentData.ItemInfo.Oven += 2;
                    // currentData.ItemInfo.Roller += 2;
                    // currentData.ItemInfo.Scissors += 2;
                    // currentData.ItemInfo.Whisk += 2;
                    break;
            }

            Debug.Log($"[WeeklyQuest] 레벨업! → Lv.{weekly.level}, 다음 요구치 {weekly.requiredXp}");
        }
        
        

        // 주간 경험치 요구치 계산
        private float GetRequiredXp(int level)
        {
            // 필요시 난이도 조절 가능
            return 100 + (level - 1) * 50;
        }

        // 현재 주차 ID 반환 (예: "2025-W39")
        private string GetCurrentWeekId()
        {
            // 한국 시간 (KST)
            DateTime now = DateTime.UtcNow.AddHours(9);

            // 이번 주 목요일 04:00 계산
            // 기준: 이번 주 월요일 00:00 → 거기서 목요일 04:00 더하기
            int diffToMonday = ((int)now.DayOfWeek + 6) % 7; // 월요일=0, 화=1 ...
            DateTime monday = now.Date.AddDays(-diffToMonday);
            DateTime resetTime = monday.AddDays(3).AddHours(4); // 목요일 04:00

            // 만약 현재 시간이 resetTime 이전이라면 → 지난 주차로 귀속
            if (now < resetTime)
                resetTime = resetTime.AddDays(-7);

            // resetTime 기준으로 주차 ID 생성
            CultureInfo cul = CultureInfo.CurrentCulture;
            int weekNum = cul.Calendar.GetWeekOfYear(
                resetTime,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            return $"{resetTime.Year}-W{weekNum}";
        }
        
        public int GetWeeklyLevel()
        {
            CheckAndResetIfNewWeek();

            var w = currentData.Quest.WeeklyQuest;
            return Mathf.Clamp(w.level, 0, 20);
        }
        // 주간 퀘스트 :: E


        // 하드 시스템 :: S
        public void StartInfinityByHours(float minutes)
        {
            InfinityHeart(minutes, false);
        }
        
        private int GetInfinityRemainingSeconds()
        {
            string s = currentData.UserInfo.Heart.infinityEndTime;
            if (string.IsNullOrEmpty(s)) return 0;
            if (!DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var utcEnd)) return 0;

            double sec = (utcEnd - DateTime.UtcNow).TotalSeconds;
            if (sec < 0) sec = 0;
            return (int)Math.Floor(sec);
        }
        
        private float GetNormalRemainingMinutes()
        {
            int remainSec = currentData.UserInfo.Heart.remainingSeconds;
            if (remainSec <= 0) return 0f;

            return (float)Math.Ceiling(remainSec / 60f);
        }
        
        private void RestoreHeartOnLaunch()
        {
            string s = currentData.UserInfo.Heart.infinityEndTime;

            if (!string.IsNullOrEmpty(s) &&
                DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var utcEnd))
            {
                if (DateTime.UtcNow < utcEnd)
                {
                    double remainMinExact = (utcEnd - DateTime.UtcNow).TotalMinutes;
                    int remainMinCeil = (int)Math.Ceiling(remainMinExact);
                    if (remainMinCeil < 1) remainMinCeil = 1;

                    InfinityHeart(remainMinCeil, false);

                    if (infinityRoutine != null) StopCoroutine(infinityRoutine);
                    infinityRoutine = StartCoroutine(InfinityTicker());
                    return;
                }
            }

            float normalMin = GetNormalRemainingMinutes();
            InfinityHeart(normalMin, true);
        }
        
        private IEnumerator InfinityTicker()
        {
            while (true)
            {
                int remain = GetInfinityRemainingSeconds();

                OnInfinityHeart?.Invoke(remain);

                if (remain <= 0)
                {
                    currentData.UserInfo.Heart.infinityEndTime = "";
                    float normalMin = GetNormalRemainingMinutes();
                    InfinityHeart(normalMin, true);

                    infinityRoutine = null;
                    yield break;
                }
                yield return null;
            }
        }
        // 하드 시스템 :: E
        
        
        // 블럭 업적 시스템 :: S
        static readonly int[] BLOCK_TIERS        = { 100, 200, 300, 400, 500 };
        static readonly int[] BLOCK_TIER_COINS   = { 600, 700, 800, 900, 1000 };
        static readonly int[] BLOCK_TIER_ALLITEM = { 2, 3, 4, 5, 6 };
        static readonly string[] BlockID = {
            "CgkI8LO4iJMSEAIQAw", "CgkI8LO4iJMSEAIQBA", "CgkI8LO4iJMSEAIQBQ", "CgkI8LO4iJMSEAIQBg", "CgkI8LO4iJMSEAIQBw"
        };
        public void blockCount(int num)
        {
            if (num <= 0) return;

            var achv = currentData.Quest.BlockAchv;

            achv.blockCount += num;

            EvaluateBlockMilestones();  // 단계 평가 & 보상
        }
        
        void EvaluateBlockMilestones()
        {
            var achv = currentData.Quest.BlockAchv;

            int level = achv.blockLevelClaimed; 
            while (level < BLOCK_TIERS.Length && achv.blockCount >= BLOCK_TIERS[level])
            {
                AwardBlockTier(level);
                level++;
            }

            achv.blockLevelClaimed = level; // 가장 높은 ‘보상 완료’ 단계 갱신
        }
        
        void AwardBlockTier(int levelIdx)
        {
            currentData.UserInfo.Coin += BLOCK_TIER_COINS[levelIdx];
            SetAllItem(BLOCK_TIER_ALLITEM[levelIdx]);

            if (levelIdx >= 0 && levelIdx < BlockID.Length)
            {
                Social.ReportProgress(BlockID[levelIdx], 100.0, _ => { });
            }
        }
        // 블럭 업적 시스템 :: E
        
        
        // 가구 업적 시스템 :: S
        static readonly int[] FURN_PLACE_TIERS        = { 20, 40, 60, 80, 100 };
        static readonly int[] FURN_PLACE_TIER_COINS   = { 200, 350, 500, 650, 800 };
        static readonly int[] FURN_PLACE_TIER_ALLITEM = { 1,   2,   3,   4,   5   };

        static readonly string[] FURN_PLACE_GPGS = {
            "CgkI8LO4iJMSEAIQCA", "CgkI8LO4iJMSEAIQCQ", "CgkI8LO4iJMSEAIQCg", "CgkI8LO4iJMSEAIQCw", "CgkI8LO4iJMSEAIQDA"
        };

        public void furniturePlaced(int num)
        {
            if (num <= 0) return;

            var achv = currentData.Quest.FurnitureAchv;
            achv.furniturePlacedCount += num;

            EvaluateFurniturePlaceMilestones(); 
        }
        
        void EvaluateFurniturePlaceMilestones()
        {
            var achv = currentData.Quest.FurnitureAchv;

            int level = achv.furniturePlaceLevelClaimed; 
            while (level < FURN_PLACE_TIERS.Length && 
                   achv.furniturePlacedCount >= FURN_PLACE_TIERS[level])
            {
                AwardFurniturePlaceTier(level);
                level++;
            }

            achv.furniturePlaceLevelClaimed = level; 
        }
        
        void AwardFurniturePlaceTier(int levelIdx)
        {
            currentData.UserInfo.Coin += FURN_PLACE_TIER_COINS[levelIdx];
            SetAllItem(FURN_PLACE_TIER_ALLITEM[levelIdx]);

            if (levelIdx >= 0 && levelIdx < FURN_PLACE_GPGS.Length)
            {
                Social.ReportProgress(FURN_PLACE_GPGS[levelIdx], 100.0, _ => { });
            }
        }
        // 가구 업적 시스템 :: E
        
        // 스태이지 업적 시스템 :: S
        static readonly int[] CHAPTER_COINS   = { 600, 800, 1000, 1200, 1400, 1600, 1800, 2000 };
        static readonly int[] CHAPTER_ALLITEM = { 3,   4,    5,    6,    7,    8,    9,    10  };

        // GPGS 단일형 업적 ID (Play Console 실제 ID로 교체)
        static readonly string[] CHAPTER_GPGS = {
            "CgkI8LO4iJMSEAIQDQ",    // 챕터1
            "CgkI8LO4iJMSEAIQDg",    // 챕터2
            "CgkI8LO4iJMSEAIQDw",    // 챕터3
            "CgkI8LO4iJMSEAIQEA",    // 챕터4
            "CgkI8LO4iJMSEAIQEQ",    // 챕터5
            "CgkI8LO4iJMSEAIQEg",    // 챕터6
            "CgkI8LO4iJMSEAIQEw",    // 챕터7
            "CgkI8LO4iJMSEAIQFA"     // 챕터8
        }; 
        /// <summary>
        /// 챕터의 마지막 스테이지를 클리어했을 때 호출해줘.
        /// </summary>
        /// <param name="chapter">1~8</param>
        public void OnChapterLastStageCleared(int chapter)
        {
            int idx = chapter;
            if ((uint)idx >= 8) return; // 범위 보호

            var flags = currentData.Quest.StageAchv.chapterClearedAwarded;
            if (flags[chapter]) return; // 이미 지급됨 → 중복 방지

            // 보상 지급
            currentData.UserInfo.Coin += CHAPTER_COINS[idx];
            SetAllItem(CHAPTER_ALLITEM[idx]);

            // GPGS 업적 해제
            var achId = CHAPTER_GPGS[idx];
            Social.ReportProgress(achId, 100.0, _ => { });

            // 플래그 저장
            flags[idx] = true;
            // 저장 루틴이 있다면 여기서 Save 호출(프로젝트 규칙에 맞춰)
        }
        // 스태이지 업적 시스템 :: E

        public void SetAllItem(int num)
        {
            currentData.ItemInfo.Coffee += num;
            currentData.ItemInfo.DonutBox += num;
            currentData.ItemInfo.DonutPan += num;
            currentData.ItemInfo.Oven += num;
            currentData.ItemInfo.Roller += num;
            currentData.ItemInfo.Scissors += num;
            currentData.ItemInfo.Whisk += num;
        }
    }

    public enum ItemType
    {
        Roller,
        DonutBox,
        Oven,
        Whisk,
        Scissors,
        DonutPan,
        Coffee
    }

    [Serializable]
    public class UserData
    {
        public UserInfo UserInfo { get; set; } = new UserInfo();
        public int Stage { get; set; } = 0;
        public int Scenario { get; set; } = 0;
        public string PlayerName { get; set; } = "";
        public ItemInfo ItemInfo { get; set; } = new ItemInfo();
        public PlaceInfo PlaceInfo { get; set; } = new PlaceInfo();

        // 광고에서 시작이 기준으로 광고 표시하는거 있어서 추가 :: S
        // public string StartUtcDay { get; set; } = "";
        // public Logs Logs { get; set; } = new Logs();
        // public Logs Logs = new Logs();
        public List<LogEntry> Logs = new List<LogEntry>();
        // 광고에서 시작이 기준으로 광고 표시하는거 있어서 추가 :: E

        // 퀘스트 항목 추가 :: S
        public QuestData Quest { get; set; } = new QuestData();
        // 퀘스트 항목 추가 :: E
    }
    
    // 퀘스트 항목 추가 :: [System.Serializable]
    [Serializable]
    public class QuestData
    {
        public DailyQuestProgress DailyQuest { get; set; } = new DailyQuestProgress();
        // public AchievementProgress Achievement { get; set; } = new AchievementProgress();
        public BlockAchievementData    BlockAchv    = new BlockAchievementData();
        public FurnitureAchievementData FurnitureAchv = new FurnitureAchievementData();
        public StageAchievementData      StageAchv     = new StageAchievementData();
        public WeeklyQuestProgress WeeklyQuest { get; set; } = new WeeklyQuestProgress();
    }

    [Serializable]
    public class DailyQuestProgress
    {
        public int currentSlot = 0;         // 현재 채워진 칸 (0~3)
        public bool isRewardActive = false; // 보상 활성 상태
        public string lastUpdateDate = "";  // 마지막 갱신일 (yyyy-MM-dd)
    }
    
    [Serializable]
    public class BlockAchievementData {
        public int blockCount;              // 누적 블록
        public int blockLevelClaimed;       // 지급 완료 최고 인덱스(0~N-1)
    }
    
    [Serializable]
    public class StageAchievementData
    {
        public bool[] chapterClearedAwarded = new bool[8];
    }
    
    [Serializable]
    public class FurnitureAchievementData {
        public int furniturePlacedCount;        // 유니크 배치 누적
        public int furniturePlaceLevelClaimed;  // 지급 완료 최고 인덱스(0~N-1)

        // 중복 배치 방지용(직렬화 리스트 + 런타임 캐시)
        // public List<long> placedInstanceIds = new List<long>();
        // [NonSerialized] public HashSet<long> placedInstanceCache;
        // public void EnsureCache() {
        //     if (placedInstanceCache == null)
        //         placedInstanceCache = new HashSet<long>(placedInstanceIds);
        // }
    }

    // [Serializable]
    // public class AchievementProgress
    // {
    //     public int blockCount;           // 누적 매치 수(제한 없음, 500 넘어도 계속 누적)
    //     public int furniturePlacedCount;        // 가구 배치 누적 수
    //     public int furniturePlaceLevelClaimed;  // 마지막으로 ‘보상 완료’한 레벨 인덱스(0~4)
    //     public int stageCount;
    //     public int blockLevelClaimed;    // 이미 보상한 최고 단계(0~5). 0=미지급
    // }
    
    [System.Serializable]
    public class WeeklyQuestProgress
    {
        public int level = 0;          
        public float currentXp = 0;    
        public float requiredXp = 100; 
        public string weekId = "";     // 주간 구분 (예: "2025-W39")
    }
    // 퀘스트 항목 추가 :: E

    // 광고 :: S
    [Serializable]
    public class LogEntry
    {
        public string AccessDay;
        public bool OpenAd;
    }
    // 광고 :: E

    // [Serializable]
    // public class Logs
    // {
    //     // public List<string> AccessDays { get; set; } = new List<string>();
    //     public List<string> AccessDays = new List<string>();
    // }



    [Serializable]
    public class UserInfo
    {
        public HeartInfo Heart { get; set; } = new HeartInfo();
        public int Coin { get; set; } = 0;
        public int Star { get; set; } = 0;
        public string Customize { get; set; } = "0, 0, -1, -1, -1, -1";
    }

    [Serializable]
    public class HeartInfo
    {
        public int currentHeart { get; set; } = 0;
        public string lastSaveTime { get; set; } = "";
        public int remainingSeconds { get; set; } = 0;
        
        // 무한 하트 종료 시각
        public string infinityEndTime { get; set; } = "";
    }

    [Serializable]
    public class ItemInfo
    {
        public int Roller { get; set; } = 0;
        public int DonutBox { get; set; } = 0;
        public int Oven { get; set; } = 0;
        public int Whisk { get; set; } = 0;
        public int Scissors { get; set; } = 0;
        public int DonutPan { get; set; } = 0;
        public int Coffee { get; set; } = 0;
    }

    [Serializable]
    public class PlaceInfo
    {
        public int CurPlace { get; set; } = 0;
        public bool[] CurMisson { get; set; } = new bool[16];
    }
}