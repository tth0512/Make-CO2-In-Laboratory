using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-200)]
public class AudioManager : MonoBehaviour
{
    [Header("Background Music")]
    [SerializeField] private AudioClip bgr_music1;
    [SerializeField] private AudioClip bgr_music2;

    [Header("Correct")]
    [SerializeField] private AudioClip sfx_correct;
    [SerializeField] private AudioClip sfx_goodjob;

    [Header("Next Level")]
    [SerializeField] private AudioClip sfx_nextlevel;
    [SerializeField] private AudioClip sfx_nextQuestion;

    [Header("Wrong")]
    [SerializeField] private AudioClip sfx_answer_wrong;
    [SerializeField] private AudioClip sfx_wrong_buzzer;

    [Header("Pop Sound")]
    [SerializeField] private AudioClip sfx_bubble0;
    [SerializeField] private AudioClip sfx_bubble1;

    private AudioSource bgrMusic1Source;
    private AudioSource bgrMusic2Source;
    private AudioSource sfxCorrectSource;
    private AudioSource sfxGoodjobSource;
    private AudioSource sfxNextLevelSource;
    private AudioSource sfxNextQuestionSource;
    private AudioSource sfxAnswerWrongSource;
    private AudioSource sfxWrongBuzzerSource;
    private AudioSource sfxBubble0Source;
    private AudioSource sfxBubble1Source;

    private Coroutine correctSequenceCoroutine;

    private static AudioManager _instance;
    public static AudioManager Ins
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance);
        }
        else 
            _instance = this;
        
        bgrMusic1Source = CreateSource(bgr_music1, true);
        bgrMusic2Source = CreateSource(bgr_music2, true);
        sfxCorrectSource = CreateSource(sfx_correct, false);
        sfxGoodjobSource = CreateSource(sfx_goodjob, false);
        sfxNextLevelSource = CreateSource(sfx_nextlevel, false);
        sfxNextQuestionSource = CreateSource(sfx_nextQuestion, false);
        sfxAnswerWrongSource = CreateSource(sfx_answer_wrong, false);
        sfxWrongBuzzerSource = CreateSource(sfx_wrong_buzzer, false);
        sfxBubble0Source = CreateSource(sfx_bubble0, false);
        sfxBubble1Source = CreateSource(sfx_bubble1, false);
    }

    public void PlayBackgroundMusic(int index)
    {
        switch (index)
        {
            case 0:
                PlaySound(bgrMusic1Source, true);
                break;
            case 1:
                PlaySound(bgrMusic2Source, true);
                break;
            default:
                Debug.LogWarning("Invalid background music index: " + index);
                break;
        }
    }

    public void PauseBackgroundMusic()
    {
        PauseSound(bgrMusic1Source);
        PauseSound(bgrMusic2Source);
    }

    public void PlayCorrectSound()
    {
        if (correctSequenceCoroutine != null)
        {
            StopCoroutine(correctSequenceCoroutine);
        }

        correctSequenceCoroutine = StartCoroutine(PlayCorrectSequence());
    }

    public void PlayWrongSound()
    {
        PlaySound(sfxAnswerWrongSource, true);
    }

    public void PlayBubbleHoverSound()
    {
        StopBubbleSounds();
        PlaySound(sfxBubble0Source, true);
    }

    public void PlayBubbleInteractSound()
    {
        StopBubbleSounds();
        PlaySound(sfxBubble1Source, true);
    }

    private void StopBubbleSounds()
    {
        if (sfxBubble0Source != null && sfxBubble0Source.isPlaying)
            sfxBubble0Source.Stop();
        if (sfxBubble1Source != null && sfxBubble1Source.isPlaying)
            sfxBubble1Source.Stop();
    }

    public void PauseCorrectSound()
    {
        PauseSound(sfxCorrectSource);
    }

    private AudioSource CreateSource(AudioClip clip, bool loop)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        return source;
    }

    private void PlaySound(AudioSource source, bool restart)
    {
        if (source == null || source.clip == null) return;
        if (restart) source.time = 0f;
        if (!source.isPlaying) source.Play();
    }

    private void PauseSound(AudioSource source)
    {
        if (source == null) return;
        if (source.isPlaying) source.Pause();
    }

    private IEnumerator PlayCorrectSequence()
    {
        PlaySound(sfxCorrectSource, true);

        float delay = sfxCorrectSource != null && sfxCorrectSource.clip != null
            ? sfxCorrectSource.clip.length
            : 0f;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        PlaySound(sfxGoodjobSource, true);
        correctSequenceCoroutine = null;
    }

}
