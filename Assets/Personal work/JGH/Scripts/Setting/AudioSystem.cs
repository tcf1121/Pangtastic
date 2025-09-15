using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AudioClipGroup
{
    public string name;
    public AudioClip audioClip;
}

public class AudioSystem : Singleton<AudioSystem>
{
    
    private Coroutine fadeCoroutine;
    
    //public static AudioSystem Instance { get; private set; }

    // [Header("BGM 클립들")]
    // [SerializeField] private List<AudioClipGroup> _bgmClips = new List<AudioClipGroup>();

    // [Header("SFX 클립들")]
    // [SerializeField] private List<AudioClipGroup> _sfxClips = new List<AudioClipGroup>();

    [SerializeField] private AudioClips audioClips;

    // AudioSource 컴포넌트들
    [HideInInspector] public AudioSource BgmAudioSource;
    [HideInInspector] public AudioSource SfxAudioSource;

    protected override void Awake()
    {
        base.Awake();

        // AudioSource 컴포넌트 생성
        BgmAudioSource = gameObject.AddComponent<AudioSource>();
        BgmAudioSource.loop = true;
        BgmAudioSource.volume = 1;
        BgmAudioSource.playOnAwake = false;

        SfxAudioSource = gameObject.AddComponent<AudioSource>();
        SfxAudioSource.loop = false;
        SfxAudioSource.volume = 1;
        SfxAudioSource.playOnAwake = false;

        LoadAudioClips();
    }

    private void LoadAudioClips()
    {

        AsyncOperationHandle<AudioClips> handle = Addressables.LoadAssetAsync<AudioClips>("AudioSO");
        handle.Completed += OnAudioClipsLoaded;
    }

    private void OnAudioClipsLoaded(AsyncOperationHandle<AudioClips> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            audioClips = handle.Result;

            Debug.Log($"AudioClips 로드 완료.");
        }
        else
        {
            Debug.LogError($"StageSO 로드 실패:{handle.OperationException}");
        }
    }


    /// <summary>
    /// BGM 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlayBGM(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < audioClips._bgmClips.Count)
        {
            var clip = audioClips._bgmClips[clipIndex].audioClip;
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
        if (clipIndex >= 0 && clipIndex < audioClips._sfxClips.Count)
        {
            var clip = audioClips._sfxClips[clipIndex].audioClip;
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

    public void SetBGM(bool isOn)
    {
        if (BgmAudioSource != null)
        {
            BgmAudioSource.mute = !isOn;
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

    public void SetSFX(bool isOn)
    {
        if (BgmAudioSource != null)
        {
            SfxAudioSource.mute = !isOn;
        }
    }

    /// <summary>
    /// 이름으로 BGM 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlayBGMByName(string clipName)
    {
        var clipGroup = audioClips._bgmClips.Find(g => g.name == clipName);
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
        var clipGroup = audioClips._sfxClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && SfxAudioSource != null)
        {
            SfxAudioSource.PlayOneShot(clipGroup.audioClip);
        }
        else
        {
            Debug.LogWarning($"SFX 클립 '{clipName}'을 찾을 수 없습니다.");
        }
        
    }
    
    /// <summary>
    /// BGM 재생 (페이드 인)
    /// </summary>
    /// <param name="clipIndex"></param>
    /// <param name="duration"></param>
    public void FadeInBGM(int clipIndex, float duration = 1f, float targetVolume = 1f)
    {
        if (clipIndex >= 0 && clipIndex < audioClips._bgmClips.Count)
        {
            var clip = audioClips._bgmClips[clipIndex].audioClip;
            if (clip != null && BgmAudioSource != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

                BgmAudioSource.clip = clip;
                BgmAudioSource.volume = 0f;
                BgmAudioSource.Play();

                fadeCoroutine = StartCoroutine(FadeVolume(BgmAudioSource, targetVolume, duration));
            }
        }
    }

    /// <summary>
    /// 이름으로 BGM 재생 (페이드 인)
    /// </summary>
    public void FadeInBGMByName(string clipName, float duration = 1f, float targetVolume = 1f)
    {
        var clipGroup = audioClips._bgmClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && BgmAudioSource != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            BgmAudioSource.clip = clipGroup.audioClip;
            BgmAudioSource.volume = 0f;
            BgmAudioSource.Play();

            fadeCoroutine = StartCoroutine(FadeVolume(BgmAudioSource, targetVolume, duration));
        }
        else
        {
            Debug.LogWarning($"BGM 클립 '{clipName}'을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// BGM 페이드 아웃 후 정지
    /// </summary>
    public void FadeOutBGM(float duration = 1f)
    {
        if (BgmAudioSource != null && BgmAudioSource.isPlaying)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOutAndStop(BgmAudioSource, duration));
        }
    }

    /// <summary>
    /// 볼륨 점진적 변경
    /// </summary>
    private IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    /// <summary>
    /// 페이드 아웃 후 정지
    /// </summary>
    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }


}