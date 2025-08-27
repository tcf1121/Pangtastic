using UnityEngine;

[CreateAssetMenu(fileName = "BlockMaster", menuName = "BlockSO/BlockMaster")]
public class SO_BlockMaster : ScriptableObject
{
    [Header("기본 정보")]
    public int BlockId;          // 블록 고유 ID
    public string BlockName;     // 블록 이름
    public string BlockType;     // 블록 타입

    [Header("속성")]
    public bool IsMovable;       // 움직일 수 있는지 여부
    public bool IsCollidable;    // 충돌 가능한지 여부
    public bool IsSpawnable;     // 스폰 가능한지 여부

    [Header("리소스")]
    public string ImgPath;       // 이미지 경로
    public Sprite BlockSprite;   // 실제 스프라이트 (Unity Editor에서 할당)
}