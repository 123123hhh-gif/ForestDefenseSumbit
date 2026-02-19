using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }


    private AudioSource bgmSource;

    private Queue<AudioSource> battleSfxPool = new Queue<AudioSource>();


    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Range(0f, 1f)] public float battleVolume = 0.7f;

    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);


        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;
    }

    #region ±³¾°Òô£¨BGM£©¿ØÖÆ

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }


    public void PauseBGM() => bgmSource.Pause();

    public void StopBGM() => bgmSource.Stop();

    public void SetBGMVolume(float volume) => bgmSource.volume = Mathf.Clamp01(volume);
    #endregion

    #region Õ½¶·ÒôÐ§£¨Battle SFX£©¿ØÖÆ

    public void PlayBattleSFX(AudioClip clip, Vector3 pos = default)
    {
        if (clip == null) return;


        AudioSource sfxSource = battleSfxPool.Count > 0 ? battleSfxPool.Dequeue() : CreateNewSfxSource();
        sfxSource.transform.position = pos;
        sfxSource.clip = clip;
        sfxSource.loop = false;
        sfxSource.volume = battleVolume;
        sfxSource.Play();


        StartCoroutine(RecycleSfx(sfxSource, clip.length));
    }


    public void SetBattleVolume(float volume) => battleVolume = Mathf.Clamp01(volume);


    private AudioSource CreateNewSfxSource()
    {
        GameObject obj = new GameObject("BattleSFX");
        obj.transform.SetParent(transform);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        return source;
    }


    private IEnumerator RecycleSfx(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.clip = null;
        battleSfxPool.Enqueue(source);
    }
    #endregion
}