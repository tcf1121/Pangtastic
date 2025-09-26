using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TEST_Dialogue : MonoBehaviour
{
    [SerializeField] private TMP_InputField _input;
    [SerializeField] private Button _button;
    [SerializeField] private DialoguePlayer _player;

    private void Awake()
    {
        if(_button == null)
        _button = GetComponent<Button>();

        if(_player == null)
            _player = FindObjectOfType<DialoguePlayer>();

        _button.onClick.AddListener(Clicked);

        _input.text = "1";
    }
    private void Clicked()
    {
        int value;

        if (!int.TryParse(_input.text, out value))
        {
            Debug.LogWarning("숫자로 변환 불가");
            return;
        }
        _player.ShowById(value);
    }
}
