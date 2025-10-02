using TMPro;
using UnityEngine;

public class OutLineText : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Color outLineColor;

    void Start()
    {
        // 개별 인스턴스 머티리얼 생성
        tmpText.fontMaterial = new Material(tmpText.fontMaterial);

        // 아웃라인 적용
        tmpText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.15f);
        tmpText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outLineColor);
    }
}
