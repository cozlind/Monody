using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

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
    [NonSerialized]
    public AudioSource audioSource;
    void Awake()
    {
        clipList = new List<AudioClip>();
        audioSource = GetComponent<AudioSource>();
        instance = this;
        beatDict = new Dictionary<string, List<Beat>>();
        getBeatDict();
    }
    void Update()
    {
        string debugText = "dspTime:" + AudioSettings.dspTime.ToString() + "\n";
        if (audioSource && audioSource.clip) debugText += audioSource.clip.name + "\ntime:" + audioSource.time + "\nlength:" + audioSource.clip.length;
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
        clipList.Add(findClip("Monody_Section_0_Loop"));
        yield return playClip("Monody_Section_0_Intro");
        yield return loopClips(clipList.ToArray());

        resetSection();
        clipList.Add(findClip("Monody_Section_1_Loop_1"));
        clipList.Add(findClip("Monody_Section_1_Loop_2_SkipLast"));
        yield return playClip("Monody_Section_1_Intro");
        yield return loopClips(clipList.ToArray(), Skip.skipLast);

        resetSection();
        clipList.Add(findClip("Monody_Section_2_Loop_1_SkipFirst"));
        clipList.Add(findClip("Monody_Section_2_Loop_2"));
        yield return playClip("Monody_Section_2_Intro");
        yield return loopClips(clipList.ToArray(), Skip.skipFirst);

        resetSection();
        clipList.Add(findClip("Monody_Section_3_Loop"));
        yield return playClip("Monody_Section_3_Intro");
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

    public IEnumerator emphasizeVolume()
    {
        audioSource.volume *= 1.4f;
        yield return new WaitForSeconds(0.5f);
        audioSource.volume /= 1.4f;
    }
    #region beat list reader
    public Dictionary<string, List<Beat>> beatDict = new Dictionary<string, List<Beat>>();
    public List<Beat> beatList
    {
        get
        {
            if (audioSource.clip==null||!beatDict.ContainsKey(audioSource.clip.name))
            {
                return null;
            }
            return beatDict[audioSource.clip.name];
        }
    }
    public AudioClip nextClip
    {
        get
        {
            if (isTrigger) return null;
            int index = clipList.IndexOf(audioSource.clip);
            if (index == -1)
            {
                return null;
            }
            index = (index + 1) % clipList.Count;
            return clipList[index % clipList.Count];
        }
    }
    public List<Beat> nextBeatList
    {
        get
        {
            if (isTrigger) return null;
            int index = clipList.IndexOf(audioSource.clip);
            if (index == -1)
            {
                return null;
            }
            index = (index + 1) % clipList.Count;
            string nextClipName = clipList[index].name;
            if (!beatDict.ContainsKey(nextClipName))
            {
                return null;
            }
            return beatDict[nextClipName];
        }
    }
    public List<Beat> nextNextBeatList
    {
        get
        {
            if (isTrigger) return null;
            int index = clipList.IndexOf(audioSource.clip);
            if (index == -1)
            {
                return null;
            }
            index = (index + 2) % clipList.Count;
            string nextNextClipName = clipList[index].name;
            if (!beatDict.ContainsKey(nextNextClipName))
            {
                return null;
            }
            return beatDict[nextNextClipName];
        }
    }
    void getBeatDict()
    {
        foreach (var clip in audioList)
        {
            TextAsset textArray = Resources.Load<TextAsset>("ClipBeats/" + clip.name);
            if (textArray == null) {  continue; }
            List<Beat> beatList = new List<Beat>();
            string[] lines = textArray.text.Split('\n');
            foreach (string line in lines)
            {
                if (line.Length<2) break;//the line feed at the end of the file
                string[] lineSplit = line.Split(':');
                Beat b=new Beat(float.Parse(lineSplit[0]), int.Parse(lineSplit[1]));
                beatList.Add(b);
            }
            beatDict.Add(clip.name, beatList);
        }
    }
    #endregion
}

public class Beat : IComparable
{
    public float time;
    public int type;//[0,3]
    public Beat()
    {
        time = -1;
        type = -1;
    }
    int IComparable.CompareTo(object obj)
    {
        Beat beat = obj as Beat;
        if (time - beat.time > float.Epsilon) return 1;
        if (beat.time - time > float.Epsilon) return -1;
        return 0;
    }
    public Beat(float t, int ty)
    {
        time = t;
        type = ty;
    }
}