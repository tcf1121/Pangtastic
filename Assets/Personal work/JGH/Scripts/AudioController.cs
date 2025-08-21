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
    private AudioSource _bgmAudioSource;
    private AudioSource _sfxAudioSource;
    
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
        _bgmAudioSource = gameObject.AddComponent<AudioSource>();
        _bgmAudioSource.loop = true;
        _bgmAudioSource.playOnAwake = false;
        
        _sfxAudioSource = gameObject.AddComponent<AudioSource>();
        _sfxAudioSource.loop = false;
        _sfxAudioSource.playOnAwake = false;
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
            if (clip != null && _bgmAudioSource != null)
            {
                _bgmAudioSource.clip = clip;
                _bgmAudioSource.Play();
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
            if (clip != null && _sfxAudioSource != null)
            {
                _sfxAudioSource.PlayOneShot(clip);
            }
        }
    }
    
    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (_bgmAudioSource != null)
        {
            _bgmAudioSource.Stop();
        }
    }
    
    /// <summary>
    /// 재생 중인 SFX 정지
    /// </summary>
    public void StopSFX()
    {
        if (_sfxAudioSource != null && _sfxAudioSource.isPlaying)
        {
            _sfxAudioSource.Stop();
        }
    }

    /// <summary>
    /// 이름으로 BGM 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlayBGMByName(string clipName)
    {
        var clipGroup = _bgmClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && _bgmAudioSource != null)
        {
            _bgmAudioSource.clip = clipGroup.audioClip;
            _bgmAudioSource.Play();
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
        if (clipGroup != null && clipGroup.audioClip != null && _sfxAudioSource != null)
        {
            _sfxAudioSource.PlayOneShot(clipGroup.audioClip);
        }
        else
        {
            Debug.LogWarning($"SFX 클립 '{clipName}'을 찾을 수 없습니다.");
        }
    }

}