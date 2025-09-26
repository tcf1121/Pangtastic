using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public int DialogId;
    public int Order;

    public Speaker LeftSpeaker;
    public Speaker CenterSpeaker;
    public Speaker RightSpeaker;

    public int SpeakerCheck;
    public StringSO DialogueStringSO;
    public Sprite BackgroundSprite;
    public string BgmSoundId;
    public string SfxSoundId;
}

[CreateAssetMenu(fileName = "New DialogueSO", menuName = "PangTasticSO/Dialogue")]
public class DialogueGroupSO : ScriptableObject
{
    public int dialogGroup;
    public List<DialogueLine> lines;
}
