using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AudioClipGroup
{
    public string name;
    public AudioClip audioClip;
}

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }
    
    [Header("BGM 클립들")]
    [SerializeField] private List<AudioClipGroup> _bgmClips = new List<AudioClipGroup>();
    
    [Header("SFX 클립들")]
    [SerializeField] private List<AudioClipGroup> _sfxClips = new List<AudioClipGroup>();
    
    // AudioSource 컴포넌트들
    [HideInInspector] public AudioSource BgmAudioSource;
    [HideInInspector] public AudioSource SfxAudioSource;
    
    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // AudioSource 컴포넌트 생성
        BgmAudioSource = gameObject.AddComponent<AudioSource>();
        BgmAudioSource.loop = true;
        BgmAudioSource.playOnAwake = false;
        
        SfxAudioSource = gameObject.AddComponent<AudioSource>();
        SfxAudioSource.loop = false;
        SfxAudioSource.playOnAwake = false;
    }



    /// <summary>
    /// BGM 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlayBGM(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < _bgmClips.Count)
        {
            var clip = _bgmClips[clipIndex].audioClip;
            if (clip != null && BgmAudioSource != null)
            {
                BgmAudioSource.clip = clip;
                BgmAudioSource.Play();
            }
        }
    }

    /// <summary>
    /// SFX 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlaySFX(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < _sfxClips.Count)
        {
            var clip = _sfxClips[clipIndex].audioClip;
            if (clip != null && SfxAudioSource != null)
            {
                SfxAudioSource.PlayOneShot(clip);
            }
        }
    }
    
    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (BgmAudioSource != null)
        {
            BgmAudioSource.Stop();
        }
    }
    
    /// <summary>
    /// 재생 중인 SFX 정지
    /// </summary>
    public void StopSFX()
    {
        if (SfxAudioSource != null && SfxAudioSource.isPlaying)
        {
            SfxAudioSource.Stop();
        }
    }

    /// <summary>
    /// 이름으로 BGM 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlayBGMByName(string clipName)
    {
        var clipGroup = _bgmClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && BgmAudioSource != null)
        {
            BgmAudioSource.clip = clipGroup.audioClip;
            BgmAudioSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM 클립 '{clipName}'을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 이름으로 SFX 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlaySFXByName(string clipName)
    {
        var clipGroup = _sfxClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && SfxAudioSource != null)
        {
            SfxAudioSource.PlayOneShot(clipGroup.audioClip);
        }
        else
        {
            Debug.LogWarning($"SFX 클립 '{clipName}'을 찾을 수 없습니다.");
        }
    }

}