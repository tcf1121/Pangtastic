using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private DialogManager _dialogueManager;
    [SerializeField] private GameObject _dialogueUI;
    [SerializeField] private Image _background;
    [SerializeField] private RectTransform _dialogueBox;

    private Coroutine _playCo;
    private List<DialogueLine> _lines = new List<DialogueLine>();

    public event Action OnDialogueGroupEnd;

    private void Awake()
    {
        if (_dialogueManager == null)
        {
            _dialogueManager = FindObjectOfType<DialogManager>();
        }

        _dialogueUI.SetActive(false);
    }

    public void Play(DialogueGroupSO group)
    {
        if (_playCo != null)
        {
            StopCoroutine(_playCo);
            _playCo = null;
        }
        _dialogueUI.SetActive(true);
        _playCo = StartCoroutine(PlayRoutine(group));
    }

    private IEnumerator PlayRoutine(DialogueGroupSO group)
    {
        if (group == null)
        {
            yield break;
        }

        _lines = group.lines;

        for (int i = 0; i < _lines.Count; i++)
        {
            //Debug.Log($"현재 라인: {i + 1} / {_lines.Count}. 대사 ID : {_lines[i].DialogId}. 스피커 : {_lines[i].SpeakerCheck}");
            DialogueLine line = _lines[i];

            if (line.BackgroundSprite != null)
            {
                _background.sprite = line.BackgroundSprite;
                _background.enabled = true;
            }

            int speakerIndex = line.SpeakerCheck;

            string dialogText = string.Empty;

            if (line.DialogueStringSO != null)
            {
                dialogText = line.DialogueStringSO.GetText(Manager.Language.GetLanguage());
            }
            else
            {
                dialogText = "";
            }

            _dialogueManager.SetDialog(line.LeftSpeaker, line.CenterSpeaker, line.RightSpeaker, speakerIndex, line.DialogueStringSO);

            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Vector2 localPoint;

                    // RectTransformUtility.ScreenPointToLocalPointInRectangle(_dialogueBox, Input.mousePosition, null, out localPoint);

                    // if (_dialogueBox.rect.Contains(localPoint))
                    // {
                    //     break;
                    // }
                    break;
                }
                yield return null;
            }
        }
        DialogueEnd();
        yield break;
    }

    public void Skip()
    {
        DialogueEnd();
    }

    private void DialogueEnd()
    {
        StopCoroutine(_playCo);
        _playCo = null;
        OnDialogueGroupEnd?.Invoke();
        _dialogueUI.SetActive(false);
    }
}
