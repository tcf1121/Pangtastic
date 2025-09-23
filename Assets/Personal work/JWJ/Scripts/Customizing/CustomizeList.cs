using System.Collections;
using System.Collections.Generic;
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
    }

    private void OnSkinClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _skinPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
        _skinButton.interactable = false;
    }

    private void OnFaceClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _facePanel.SetActive(true);
        //Manager.Audio.PlaySFX();
        _faceButton.interactable = false;
    }

    private void OnHairClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _hairPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
        _hairButton.interactable = false;
    }
    private void OnEWClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _facewearPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
        _facewearButton.interactable = false;
    }
    private void OnBagClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _bagPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
        _bagButton.interactable = false;
    }
    private void OnAccClicked()
    {
        CloseAllPanel();
        EnableAllButtons();
        _accessoryPanel.SetActive(true);
        //Manager.Audio.PlaySFX();
        _accessoryButton.interactable = false;
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
