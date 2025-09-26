using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DialoguePlayer : MonoBehaviour
{
    [SerializeField] private List<DialogueGroupSO> _dialogueGroupSO = new List<DialogueGroupSO>();

    private void Awake()
    {
        LoadAllGroup();
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

            _dialogueGroupSO.Sort((a, b) => a.dialogGroup.CompareTo(b.dialogGroup)); // 스테이지 정렬

            Debug.Log($"DialogueGroupSO 로드 완료. 총 {_dialogueGroupSO.Count}개");
        }
        else
        {
            Debug.LogError($"DialogueGroupSO 로드 실패:{handle.OperationException}");
        }
    }
}
