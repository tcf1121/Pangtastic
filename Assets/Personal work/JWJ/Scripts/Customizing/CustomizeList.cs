using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeList : MonoBehaviour
{
    [SerializeField] GameObject _skinPanel;
    [SerializeField] GameObject _facePanel;
    [SerializeField] GameObject _hairPanel;
    [SerializeField] GameObject _facewearPanel;
    [SerializeField] GameObject _bagPanel;
    [SerializeField] GameObject _accessoryPanel;

    [SerializeField] Button _skinButton;
    [SerializeField] Button _faceButton;
    [SerializeField] Button _hairButton;
    [SerializeField] Button _facewearButton;
    [SerializeField] Button _bagButton;
    [SerializeField] Button _accessoryButton;

    [SerializeField] ScrollRect _scrollRect;

    private RectTransform _skinRT;
    private RectTransform _faceRT;
    private RectTransform _hairRT;
    private RectTransform _faceWearRT;
    private RectTransform _bagRT;
    private RectTransform _accessoryRT;


    private void Awake()
    {
        _skinPanel.SetActive(true);
        _facePanel.SetActive(false);
        _hairPanel.SetActive(false);
        _facewearPanel.SetActive(false);
        _bagPanel.SetActive(false);
        _accessoryPanel.SetActive(false);

        _skinButton.onClick.AddListener(OnSkinClicked);
        _faceButton.onClick.AddListener(OnFaceClicked);
        _hairButton.onClick.AddListener(OnHairClicked);
        _facewearButton.onClick.AddListener(OnEWClicked);
        _bagButton.onClick.AddListener(OnBagClicked);
        _accessoryButton.onClick.AddListener(OnAccClicked); 
        
        _skinButton.interactable = false;

        if(_scrollRect == null)
        {
            _scrollRect = GetComponentInChildren<ScrollRect>();
        }

        _skinRT = _skinPanel.GetComponent<RectTransform>();
        _faceRT = _facePanel.GetComponent<RectTransform>();
        _hairRT = _hairPanel.GetComponent<RectTransform>();
        _faceWearRT = _facewearPanel.GetComponent<RectTransform>();
        _bagRT = _bagPanel.GetComponent<RectTransform>();
        _accessoryRT = _accessoryPanel.GetComponent<RectTransform>();

    }

    private void OnSkinClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _skinPanel.SetActive(true);
        Manager.Audio.PlaySFX("Touch");
        _skinButton.interactable = false;
        _scrollRect.content = _skinRT;
    }

    private void OnFaceClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _facePanel.SetActive(true);
        Manager.Audio.PlaySFX("Touch");
        _faceButton.interactable = false;
        _scrollRect.content = _faceRT;
    }

    private void OnHairClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _hairPanel.SetActive(true);
        Manager.Audio.PlaySFX("Touch");
        _hairButton.interactable = false;
        _scrollRect.content = _hairRT;
    }
    private void OnEWClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _facewearPanel.SetActive(true);
        Manager.Audio.PlaySFX("Touch");
        _facewearButton.interactable = false;
        _scrollRect.content = _faceWearRT;
    }
    private void OnBagClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _bagPanel.SetActive(true);
        Manager.Audio.PlaySFX("Touch");
        _bagButton.interactable = false;
        _scrollRect.content = _bagRT;
    }
    private void OnAccClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _accessoryPanel.SetActive(true);
        Manager.Audio.PlaySFX("Touch");
        _accessoryButton.interactable = false;
        _scrollRect.content = _accessoryRT;
    }

    private void CloseAllPanel()
    {
        _skinPanel.SetActive(false);
        _facePanel.SetActive(false);
        _hairPanel.SetActive(false);
        _facewearPanel.SetActive(false);
        _bagPanel.SetActive(false);
        _accessoryPanel.SetActive(false);
    }

    private void EnableAllButtons()
    {
        _skinButton.interactable = true;
        _faceButton.interactable = true;
        _hairButton.interactable = true;
        _facewearButton.interactable = true;
        _bagButton.interactable = true;
        _accessoryButton.interactable = true;
    }


}
