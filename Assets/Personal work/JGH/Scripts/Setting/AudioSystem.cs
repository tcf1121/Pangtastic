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

namespace JGH
{
// <<<<<<< HEAD
    public class AudioSystem : MonoBehaviour
    {

        private Coroutine fadeCoroutine;

        //public static AudioSystem Instance { get; private set; }


        [SerializeField] private AudioClips audioClips;

        [Header("BGM 클립들")] [SerializeField] private List<AudioClipGroup> _bgmClips = new List<AudioClipGroup>();

        [Header("SFX 클립들")] [SerializeField] private List<AudioClipGroup> _sfxClips = new List<AudioClipGroup>();

        // AudioSource 컴포넌트들
        [HideInInspector] public AudioSource BgmAudioSource;
        [HideInInspector] public AudioSource SfxAudioSource;

        protected void Awake()
        {
            //base.Awake();

            // AudioSource 컴포넌트 생성
            BgmAudioSource = gameObject.AddComponent<AudioSource>();
            BgmAudioSource.loop = true;
            BgmAudioSource.volume = 1;
            BgmAudioSource.playOnAwake = false;

            SfxAudioSource = gameObject.AddComponent<AudioSource>();
            SfxAudioSource.loop = false;
            SfxAudioSource.volume = 1;
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
}
// =======
// }
// >>>>>>> Develop
