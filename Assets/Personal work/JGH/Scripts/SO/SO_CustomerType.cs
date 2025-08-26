using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerType", menuName = "CustomerSO/CustomerType")]
public class SO_CustomerType : ScriptableObject
{
    public int CustomerTypeId;
    public int PatienceReduction;
    public int OrderCound;
    public string AppearedDialogId;
    public string Patience_100_51_dialogId;
    public string Patience_50_1_dialogId;
    public string Patience_0_dialogId;
    public int SpecialRandomCount;
    public string CustomerType;
}
