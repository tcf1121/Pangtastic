using UnityEngine;

[CreateAssetMenu(fileName = "SpecialBlock", menuName = "BlockSO/SpecialBlock")]
public class SO_SpecialBlock : ScriptableObject
{
    public int BlockId;
    public int ActiveSfxId;
    public int ActiveVfxId;
    public int Description;
}
