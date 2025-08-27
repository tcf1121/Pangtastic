using UnityEngine;

[CreateAssetMenu(fileName = "BlockMatal", menuName = "BlockSO/BlockMatal")]
public class SO_BlockMatal : ScriptableObject
{
    public int BlockId;
    public int MatchScore;
    public int MatchSfxId;
    public int ComboMatchSfxId;
    public int DragSfxId;
    public int DragFailSfxId;
    public int CollisionSfxId;
    public int MatchVfxId;
}
