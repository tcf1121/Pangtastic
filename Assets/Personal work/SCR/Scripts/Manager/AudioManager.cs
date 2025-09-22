using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class AudioManager : Singleton<AudioManager>
{

    [SerializeField] private AudioClips audioClips;
    [HideInInspector] public AudioSource BgmAudioSource;
    [HideInInspector] public AudioSource SfxAudioSource;
    private MissionPlace curPlace;
    public bool OnVibrate { get { return _onVibrate; } }
    private bool _onVibrate;
#if UNITY_ANDROID
    private static AndroidJavaObject vibrator;
#endif
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

    public void SetVibrate(bool value)
    {
        _onVibrate = value;
    }

    public void SetLobbyPlace(MissionPlace place = MissionPlace.Donut)
    {
        curPlace = place;
    }

    public void PlayLobbyBGM()
    {
        var clip = audioClips.LobbyBGM(curPlace);
        if (clip != null)
        {
            BgmAudioSource.clip = clip;
            BgmAudioSource.Play();
        }
    }

    public void PlayPlaceBGM(MissionPlace place)
    {
        var clip = audioClips.LobbyBGM(place);
        if (clip != null)
        {
            BgmAudioSource.clip = clip;
            BgmAudioSource.Play();
        }
    }

    public void PlayPuzzleBGM()
    {
        var clip = audioClips.PuzzleBGM();
        if (clip != null)
        {
            BgmAudioSource.clip = clip;
            BgmAudioSource.Play();
        }
    }

    public string GetBGMName(int index)
    {
        Debug.Log(audioClips.Lobby[index].name);
        return audioClips.Lobby[index].name;
    }


    /// <summary>
    /// SFX 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlaySFX(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < audioClips.SFX.Count)
        {
            var clip = audioClips.SFX[clipIndex];
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
    /// 이름으로 SFX 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlaySFX(string clipName)
    {
#if UNITY_ANDROID
        if (_onVibrate)
            if (clipName == "Block_Match" ||
            clipName == "Block_MakeSpecial")
            {
                Vibration();
            }
#endif
        var clipGroup = audioClips.SFX.Find(g => g.name == clipName);
        if (clipGroup != null)
        {
            SfxAudioSource.PlayOneShot(clipGroup);
        }
        else
        {
            Debug.LogWarning($"SFX 클립 '{clipName}'을 찾을 수 없습니다.");
        }
    }

    private void Vibration()
    {
#if UNITY_ANDROID
        Handheld.Vibrate();
#else
        
        Debug.Log("진동하는 중");
#endif
    }

}
