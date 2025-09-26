using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private CharacterImageMake _ui;
    [SerializeField] private Image _background;

    private Coroutine _playCo;
    private List<DialogueLine> _lines = new List<DialogueLine>();

    public void Play(DialogueGroupSO group)
    {
        if (_playCo != null)
        {
            StopCoroutine(_playCo);
            _playCo = null;
        }
        _ui.gameObject.SetActive(true);
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

            _ui.SetCharacter(line.LeftSpeaker, line.CenterSpeaker, line.RightSpeaker, speakerIndex, line.DialogueStringSO);
            _ui.SetDialog(dialogText);

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    break;
                }
                yield return null;
            }
        }

        Debug.Log("다이얼로그 그룹 종료");
        _ui.gameObject.SetActive(false);
        yield break;
    }
}
