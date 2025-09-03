using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SCR
{
    public enum LoginType
    {
        Guest,
        Google
    }
    public class CheckLogin : MonoBehaviour
    {
        [SerializeField] Button _enterBtn;
        [SerializeField] Button _googlePlayBtn;
        [SerializeField] Button _guestBtn;
        [SerializeField] GameObject _enterPanel;
        [SerializeField] GameObject _loginPanel;

        void Awake()
        {
            _enterBtn.onClick.AddListener(CheckBefore);
            _googlePlayBtn.onClick.AddListener(GPGSManager.Instance.AuthenticateUser);
            //_guestBtn.onClick.AddListener(게스트 로그인 방법);
        }

        private void CheckBefore()
        {
            _enterPanel.SetActive(false);
            // if (전에 로그인한 적이 있으면 그 방법으로 로그인)
            // {

            // }
            // else
            //     _loginPanel.SetActive(true);
        }

        private void HowToLogin(LoginType loginType)
        {
            // 게스트로 로그인 했을 경우
            if (loginType == LoginType.Guest)
            {

            }
            // 구글로 로그인 했을 경우
            else
            {
                GPGSManager.Instance.AuthenticateUser();
            }
        }
    }
}

