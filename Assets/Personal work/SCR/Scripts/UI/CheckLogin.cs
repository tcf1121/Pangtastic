using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
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
            Manager.Data.SetUser();
            await Manager.DB.InitFirebase();
            SceneManager.LoadScene("Lobby Scene");
        }

        private void Logout()
        {
            DeleteFB();
            Manager.Data.DeleteSaveData();
        }

        void ShowPopup()
        {
            _infoPopup.SetActive(true);
        }

        private void DeleteFB()
        {
            FirebaseAuth auth = Manager.DB.auth;
            if (auth != null)
            {
                DatabaseReference dataToRemove = Manager.DB.GetUserPath(auth.CurrentUser.UserId);
                dataToRemove.RemoveValueAsync().ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("데이터 삭제 성공!");
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.LogError("데이터 삭제 실패: " + task.Exception);
                    }
                });
                auth.SignOut();
            }
        }

        public void PushButton()
        {
            if (Application.isMobilePlatform)
            {
                Handheld.Vibrate();
            }
            else
            {
                Manager.Audio.PlaySFX("Touch");
            }
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
            }

        }
    }
}

