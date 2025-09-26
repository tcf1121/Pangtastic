using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DialoguePlayer : MonoBehaviour
{
    [SerializeField] private DialogueController _controller;
    [SerializeField] ScenarioManager _scenario;
    [SerializeField] private List<DialogueGroupSO> _dialogueGroupSO = new List<DialogueGroupSO>();

    private void Awake()
    {
        LoadAllGroup();

        if (_controller == null)
        {
            _controller = FindObjectOfType<DialogueController>();
        }
    }

    private void LoadAllGroup()
    {
        _dialogueGroupSO.Clear();

        AsyncOperationHandle<IList<DialogueGroupSO>> handle = Addressables.LoadAssetsAsync<DialogueGroupSO>("DialogueGroupSO", null);
        handle.Completed += OnDialogueGroupLoaded;
    }

    private void OnDialogueGroupLoaded(AsyncOperationHandle<IList<DialogueGroupSO>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<DialogueGroupSO> loadedList = handle.Result;
            for (int i = 0; i < loadedList.Count; i++)
            {
                _dialogueGroupSO.Add(loadedList[i]);
            }
            Debug.Log($"DialogueGroupSO 로드 완료 총 {_dialogueGroupSO.Count}개");
            _scenario.PlayScenario();
        }
        else
        {
            Debug.LogError($"DialogueGroupSO 로드 실패:{handle.OperationException}");
        }
    }

    public void ShowById(int groupId)
    {
        for (int i = 0; i < _dialogueGroupSO.Count; i++)
        {
            if (_dialogueGroupSO[i].dialogGroup == groupId)
            {
                if (_controller != null)
                {
                    _controller.Play(_dialogueGroupSO[i]);
                    return;
                }
                else
                {
                    Debug.LogError("DialogueController 연결안됨");
                    return;
                }
            }
        }
        Debug.LogError($"잘못된 DialogueGroup ID: {groupId}");
    }


}
