using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

    bool isFly = false;
	public void trigger(float speed)
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = true;
        rigidbody.velocity = transform.up * speed;
        isFly = true;
        Invoke("DestroySelf", 2);
	}
    void DestroySelf()
    {
        Destroy(gameObject);
    }
	void Update()
    {
        if (isFly)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, GetComponent<Rigidbody>().velocity);
        }
    }
}
