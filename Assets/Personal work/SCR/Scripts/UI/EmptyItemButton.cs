using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmptyItemButton : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(EmptySound);
    }

    private void EmptySound()
    {
        Manager.Audio.PlaySFX("Item_fail");
    }
}
