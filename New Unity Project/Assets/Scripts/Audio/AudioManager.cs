using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音频播放器")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("背景音乐淡入淡出")]
    [Range(0.1f, 5f)]
    public float bgmFadeDuration = 0.8f;

    private Coroutine bgmFadeCoroutine;

    private void Awake()
    {
        // 如果已经存在另一个 AudioManager，就销毁重复的对象
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 注册当前实例
        Instance = this;

        // 切换场景时保留这个对象
        DontDestroyOnLoad(gameObject);

        if (bgmSource != null)
        {
            bgmSource.volume = 0f;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 保持 BGM 过渡不中断：切场景时不重置当前 AudioSource 的播放状态
        if (bgmSource == null)
        {
            return;
        }

        if (bgmSource.clip != null && bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            if (bgmSource.volume < 0.01f)
            {
                bgmSource.volume = 0.05f;
            }
        }
    }

    // 播放背景音乐，可选淡入时间
    public void PlayBGM(AudioClip clip, float duration = -1f)
    {
        if (clip == null) return;

        if (bgmSource == null)
        {
            Debug.LogWarning("BGM AudioSource 未分配");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        float fadeTime = duration > 0f ? duration : bgmFadeDuration;
        bgmFadeCoroutine = StartCoroutine(FadeInBGM(clip, fadeTime));
    }

    // 停止背景音乐，可选淡出时间
    public void StopBGM(float duration = -1f)
    {
        if (bgmSource == null)
            return;

        if (!bgmSource.isPlaying && bgmSource.clip == null)
            return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        float fadeTime = duration > 0f ? duration : bgmFadeDuration;
        bgmFadeCoroutine = StartCoroutine(FadeOutBGM(fadeTime));
    }

    // 播放音效
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxSource == null)
        {
            Debug.LogWarning("SFX AudioSource 未分配");
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    private IEnumerator FadeInBGM(AudioClip clip, float duration)
    {
        float startVolume = bgmSource.isPlaying ? bgmSource.volume : 0f;

        if (bgmSource.isPlaying && bgmSource.clip != null && bgmSource.clip != clip)
        {
            yield return StartCoroutine(FadeVolume(bgmSource, startVolume, 0f, duration * 0.5f));
            bgmSource.Stop();
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
        bgmSource.volume = 0f;

        yield return StartCoroutine(FadeVolume(bgmSource, 0f, 1f, duration));
        bgmFadeCoroutine = null;
    }

    private IEnumerator FadeOutBGM(float duration)
    {
        if (!bgmSource.isPlaying && bgmSource.clip == null)
        {
            bgmFadeCoroutine = null;
            yield break;
        }

        float startVolume = bgmSource.volume;
        yield return StartCoroutine(FadeVolume(bgmSource, startVolume, 0f, duration));

        bgmSource.Stop();
        bgmSource.clip = null;
        bgmSource.volume = 0f;
        bgmFadeCoroutine = null;
    }

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        if (source == null)
            yield break;

        if (duration <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        source.volume = to;
    }
}