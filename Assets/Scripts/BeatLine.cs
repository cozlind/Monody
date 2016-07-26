using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public class BeatLine : MonoBehaviour
{
    private static BeatLine instance;
    public static BeatLine Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("the beatline is null");
            }
            return instance;
        }
    }
    public float rotateSpeed = 200;
    public float maxLength = 50;
    static Vector3 direct = new Vector3(1, 0, 0);
    Ray ray;
    public List<RaycastHit> hitList = new List<RaycastHit>();

    GameObject keyJ, keyK, keyL, keySmc;
    public float moveSpeed = 50;
    public float maxTime;
    int index = 0;
    GameObject key;
    Vector3 pos;
    bool isStart;
    [NonSerialized]
    public bool isUpdateHits = true;
    void Awake()
    {
        isStart = true;
        instance = this;
        keyJ = Resources.Load("Prefabs/KeyJ") as GameObject;
        keyK = Resources.Load("Prefabs/KeyK") as GameObject;
        keyL = Resources.Load("Prefabs/KeyL") as GameObject;
        keySmc = Resources.Load("Prefabs/KeySmc") as GameObject;
    }
    void Update()
    {
        //float inputX = Input.GetAxis("Horizontal") * Time.deltaTime * rotateSpeed;
        //direct = Quaternion.Euler(-Vector3.forward * inputX) * direct;
        direct = transform.right;
        //get the whole hit point within the range of distance
        if (Input.GetAxis("Horizontal") != 0 || isUpdateHits)
        {
            isUpdateHits = false;
            //draw basic ray
            RaycastHit hit;
            Vector3 origin = transform.position;
            Vector3 dir = direct;
            float distance = maxLength;
            hitList = new List<RaycastHit>();
            if (Physics.Raycast(origin, dir, out hit))
                while (Physics.Raycast(origin, dir, out hit))
                {
                    float hitDistance = Vector3.Distance(hit.point, origin);
                    if (distance - hitDistance < 0)//less than the distance among the hitpoint
                    {
                        hit.point = origin + (hit.point - origin).normalized * distance;
                        hitList.Add(hit);
                        break;
                    }
                    distance -= hitDistance;
                    hitList.Add(hit);
                    origin = hit.point;
                    dir = Vector3.Reflect(dir, hit.normal);
                }
            else
            {
                hit.point = origin + dir.normalized * maxLength;
                hitList.Add(hit);
            }
        }
        //Draw Lines
        DrawLines();
        //Draw Beats
        maxTime = maxLength / moveSpeed;
        float beatTime = AudioManager.Instance.audioSource.time + maxTime;

        if (AudioManager.Instance.nextClip != null
            && AudioManager.Instance.nextBeatList != null
            && beatTime > AudioManager.Instance.audioSource.clip.length
            && beatTime <= AudioManager.Instance.audioSource.clip.length + maxTime
            && beatTime <= AudioManager.Instance.audioSource.clip.length + AudioManager.Instance.nextClip.length)
        {
            //Debug.Log(1 + ":" + beatTime + ":" + (beatTime - AudioManager.Instance.audioSource.clip.length));
            beatTime -= AudioManager.Instance.audioSource.clip.length;
            if (beatTime <= AudioManager.Instance.nextBeatList[0].time) index = 0;
            instantiateKey(ref index, beatTime, AudioManager.Instance.nextBeatList);
        }
        else if (AudioManager.Instance.nextClip != null
            && AudioManager.Instance.nextNextBeatList != null
            && beatTime > AudioManager.Instance.audioSource.clip.length + AudioManager.Instance.nextClip.length
            && beatTime <= AudioManager.Instance.audioSource.clip.length + maxTime)
        {
            //Debug.Log(2 + ":" + beatTime + ":" + (beatTime - AudioManager.Instance.audioSource.clip.length - AudioManager.Instance.nextClip.length));
            beatTime -= AudioManager.Instance.audioSource.clip.length + AudioManager.Instance.nextClip.length;
            if (beatTime <= AudioManager.Instance.nextNextBeatList[0].time) index = 0;
            instantiateKey(ref index, beatTime, AudioManager.Instance.nextNextBeatList);
        }
        else if (AudioManager.Instance.beatList != null
            && beatTime <= AudioManager.Instance.audioSource.clip.length)
        {
            //Debug.Log(3 + ":" + beatTime);
            instantiateKey(ref index, beatTime, AudioManager.Instance.beatList);
        }
    }
    void instantiateKey(ref int index, float beatTime, List<Beat> list)
    {
        if (list == null) return;
        float distance = maxLength;
        while (index < list.Count)
        {
            Beat beat = list[index];
            if (beat.time >= beatTime)
            {
                if (isStart)
                    isStart = false;
                break;
            }
            if (isStart) distance = (beat.time + AudioManager.Instance.audioSource.clip.length) * moveSpeed;
            getPointPos(distance, out pos);
            switch (beat.type)
            {
                case 0:
                    key = Instantiate(keyJ, pos, Quaternion.identity) as GameObject;
                    break;
                case 1:
                    key = Instantiate(keyK, pos, Quaternion.identity) as GameObject;
                    break;
                case 2:
                    key = Instantiate(keyL, pos, Quaternion.identity) as GameObject;
                    break;
                case 3:
                    key = Instantiate(keySmc, pos, Quaternion.identity) as GameObject;
                    break;
            }
            // Debug.Log(index + "/" + list.Count + ":" + beat.time + "/" + beatTime + ":" + distance);
            key.GetComponent<BeatUnit>().distance = distance;
            index++;
        }
    }
    public void getPointPos(float distance, out Vector3 pos)
    {
        Ray ray = new Ray(transform.position, hitList[0].point);
        for (int i = 0; i < hitList.Count - 1; i++)
        {
            Vector3 hitPoint = hitList[i].point;
            float hitDistance = Vector3.Distance(hitPoint, ray.origin);
            if (distance - hitDistance < 0)//less than the distance among the hitpoint
            {
                ray.direction = hitPoint - ray.origin;
                pos = ray.GetPoint(distance);
                return;
            }
            distance -= hitDistance;
            ray.origin = hitPoint;
        }
        //above the last hitpoint or no hitpoint exist
        ray.direction = hitList[hitList.Count - 1].point - ray.origin;
        pos = ray.GetPoint(distance);
    }
    void DrawLines()
    {
        List<Vector3> pointList = new List<Vector3>();
        pointList.Add(transform.position);
        foreach (var hit in hitList)
        {
            pointList.Add(hit.point);
        }
        GetComponent<LineRenderer>().SetVertexCount(pointList.Count);
        GetComponent<LineRenderer>().SetPositions(pointList.ToArray());
    }
    void OnDrawGizmos()
    {
        try
        {
            float distance = maxLength;
            Gizmos.color = Color.yellow;
            Ray ray = new Ray(transform.position, direct);
            for (int i = 0; i < hitList.Count - 1; i++)
            {
                RaycastHit hit = hitList[i];
                ray.direction = hit.point - ray.origin;
                float hitDistance = Vector3.Distance(hit.point, ray.origin);
                if (distance - hitDistance < 0)//less than the distance among the hitpoint
                {
                    Gizmos.DrawRay(ray.origin, ray.direction * distance);
                    return;
                }
                Gizmos.DrawRay(ray.origin, ray.direction * hitDistance);
                distance -= hitDistance;
                ray.origin = hit.point;
            }
            //above the last hitpoint or no hitpoint exist
            ray.direction = hitList[hitList.Count - 1].point - ray.origin;
            Gizmos.DrawRay(ray.origin, ray.direction * distance);
        }
        catch { }
    }
}
