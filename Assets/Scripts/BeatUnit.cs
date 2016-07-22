﻿using UnityEngine;
using System.Collections;

public class BeatUnit : MonoBehaviour {

    public GameObject hitArea;
    public float threshold = 0.22f;
    public float distance;
    bool isPerfect = false;
	void Start ()
    {
        hitArea = GameObject.Find("HitArea");
        StartCoroutine("destroySelf");
    } 
    IEnumerator destroySelf()
    {
        yield return new WaitForSeconds(distance / BeatLine.Instance.moveSpeed-threshold/2);
        KeyCode key = new KeyCode();
        switch (gameObject.name)
        {
            case "KeyJ(Clone)":
                key = KeyCode.J;
                break;
            case "KeyK(Clone)":
                key = KeyCode.K;
                break;
            case "KeyL(Clone)":
                key = KeyCode.L;
                break;
            case "KeySmc(Clone)":
                key = KeyCode.Semicolon;
                break;
        }
        float time = 0;
        while (time<=threshold)
        {
            time += Time.deltaTime;
            if (Input.GetKeyDown(key))
            {
                isPerfect = true;
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
                gameObject.transform.localScale /= 2f;
                hitArea.transform.localScale *= 1.5f;
                yield return new WaitForSeconds(0.1f);
                hitArea.transform.localScale /= 1.5f;
                break;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
	void Update ()
    {
        distance -= BeatLine.Instance.moveSpeed * Time.deltaTime;
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Pow(1 - distance / BeatLine.Instance.maxLength,2));
        if (!isPerfect)
        {
            transform.position = BeatLine.Instance.getPointPos(distance);
        }
    }
}
