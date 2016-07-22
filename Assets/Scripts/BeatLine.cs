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
    static Vector3 newDirect = Vector3.zero;
    Ray ray;
    List<RaycastHit> hitList = new List<RaycastHit>();

    GameObject keyJ, keyK, keyL, keySmc;
    void Awake()
    {
        isStart = true;
        instance = this;
        keyJ = Resources.Load("Prefabs/KeyJ") as GameObject;
        keyK = Resources.Load("Prefabs/KeyK") as GameObject;
        keyL = Resources.Load("Prefabs/KeyL") as GameObject;
        keySmc = Resources.Load("Prefabs/KeySmc") as GameObject;
    }
    public float moveSpeed = 50;
    public float maxTime;
    int index = 0;
    GameObject key;
    bool isStart;
    void Update()
    {
        float inputX = Input.GetAxis("Horizontal") * Time.deltaTime * rotateSpeed;
        direct = Quaternion.Euler(-Vector3.forward * inputX) * direct;
        //draw basic ray
        hitList = new List<RaycastHit>();
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 dir = direct;
        while (Physics.Raycast(origin, dir, out hit))
        {
            hitList.Add(hit);
            origin = hit.point;
            dir = Vector3.Reflect(dir, hit.normal);
        }
        //last one
        hit.point = origin + dir;
        hitList.Add(hit);
        //Draw Lines
        DrawLines();
        //Draw Beats
        maxTime = maxLength / moveSpeed;
        float beatTime = AudioManager.Instance.audioSource.time + maxTime;
        if (AudioManager.Instance.nextClip != null
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
            && beatTime > AudioManager.Instance.audioSource.clip.length + AudioManager.Instance.nextClip.length
            && beatTime <= AudioManager.Instance.audioSource.clip.length + maxTime)
        {
            //Debug.Log(2 + ":" + beatTime + ":" + (beatTime - AudioManager.Instance.audioSource.clip.length - AudioManager.Instance.nextClip.length));
            beatTime -= AudioManager.Instance.audioSource.clip.length + AudioManager.Instance.nextClip.length;
            if (beatTime <= AudioManager.Instance.nextNextBeatList[0].time) index = 0;
            instantiateKey(ref index, beatTime, AudioManager.Instance.nextNextBeatList);
        }
        else if (beatTime <= AudioManager.Instance.audioSource.clip.length)
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
            switch (beat.type)
            {
                case 0:
                    key = Instantiate(keyJ, getPointPos(distance), Quaternion.identity) as GameObject;
                    break;
                case 1:
                    key = Instantiate(keyK, getPointPos(distance), Quaternion.identity) as GameObject;
                    break;
                case 2:
                    key = Instantiate(keyL, getPointPos(distance), Quaternion.identity) as GameObject;
                    break;
                case 3:
                    key = Instantiate(keySmc, getPointPos(distance), Quaternion.identity) as GameObject;
                    break;
            }
            // Debug.Log(index + "/" + list.Count + ":" + beat.time + "/" + beatTime + ":" + distance);
            key.GetComponent<BeatUnit>().distance = distance;
            index++;
        }
    }
    public Vector3 getPointPos(float distance)
    {
        Ray ray = new Ray(transform.position, direct);
        for (int i = 0; i < hitList.Count - 1; i++)
        {
            RaycastHit hit = hitList[i];
            float hitDistance = Vector3.Distance(hit.point, ray.origin);
            if (distance - hitDistance < 0)//less than the distance among the hitpoint
            {
                ray.direction = hit.point - ray.origin;
                return ray.GetPoint(distance);
            }
            distance -= hitDistance;
            ray.origin = hit.point;
        }
        //above the last hitpoint or no hitpoint exist
        ray.direction = hitList[hitList.Count - 1].point - ray.origin;
        return ray.GetPoint(distance);
    }
    void DrawLines()
    {
        List<Vector3> pointList = new List<Vector3>();
        try
        {
            float distance = maxLength;
            pointList.Add(transform.position);
            Ray ray = new Ray(transform.position, direct);
            for (int i = 0; i < hitList.Count - 1; i++)
            {
                RaycastHit hit = hitList[i];
                pointList.Add(hit.point);
                ray.direction = hit.point - ray.origin;
                float hitDistance = Vector3.Distance(hit.point, ray.origin);
                if (distance - hitDistance < 0)//less than the distance among the hitpoint
                {
                    pointList.Add(ray.origin + ray.direction * distance);
                    return;
                }
                pointList.Add(ray.origin + ray.direction * hitDistance);
                distance -= hitDistance;
                ray.origin = hit.point;
            }
            //above the last hitpoint or no hitpoint exist
            ray.direction = hitList[hitList.Count - 1].point - ray.origin;
            pointList.Add(ray.origin + ray.direction * distance);
        }
        catch { }
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
