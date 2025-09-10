using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestIncreasePatience : MonoBehaviour
{
    [SerializeField] private OrderStateController order;
    [SerializeField] private Button button;
    [SerializeField] private TMP_InputField percent;

    private void Awake()
    {
        order = FindObjectOfType<OrderStateController>();
        button.onClick.AddListener(IncreasePatience);
    }

    private void IncreasePatience()
    {
        float value = float.Parse(percent.text);
        order.AddPatience(value);
        Debug.Log($"인내심 {value}% 증가");
    }




}
