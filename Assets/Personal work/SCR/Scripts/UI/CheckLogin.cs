using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SCR
{

    public class CheckLogin : MonoBehaviour
    {
        [SerializeField] Button _enterBtn;
        [SerializeField] Button _logoutBtn;
        [SerializeField] GameObject _enterPanel;
        [SerializeField] private GameObject _infoPopup;

        [SerializeField] private GameObject loadingUI;
        [SerializeField] private Image loadingBar;
        private DateTime targetDate = new DateTime(2026, 4, 2);

        void Awake()
        {
            Application.targetFrameRate = 60;
            _logoutBtn.onClick.AddListener(Logout);
            _enterBtn.onClick.AddListener(PushButton);
            //_enterBtn.onClick.AddListener(CheckBeforeAsync);
            _enterBtn.onClick.AddListener(() => StartCoroutine(CheckBeforeCoroutine()));
        }

        private void Start()
        {
            HistorySetting();
            DateTime now = DateTime.Now;

            if (now >= targetDate)
            {
                ShowPopup();
            }
        }



        private async void CheckBeforeAsync()
        {

            _enterPanel.SetActive(false);
            loadingUI.SetActive(true);
            loadingBar.fillAmount = 0f;
            Manager.Data.SetUser();
            Task firebaseTask = Manager.DB.InitFirebase();
            // InitFirebase가 끝날 때까지 가짜 로딩바 연출
            float fakeProgress = 0f;
            while (!firebaseTask.IsCompleted)
            {

                fakeProgress += 0.5f * 0.016f; // deltaTime 직접 계산
                if (loadingBar != null)
                    loadingBar.fillAmount = Mathf.Clamp01(fakeProgress);

                await Task.Delay(16); // 약 1프레임(60fps 기준) 대기
            }
            await firebaseTask; // 에러 처리 포함
            SceneManager.LoadScene("Lobby Scene");
        }

        private IEnumerator CheckBeforeCoroutine()
        {
            _enterPanel.SetActive(false);
            loadingUI.SetActive(true);
            loadingBar.fillAmount = 0f;

            // 1단계: Data.SetUser() → 25%
            Manager.Data.SetUser();
            yield return StartCoroutine(FillToTarget(0.25f, 0.5f));

            // 2단계: Firebase Init → 50%
            Task firebaseTask = Manager.DB.InitFirebase();

            while (!firebaseTask.IsCompleted)
            {
                loadingBar.fillAmount = Mathf.MoveTowards(
                    loadingBar.fillAmount, 0.5f, Time.deltaTime * 0.2f);
                yield return null; // 프레임 대기
            }
            // Firebase 완료 보장
            yield return new WaitUntil(() => firebaseTask.IsCompleted);
            yield return StartCoroutine(FillToTarget(0.5f, 0.5f));

            // 3단계: 다음 씬 비동기 로딩  → 100%
            AsyncOperation op = SceneManager.LoadSceneAsync("Lobby Scene");
            op.allowSceneActivation = false; // 다 찼을 때 씬 전환하도록 제어

            while (op.progress < 0.9f) // 0~0.9f까지만 반환 (95%까지)
            {
                float target = 0.5f + op.progress * 0.25f / 0.95f; // 75%~100% 매핑
                loadingBar.fillAmount = Mathf.MoveTowards(loadingBar.fillAmount, target, Time.deltaTime * 0.5f);
                yield return null;
            }

            // 0.9f → 실제 100%는 allowSceneActivation = true 해야 씬 전환됨
            yield return StartCoroutine(FillToTarget(1f, 0.2f));

            // 씬 전환
            op.allowSceneActivation = true;

            SceneManager.LoadScene("Lobby Scene");
        }

        /// <summary>
        /// 로딩바를 target까지 duration 동안 부드럽게 채움
        /// </summary>
        private IEnumerator FillToTarget(float target, float duration)
        {
            float start = loadingBar.fillAmount;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                loadingBar.fillAmount = Mathf.Lerp(start, target, t);
                yield return null;
            }

            loadingBar.fillAmount = target;
        }


        private void Logout()
        {
            Manager.Data.OffTest();
            Manager.Data.DeleteSaveData();
            Manager.DB.DeleteFB();
            _logoutBtn.gameObject.SetActive(false);
        }

        void ShowPopup()
        {
            _infoPopup.SetActive(true);
        }

        public void PushButton()
        {
            Manager.Audio.PlaySFX("Touch");
        }

        private void HistorySetting()
        {
            string DROPDOWN_KEY;
            bool historyOption;
            foreach (ToggleType type in Enum.GetValues(typeof(ToggleType)))
            {
                DROPDOWN_KEY = $"Setting_{type.ToString()}";
                if (PlayerPrefs.HasKey(DROPDOWN_KEY) == false) historyOption = true;
                else historyOption = PlayerPrefs.GetInt(DROPDOWN_KEY) == 0 ? false : true;

                if (type == ToggleType.BGM)
                    Manager.Audio.SetBGM(historyOption);
                else if (type == ToggleType.SFX)
                    Manager.Audio.SetSFX(historyOption);
                else if (type == ToggleType.Vibration)
                    Manager.Audio.SetVibrate(historyOption);
            }

        }
    }
}

