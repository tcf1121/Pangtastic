using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClip", menuName = "Sound/AudioClip")]
public class AudioClips : ScriptableObject
{
    [Header("BGM 클립들")]
    [SerializeField] public List<AudioClipGroup> _bgmClips = new List<AudioClipGroup>();

    [Header("SFX 클립들")]
    [SerializeField] public List<AudioClipGroup> _sfxClips = new List<AudioClipGroup>();
}
