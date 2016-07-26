using UnityEngine;
using System.Collections;

public class BeatUnit : MonoBehaviour {

    public GameObject hitArea;
    public GameObject player;
    public GameObject arrowPoint;
    public Object arrowSprite;
    public float threshold = 0.22f;
    public float distance;
    bool isPerfect = false;
	void Start ()
    {
        hitArea = PlayerController.Instance.hitArea;
        player = PlayerController.Instance.player;
        arrowPoint = PlayerController.Instance.arrowPoint;
        arrowSprite = PlayerController.Instance.arrowSprite;
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
                yield return PlayerController.Instance.clickPerfect(gameObject,key);
                Destroy(gameObject);
                yield break;
            }
            else if (Input.anyKeyDown)
            {
                PlayerController.Instance.clickFail();
            }
            yield return null;
        }
        PlayerController.Instance.clickFail();
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
