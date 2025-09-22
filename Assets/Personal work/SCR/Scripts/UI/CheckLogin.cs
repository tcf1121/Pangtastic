using System;
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
            _enterBtn.onClick.AddListener(CheckBeforeAsync);
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
                fakeProgress += Time.deltaTime * 0.5f; // 속도 조절 가능
                if (loadingBar != null)
                    loadingBar.fillAmount = Mathf.Clamp01(fakeProgress);

                await Task.Yield();
            }
            await firebaseTask; // 에러 처리 포함
            SceneManager.LoadScene("Lobby Scene");
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

