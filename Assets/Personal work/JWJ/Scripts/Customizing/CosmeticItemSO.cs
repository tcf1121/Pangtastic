using System.Collections;
using System.Collections.Generic;
using UnityEngine; // 유니티 기본 네임스페이스 //추가됨!!!

// 아이템 해금 타입을 정의하는 열거형 //추가됨!!!
public enum UnlockType //추가됨!!!
{ 
    Default = 0,     // 기본 소유(처음부터 사용 가능) //추가됨!!!
    StageReward = 1, // 스테이지 보상으로 해금되는 유형 //추가됨!!!
    Shop = 2         // 상점 구매로 해금되는 유형 //추가됨!!!
} //추가됨!!!

// 메뉴에서 생성 가능한 ScriptableObject로 지정 //추가됨!!!
[CreateAssetMenu(fileName = "CosmeticItem", menuName = "PangTasticSO/CosmeticItem")] //추가됨!!!
public class CosmeticItemSO : ScriptableObject //추가됨!!!
{ //추가됨!!!
    [Header("기본 식별 정보")] // 에디터에서 그룹 헤더 표시 //추가됨!!!
    public int id = 0; // 아이템의 전역 유일 ID(정수) //추가됨!!!
    public string displayName = "New Item"; // UI에 노출할 아이템 이름 //추가됨!!!
    public CosmeticCategory category = CosmeticCategory.None; // 아이템 카테고리 //추가됨!!!

    [Header("표시 리소스")] // 표시 관련 헤더 //추가됨!!!
    public Sprite icon = null; // 인벤토리/리스트에 보여줄 아이콘 //추가됨!!!
    public GameObject prefab = null; // 3D/캐릭터 장착용 프리팹(3D일 때) //추가됨!!!
    public Sprite sprite2D = null; // 2D 스프라이트(2D 착용/미리보기용) //추가됨!!!

    [Header("색상 옵션")] // 색상 헤더 //추가됨!!!
    public bool tintable = false; // 색상 변경 가능 여부 //추가됨!!!
    public Color defaultTint = Color.white; // 기본 색상 //추가됨!!!

    [Header("해금/획득 정보")] // 해금 헤더 //추가됨!!!
    public UnlockType unlockType = UnlockType.Default; // 해금 방식 //추가됨!!!
    public string unlockParam = ""; // 보상 스테이지ID 또는 상점 상품ID 등 //추가됨!!!

#if UNITY_EDITOR // 아래 검증 로직은 에디터에서만 동작 //추가됨!!!
    private void OnValidate() // 값 변경 시 자동으로 호출되는 검증 함수 //추가됨!!!
    { //추가됨!!!
        if (id <= 0) // ID가 0 이하일 경우 //추가됨!!!
        { //추가됨!!!
            Debug.LogWarning($"[CosmeticItemSO] ID가 유효하지 않습니다: {name} (id={id})"); // 경고 로그 //추가됨!!!
        } //추가됨!!!
        if (category == CosmeticCategory.None) // 카테고리가 None일 경우 //추가됨!!!
        { //추가됨!!!
            Debug.LogWarning($"[CosmeticItemSO] 카테고리가 None 입니다: {name}"); // 경고 로그 //추가됨!!!
        } //추가됨!!!
        if (icon == null) // 아이콘이 비어있을 경우 //추가됨!!!
        { //추가됨!!!
            // 아이콘은 필수는 아니나, 리스트 UI 가독성을 위해 권장 //추가됨!!!
        } //추가됨!!!
        // prefab 또는 sprite2D는 프로젝트에 따라 둘 중 하나만 쓰일 수 있음 //추가됨!!!
    } //추가됨!!!
#endif //추가됨!!!
} //추가됨!!!
