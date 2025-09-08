using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ScriptDialog", menuName = "ScriptSystem/ScriptDialogData")]
public class SO_ScriptDialog : ScriptableObject
{
    public List<ScriptingSystem.DialogData> dialogs;
}