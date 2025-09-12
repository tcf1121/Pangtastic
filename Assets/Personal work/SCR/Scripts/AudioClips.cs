using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClip", menuName = "Sound/AudioClip")]
public class AudioClips : ScriptableObject
{
    [Header("로비 BGM")]
    public List<AudioClip> Lobby = new List<AudioClip>();

    [Header("퍼즐 BGM")]
    public AudioClip Puzzle;

    [Header("SFX 클립들")]
    public List<AudioClip> SFX = new List<AudioClip>();

    public AudioClip PuzzleBGM()
    {
        return Puzzle;
    }

    public AudioClip LobbyBGM(MissonPlace place = MissonPlace.Donut)
    {
        return Lobby[(int)place];

    }
}