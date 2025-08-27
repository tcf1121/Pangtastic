using UnityEngine;

[CreateAssetMenu(fileName = "CustomerData", menuName = "CustomerSO/CustomerData")]
public class SO_CustomerData : ScriptableObject
{
    [Header("고객 기본 정보")] 
    public int CustomerId; // ID
    public string CustomerName; // 고객 이름 
    public int CustomerTypeId; // 고객 유형 ID

    [Header("선호 메뉴")] 
    public string LikeMenu1; // 선호메뉴 1
    public string LikeMenu2; // 선호메뉴 2
    public string LikeMenu3; // 선호메뉴 3

    [Header("이미지")]
    public Sprite CustomerImage;
}
