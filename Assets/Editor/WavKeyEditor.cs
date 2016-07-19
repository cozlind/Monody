using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;

public class WavKeyEditor : EditorWindow
{
    #region init part
    const int windowWidth = 1366;
    const int windowHeight = 500;
    const int leftWidth = 135;
    const int rightWidth = windowWidth - leftWidth;
    const int itemBarHeight = 17;
    const int scrollBarHeight = 15;
    static int waveFormWidth = 3000;
    const int waveFormHeight = 300;
    static float playHeadTime = 0;
    static float widthBySecond = 18;

    static Rect windowRect = new Rect(100, 150, windowWidth, windowHeight);
    static Rect mouseRect = new Rect(leftWidth, 0, rightWidth, windowHeight - scrollBarHeight);
    static Rect timelineRect = new Rect(0, 0, waveFormWidth, itemBarHeight);
    static Rect waveFormRect = new Rect(0, timelineRect.yMax, waveFormWidth, waveFormHeight);
    static Rect key1Rect = new Rect(leftWidth, waveFormRect.yMax, waveFormWidth, itemBarHeight);
    static Rect key2Rect = new Rect(leftWidth, key1Rect.yMax, waveFormWidth, itemBarHeight);
    static Rect key3Rect = new Rect(leftWidth, key2Rect.yMax, waveFormWidth, itemBarHeight);
    static Rect key4Rect = new Rect(leftWidth, key3Rect.yMax, waveFormWidth, itemBarHeight);
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/WavKey Editor")]
    static void Init()
    {
        EditorWindow.GetWindowWithRect<WavKeyEditor>(windowRect, false, "WavKey Editor");

        UnityEngine.Object[] objects = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);
        if (objects.Length == 0) { disableToolbar = true; return; }
        disableToolbar = false;
        clip = objects[0] as AudioClip;
        GetWaveForm();
        GetBeatList();
    }
    internal class Styles
    {
        public GUIContent playContent = EditorGUIUtility.IconContent("Animation.Play");
        public GUIContent recordContent = EditorGUIUtility.IconContent("Animation.Record");
        public GUIContent prevKeyContent = EditorGUIUtility.IconContent("Animation.PrevKey");
        public GUIContent nextKeyContent = EditorGUIUtility.IconContent("Animation.NextKey");
        public GUIContent addKeyframeContent = EditorGUIUtility.IconContent("Animation.AddKeyframe");
        public GUIContent addEventContent = EditorGUIUtility.IconContent("Animation.AddEvent");
        public GUIContent beat = EditorGUIUtility.IconContent("Animation.EventMarker");
        public GUIStyle curveEditorBackground = "AnimationCurveEditorBackground";
        public GUIStyle eventBackground = "AnimationEventBackground";
        public GUIStyle keyframeBackground = "AnimationKeyframeBackground";
        public GUIStyle rowOdd = "AnimationRowEven";
        public GUIStyle rowEven = "AnimationRowOdd";
        public GUIStyle TimelineTick = "AnimationTimelineTick";

        public GUIStyle keyLabel = new GUIStyle(EditorStyles.largeLabel);



        public Styles()
        {
            keyLabel.alignment = TextAnchor.MiddleCenter;
            keyLabel.fontStyle = FontStyle.Bold;
            keyLabel.normal.background = eventBackground.normal.background;
        }
    }
    Styles styles = new Styles();
    void OnSelectionChange()
    {
        UnityEngine.Object[] objects = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);
        if (objects.Length == 0) { disableToolbar = true; return; }
        disableToolbar = false;
        clip = objects[0] as AudioClip;
        GetWaveForm();
        GetBeatList();
    }
    #endregion

    #region waveform and beat
    static Material handleWireMaterial2D;
    Vector2 scrollPos = Vector2.zero;
    static AudioClip clip;
    static Texture2D waveForm = new Texture2D(waveFormWidth, waveFormHeight);
    static bool disableToolbar = false;
    static void GetWaveForm()
    {
        if (clip == null) return;
        waveFormWidth = Mathf.CeilToInt(clip.length * widthBySecond);
        //combine the texture with multi channel
        if (clip.channels == 1)
        {
            waveForm = AudioUtility.GetWaveForm(clip, 0, waveFormWidth, waveFormHeight);
            return;
        }
        waveForm = new Texture2D(waveFormWidth, waveFormHeight, TextureFormat.ARGB32, false);
        Texture2D[] waveForms = new Texture2D[clip.channels];
        for (int i = 0; i < clip.channels; i++)
        {
            waveForms[i] = AudioUtility.GetWaveForm(clip, i, waveFormWidth, waveFormHeight / clip.channels);

            waveForm.SetPixels(0, waveFormHeight * i / clip.channels, waveFormWidth, waveFormHeight / clip.channels, waveForms[i].GetPixels());
            waveForm.Apply();
        }
    }
    static List<Beat> beatList = new List<Beat>();
    bool isPressed = false;
    bool isKeyPressed = false;
    int beatIndex = 0;
    bool recordToggle = false;
    bool playToggle = false;
    static void GetBeatList()
    {
        beatList.Clear();
        String path = Directory.GetCurrentDirectory() + @"/Assets/ClipBeats/" + clip.name + ".txt";
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] lineSplit = line.Split(":".ToCharArray());
                beatList.Add(new Beat(float.Parse(lineSplit[0]), int.Parse(lineSplit[1])));
            }
        }
    }
    void beatMarkGUI()
    {
        //beat marker
        //find the one clicked
        foreach (var beat in beatList)
        {
            Rect keyRect = new Rect();
            switch (beat.type)
            {
                case 0: keyRect = key1Rect; break;
                case 1: keyRect = key2Rect; break;
                case 2: keyRect = key3Rect; break;
                case 3: keyRect = key4Rect; break;
            }
            float beatx = secondToPixel(beat.time);
            if (beatx - 5 < leftWidth) continue;
            Rect beatRect = new Rect(beatx - 5, keyRect.y, 11, keyRect.height);
            if (beatRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    isPressed = true;
                    beatIndex = beatList.IndexOf(beat);
                    break;
                }
            }
        }
        if (Event.current.type == EventType.MouseUp)
        {
            isPressed = false;
        }
        //left click to drag
        if (isPressed && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            beatList[beatIndex].time += Event.current.delta.x / widthBySecond;
            Repaint();
        }
        //right click to delete
        else if (isPressed && Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            beatList.RemoveAt(beatIndex);
            Repaint();
        }
        //show the beat from beatlist
        foreach (var beat in beatList)
        {
            Rect keyRect = new Rect();
            switch (beat.type)
            {
                case 0: keyRect = key1Rect; break;
                case 1: keyRect = key2Rect; break;
                case 2: keyRect = key3Rect; break;
                case 3: keyRect = key4Rect; break;
            }
            float beatx = secondToPixel(beat.time);
            if (beatx - 5 < leftWidth) continue;
            Rect beatRect = new Rect(beatx - 5, keyRect.y, 11, keyRect.height);
            GUI.Label(beatRect, styles.beat);
        }
        //add new beat by left clicking blank area
        if (!isPressed && key1Rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {//add the beat label
            beatList.Add(new Beat(pixelToSecond(Event.current.mousePosition.x), 0));
        }
        else if (!isPressed && key2Rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            beatList.Add(new Beat(pixelToSecond(Event.current.mousePosition.x), 1));
        }
        else if (!isPressed && key3Rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            beatList.Add(new Beat(pixelToSecond(Event.current.mousePosition.x), 2));
        }
        else if (!isPressed && key4Rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            beatList.Add(new Beat(pixelToSecond(Event.current.mousePosition.x), 3));
        }
        Repaint();
    }
    internal class Beat : IComparable
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
    void recordAndPlayClip()
    {
        if (AudioUtility.IsClipPlaying(clip))
        {
            playHeadTime = AudioUtility.GetClipPosition(clip);
            Repaint();
            if (playHeadTime >= clip.length - float.Epsilon)
            {
                playToggle = false;
                recordToggle = false;
                AudioUtility.StopClip(clip);
            }
            if (!isKeyPressed && Event.current.type == EventType.KeyDown)
            {
                isKeyPressed = true;
                if (recordToggle && isKeyPressed)
                {
                    Beat beat = new Beat();
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.J:
                            beat = new Beat(AudioUtility.GetClipPosition(clip), 0);
                            beatList.Add(beat);
                            break;
                        case KeyCode.K:
                            beat = new Beat(AudioUtility.GetClipPosition(clip), 1);
                            beatList.Add(beat);
                            break;
                        case KeyCode.L:
                            beat = new Beat(AudioUtility.GetClipPosition(clip), 2);
                            beatList.Add(beat);
                            break;
                        case KeyCode.Semicolon:
                            beat = new Beat(AudioUtility.GetClipPosition(clip), 3);
                            beatList.Add(beat);
                            break;
                    }
                }
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                isKeyPressed = false;
            }
            if (!playToggle && !recordToggle)
            {
                AudioUtility.StopClip(clip);
            }
        }
        else if (!AudioUtility.IsClipPlaying(clip) && (playToggle || recordToggle))
        {
            AudioUtility.PlayClip(clip);
            AudioUtility.SetClipSamplePosition(clip, secondToSample(playHeadTime));
        }
    }
    #endregion

    #region gui scalezoom
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            //Left Area
            EditorGUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(leftWidth) });
            {
                //left top toolbar with three buttons
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, new GUILayoutOption[] { GUILayout.Width(leftWidth) });
                {
                    EditorGUI.BeginDisabledGroup(disableToolbar);
                    {
                        recordToggle = GUILayout.Toggle(recordToggle, styles.recordContent, EditorStyles.toolbarButton, new GUILayoutOption[0]);
                        playToggle = GUILayout.Toggle(playToggle, styles.playContent, EditorStyles.toolbarButton, new GUILayoutOption[0]);
                        recordAndPlayClip();
                        if (GUILayout.Button("SAVE", EditorStyles.toolbarButton, new GUILayoutOption[0]))
                        {
                            //StreamWriter第二个参数为false覆盖现有文件，为true则把文本追加到文件末尾
                            String path = Directory.GetCurrentDirectory() + @"/Assets/ClipBeats/";
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            using (StreamWriter file = new StreamWriter(path + clip.name + ".txt", false))
                            {
                                beatList.Sort();
                                foreach (Beat beat in beatList)
                                {
                                    //string line = secondToSample(beat.time) + ":" + beat.type;
                                    string line = beat.time + ":" + beat.type;
                                    file.WriteLine(line);// 直接追加文件末尾，换行   
                                }
                                Debug.Log("save success");
                            }
                        }
                        if (GUILayout.Button("CLR", EditorStyles.toolbarButton, new GUILayoutOption[0]))
                        {
                            beatList.Clear();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndHorizontal();
                //keyRect background
                GUI.Label(new Rect(0, key1Rect.y, leftWidth, key1Rect.height), "J", styles.keyLabel);
                GUI.Label(new Rect(0, key2Rect.y, leftWidth, key2Rect.height), "K", styles.keyLabel);
                GUI.Label(new Rect(0, key3Rect.y, leftWidth, key3Rect.height), "L", styles.keyLabel);
                GUI.Label(new Rect(0, key4Rect.y, leftWidth, key4Rect.height), ";", styles.keyLabel);
            }
            EditorGUILayout.EndVertical();
            //right area
            //background
            GUI.Label(mouseRect, GUIContent.none, styles.curveEditorBackground);
            //right top timeline bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, new GUILayoutOption[0]);
            {
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            //keyRect background
            GUI.Label(key1Rect, GUIContent.none, styles.eventBackground);
            GUI.Label(key2Rect, GUIContent.none, styles.eventBackground);
            GUI.Label(key3Rect, GUIContent.none, styles.eventBackground);
            GUI.Label(key4Rect, GUIContent.none, styles.eventBackground);
            if (clip == null) return;

            //mouse control
            if (Event.current.type == EventType.MouseDown && mouseRect.Contains(Event.current.mousePosition))
            {
                playHeadTime = pixelToSecond(Event.current.mousePosition.x);
                Repaint();
            }
            if (Event.current.type == EventType.scrollWheel)
            {
                float widthBySecondRaw = widthBySecond + HandleUtility.niceMouseDelta;

                //restricted condition: texture2d's width is less than 16383
                if (Mathf.CeilToInt(clip.length * widthBySecondRaw) < 16383)
                    widthBySecond = Mathf.Clamp(widthBySecondRaw, 8, 200);
                GetWaveForm();
                Repaint();
            }
            //scroll view
            scrollPos = GUI.BeginScrollView(new Rect(mouseRect.x, mouseRect.y, mouseRect.width, mouseRect.height + scrollBarHeight), scrollPos, new Rect(0, 0, waveFormWidth, 0), true, false);
            {
                TimeLineGUI();
                //draw waveform
                waveFormRect = new Rect(0, timelineRect.yMax, waveFormWidth, waveFormHeight);
                GUI.Label(waveFormRect, waveForm);
            }
            GUI.EndScrollView();
            drawRedLine();
            beatMarkGUI();
        }
        EditorGUILayout.EndHorizontal();
    }
    int secondToSample(float t)
    {
        return Mathf.RoundToInt(t * clip.samples / clip.length);
    }
    float pixelToSecond(float x)
    {
        return (x - leftWidth + scrollPos.x) / widthBySecond;
    }
    float secondToPixel(float time)
    {
        return time * widthBySecond - scrollPos.x + leftWidth;
    }
    static void applyWireMaterial()
    {
        handleWireMaterial2D = (Material)EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
        handleWireMaterial2D.SetPass(0);
    }
    void drawRedLine()
    {
        applyWireMaterial();
        float x = secondToPixel(playHeadTime);
        if (x < leftWidth) return;
        GL.Begin(GL.LINES);
        GL.Color(new Color(255f, 0, 0, 255f));
        GL.Vertex(new Vector3(x, 0, 0f));
        GL.Vertex(new Vector3(x, windowHeight - scrollBarHeight, 0f));
        GL.End();
    }
    float[] secondByUnit = { 60, 30, 2, 1, 0.5f, 0.1f };//每个单位是多少秒，1min 0.5min 2s 1s 0.5s 0.25s 0.1s
    int[] unitByParent = { 2, 2, 15, 15, 10, 10 };//每个父级单位由多少个基本单位组成，父级单位用长线表示2min, 1min, 30s, 15s,10s, 2s, 1s
    int minWidth = 20;
    int maxWidth = 35;
    int currUnit = 0;
    public void TimeLineGUI()
    {
        float widthByUnit = widthBySecond * secondByUnit[currUnit];
        while (widthByUnit > maxWidth) widthByUnit = widthBySecond * secondByUnit[currUnit = Mathf.Clamp(currUnit + 1, 0, secondByUnit.Length)];
        while (widthByUnit < minWidth) widthByUnit = widthBySecond * secondByUnit[currUnit = Mathf.Clamp(currUnit - 1, 0, secondByUnit.Length)];
        float timeLength = clip.length * widthBySecond < rightWidth ? rightWidth / widthBySecond : clip.length;
        for (int unit = 0; unit * secondByUnit[currUnit] < timeLength; unit++)
        {
            float pos = unit * widthByUnit;
            //if (pos < leftWidth) continue;
            if (unit % unitByParent[currUnit] == 0)
            {
                float longLineLength = 10;
                DrawLine(new Vector2(pos, timelineRect.yMax - longLineLength), new Vector2(pos, timelineRect.yMax), Color.black);
                string text = FormatFrame(Mathf.RoundToInt(unit * secondByUnit[currUnit]), 2);
                GUI.Label(new Rect(pos + 3, -3f, 40f, 20f), text, styles.TimelineTick);
            }
            float shortLineLength = 5;
            DrawLine(new Vector2(pos, timelineRect.yMax - shortLineLength), new Vector2(pos, timelineRect.yMax), Color.black);
        }
    }
    public string FormatFrame(int frame, int frameDigits)
    {
        return frame / (int)60 + ":" + ((float)frame % 60).ToString().PadLeft(frameDigits, '0');
    }
    private static void DrawLine(Vector2 p1, Vector2 p2, Color color)
    {
        applyWireMaterial();
        GL.Begin(1);
        GL.Color(color);
        GL.Vertex(p1);
        GL.Vertex(p2);
        GL.End();
    }
    #endregion
}