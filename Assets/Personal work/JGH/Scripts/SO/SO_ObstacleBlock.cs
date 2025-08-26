using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleBlock", menuName = "BlockSO/ObstacleBlock")]
public class SO_ObstacleBlock : ScriptableObject
{
    public int BlockId;
    public int Hp;
    public int HitSfxId;
    public int DestroySfxId;
    public int DestroyVfxId;
    public int Description;
}
