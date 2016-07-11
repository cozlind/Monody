using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{

    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("AudioManager instance does not exist");
            }
            return instance;
        }
    }
    [SerializeField]
    AudioClip[] audioList;
    AudioSource audioSource;
    void Awake()
    {
        clipList = new List<AudioClip>();
        audioSource = GetComponent<AudioSource>();
        instance = this;
    }
    void Update()
    {
        string debugText = "dspTime:" + AudioSettings.dspTime.ToString() + "\n";
        if (audioSource && audioSource.clip) debugText += audioSource.clip.name+"\ntime:" + audioSource.time + "\nlength:" + audioSource.clip.length;
        UIManager.Instance.textDebug.text = debugText;
    }
    void Start()
    {
        StartCoroutine(startAudio());
    }
    List<AudioClip> clipList;
    enum Skip { none, skipFirst, skipLast, both };
    public bool isTrigger = false;
    IEnumerator startAudio()
    {
        resetSection();
        yield return playClip("Monody_Section_0_Intro");
        clipList.Add(findClip("Monody_Section_0_Loop"));
        yield return loopClips(clipList.ToArray());

        resetSection();
        yield return playClip("Monody_Section_1_Intro");
        clipList.Add(findClip("Monody_Section_1_Loop_1"));
        clipList.Add(findClip("Monody_Section_1_Loop_2_SkipLast"));
        yield return loopClips(clipList.ToArray(), Skip.skipLast);

        resetSection();
        yield return playClip("Monody_Section_2_Intro");
        clipList.Add(findClip("Monody_Section_2_Loop_1_SkipFirst"));
        clipList.Add(findClip("Monody_Section_2_Loop_2"));
        yield return loopClips(clipList.ToArray(), Skip.skipFirst);

        resetSection();
        yield return playClip("Monody_Section_3_Intro");
        clipList.Add(findClip("Monody_Section_3_Loop"));
        yield return loopClips(clipList.ToArray());

        resetSection();
        clipList.Add(findClip("Monody_Section_4_Loop_1"));
        clipList.Add(findClip("Monody_Section_4_Loop_2_SkipLast"));
        yield return loopClips(clipList.ToArray(), Skip.skipLast);

        resetSection();
        clipList.Add(findClip("Monody_Section_5_Loop_1"));
        clipList.Add(findClip("Monody_Section_5_Loop_2"));
        clipList.Add(findClip("Monody_Section_5_Loop_3_SkipLast"));
        yield return loopClips(clipList.ToArray(), Skip.skipLast);

        yield return playClip("Monody_Section_6_Cadenza_1");
        yield return playClip("Monody_Section_6_Cadenza_2");
        resetSection();
        clipList.Add(findClip("Monody_Section_7_Loop_1"));
        clipList.Add(findClip("Monody_Section_7_Loop_2_SkipLast"));
        yield return loopClips(clipList.ToArray(), Skip.skipLast);

        yield return playClip("Monody_Section_8_Intro");
        yield return playClip("Monody_Section_8_Over");
    }
    void resetSection()
    {
        clipList.Clear();
        isTrigger = false;
    }
    public AudioClip findClip(string name)
    {
        foreach (var clip in audioList)
        {
            if (clip.name.Equals(name))
            {
                return clip;
            }
        }
        Debug.LogError("connot find the clip : " + name);
        return null;
    }
    IEnumerator loopClips(AudioClip[] clips, Skip skip = Skip.none)
    {
        int i = 0;

        //after intro, play from the second loop
        if (skip == Skip.skipFirst || skip == Skip.both) i = 1;

        while (true)
        {
            audioSource.clip = clips[i];
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
            if (isTrigger)
            {
                if (skip == Skip.skipLast || skip == Skip.both)
                {
                    if (i == clips.Length - 2)
                        break;//the audio is replaced by next part
                }
                else if (i == clips.Length - 1)
                    break;//the audio will finished the whole loop, then begin another
            }
            i = (i + 1) % clips.Length;
        }
    }
    IEnumerator playClip(string name)
    {
        audioSource.clip = findClip(name);
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
    }
    public void playClip(AudioClip clip)
    {
        //if (audioSource.isPlaying) audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
