using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeList : MonoBehaviour
{
    [SerializeField] GameObject _skinPanel;
    [SerializeField] GameObject _facePanel;
    [SerializeField] GameObject _hairPanel;
    [SerializeField] GameObject _eyewearPanel;
    [SerializeField] GameObject _bagPanel;
    [SerializeField] GameObject _accessoryPanel;

    [SerializeField] Button _skinButton;
    [SerializeField] Button _faceButton;
    [SerializeField] Button _hairButton;
    [SerializeField] Button _eyewearButton;
    [SerializeField] Button _bagButton;
    [SerializeField] Button _accessoryButton;

    private void Awake()
    {
        _skinPanel.SetActive(true);
        _facePanel.SetActive(false);
        _hairPanel.SetActive(false);
        _eyewearPanel.SetActive(false);
        _bagPanel.SetActive(false);
        _accessoryPanel.SetActive(false);

        _skinButton.onClick.AddListener(OnSkinClicked);
        _faceButton.onClick.AddListener(OnFaceClicked);
        _hairButton.onClick.AddListener(OnHairClicked);
        _eyewearButton.onClick.AddListener(OnEWClicked);
        _bagButton.onClick.AddListener(OnBagClicked);
        _accessoryButton.onClick.AddListener(OnAccClicked);
    }

    private void OnSkinClicked()
    {
        CloseAllPanel();
        _skinPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
    }

    private void OnFaceClicked()
    {
        CloseAllPanel();
        _facePanel.SetActive(true);
        //Manager.Audio.PlaySFX();
    }

    private void OnHairClicked()
    {
        CloseAllPanel();
        _hairPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
    }
    private void OnEWClicked()
    {
        CloseAllPanel();
        _eyewearPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
    }
    private void OnBagClicked()
    {
        CloseAllPanel();
        _bagPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
    }
    private void OnAccClicked()
    {
        CloseAllPanel();
        _accessoryPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
    }

    private void CloseAllPanel()
    {
        _skinPanel.SetActive(false);
        _facePanel.SetActive(false);
        _hairPanel.SetActive(false);
        _eyewearPanel.SetActive(false);
        _bagPanel.SetActive(false);
        _accessoryPanel.SetActive(false);
    }


}
