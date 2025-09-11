using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CurrencyStarBuy : MonoBehaviour
{
    [SerializeField] private string buttonName;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Button _button;

    private void Start()
    {
        _button.onClick.AddListener(SetupAndPlayEffect);
    }

    private void SetupAndPlayEffect()
    {
        if (string.IsNullOrEmpty(buttonName))
        {
            Debug.LogWarning("이름이 없습니다.");
            return;
        }
        if (Manager.Currency.GetStars() < int.Parse(transform.Find("Amount").GetComponent<TMP_Text>().text))
        {
            Debug.LogWarning("별이 부족합니다.");
            return;
        }
        
        // 별 차감
        var startText = GameObject.FindWithTag("StarText");
        if (startText)
        {
            Manager.Currency.SpendStar(int.Parse(transform.Find("Amount").GetComponent<TMP_Text>().text));
            var tmp = startText.GetComponent<TMP_Text>();


            switch (buttonName)
            {
                case "":
                    FindObjectOfType<FurnitureActive>()
                        .ActivateByNames("Donuts_16 Variant", buttonName);
                    break;
            }
            //if (tmp) .SetStarText(tmp);
        }

        // 이펙트 실행
        Manager.Effect.CurrencyInPlayStartEffectDown(_prefab, GameObject.FindWithTag("StarTargetUI").GetComponent<RectTransform>(), gameObject.GetComponent<RectTransform>());

        // 도넛 샵 비활성화
        StartCoroutine(CloseDonut());

    }

    private IEnumerator CloseDonut()
    {
        // DonutDecoration 찾기
        GameObject donut = GameObject.Find("DonutDecoration");
        if (donut != null)
        {
            // 1.2초 대기
            yield return new WaitForSeconds(1.3f);

            // 비활성화
            donut.SetActive(false);
            Debug.Log("DonutDecoration 1.3초 후 비활성화 완료");
        }
    }


}
