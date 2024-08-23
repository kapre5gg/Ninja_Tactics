using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string soundName; // 곡의 이름
    public AudioClip clip; // 곡
}

public class SoundManager : MonoBehaviour
{
    static public SoundManager instance; // 자기 자신을 인스턴스로 만듦

    #region Singleton
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [Header("오디오클립")]
    [SerializeField]
    public Sound[] bgmSounds;
    [SerializeField]
    public Sound[] sfxSounds;

    [Header("브금플레이어")]
    [SerializeField]
    public AudioSource bgmPlayer;

    [Header("효과음플레이어")]
    [SerializeField]
    public AudioSource[] sfxPlayer;
    public string[] playSoundName;

    private int lastBGMIndex = -1; // 이전에 재생된 BGM의 인덱스를 저장

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        PlayRandomBGM(); // 초기 BGM 재생
    }

    public void PlaySE(string _soundName) // 특정 사운드를 재생
    {
        for (int i = 0; i < sfxSounds.Length; i++)
        {
            if (_soundName == sfxSounds[i].soundName)
            {
                for (int x = 0; x < sfxPlayer.Length; x++)
                {
                    if (!sfxPlayer[x].isPlaying)
                    {
                        playSoundName[x] = sfxSounds[i].soundName;
                        sfxPlayer[x].clip = sfxSounds[i].clip;
                        sfxPlayer[x].Play();
                        return;
                    }
                }
                Debug.Log("모든 sfxPlayer 오디오소스가 사용 중입니다.");
                return;
            }
        }
        Debug.Log(_soundName + " 사운드가 SoundManager에 등록되지 않았습니다.");
    }

    public void StopAllSE() // 모든 사운드 중지
    {
        for (int i = 0; i < sfxPlayer.Length; i++)
        {
            sfxPlayer[i].Stop();
        }
    }

    public void StopSE(string _soundName) // 특정 사운드만 중지
    {
        for (int i = 0; i < sfxPlayer.Length; i++)
        {
            if (playSoundName[i] == _soundName)
            {
                sfxPlayer[i].Stop();
                return;
            }
        }
        Debug.Log("재생 중인 " + _soundName + " 이 없습니다.");
    }

    private void PlayRandomBGM()
    {
        int random;

        // 이전에 재생한 BGM과 다른 곡을 선택
        do
        {
            random = Random.Range(0, bgmSounds.Length);
        } while (random == lastBGMIndex);

        lastBGMIndex = random; // 현재 재생된 BGM 인덱스 저장

        bgmPlayer.clip = bgmSounds[random].clip;
        bgmPlayer.Play();

        // 현재 곡의 길이만큼 대기 후 다음 곡 재생
        StartCoroutine(PlayNextBGMAfterDelay(bgmPlayer.clip.length));
    }

    private IEnumerator PlayNextBGMAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayRandomBGMWithFade();
    }

    private void PlayRandomBGMWithFade()
    {
        StartCoroutine(FadeOutAndPlayNewBGM());
    }

    private IEnumerator FadeOutAndPlayNewBGM()
    {
        // 페이드 아웃
        float fadeDuration = 1.5f;
        float startVolume = bgmPlayer.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmPlayer.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        bgmPlayer.volume = 0;

        // 새로운 BGM 재생
        PlayRandomBGM();

        // 페이드 인
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmPlayer.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        bgmPlayer.volume = startVolume;
    }
}
