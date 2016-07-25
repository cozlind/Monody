using UnityEngine;
using System.Collections;

public class BeatUnit : MonoBehaviour {

    public GameObject hitArea;
    public GameObject player;
    public float threshold = 0.22f;
    public float distance;
    bool isPerfect = false;
	void Start ()
    {
        hitArea = GameObject.Find("HitArea");
        player = GameObject.Find("Player");
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
                //click perfect
                isPerfect = true;
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
                gameObject.transform.localScale /= 2f;
                hitArea.transform.localScale *= 1.5f;

                //
                PlayerController.Instance.enableGravity = true;
                PlayerController.Instance.dash(BeatLine.Instance.hitList[0].point);

                 yield return new WaitForSeconds(0.1f);
                hitArea.transform.localScale /= 1.5f;
                break;
            }
            yield return null;
        }
        if (!isPerfect)
            PlayerController.Instance.enableGravity = false;
        Destroy(gameObject);
    }
	void Update ()
    {
        distance -= BeatLine.Instance.moveSpeed * Time.deltaTime;
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Pow(1 - distance / BeatLine.Instance.maxLength,2));
        if (!isPerfect)
        {
            Vector3 pos;
            BeatLine.Instance.getPointPos(distance,out pos);
            transform.position = pos;
        }
    }
}
