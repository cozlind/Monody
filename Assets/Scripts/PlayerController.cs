using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    private static PlayerController instance;
    public static PlayerController Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("player controller instance is null");
            }
            return instance;
        }
    }
    [Header("Beating")]
    public float rotateSpeed = 200;
    GameObject hitArea;

    [Header("Controls")]
    public string XAxis = "Horizontal";
    public string YAxis = "Vertical";
    public string JumpButton = "Jump";

	[Header("Moving")]
    public float walkSpeed = 4;
    public float runSpeed = 10;
    public float gravity = 2;
    public bool enableGravity = true;
    public float dashDistance = 5;
    public float dashDuration =0.5f;

    [Header("Jumping")]
    public float jumpSpeed = 30;
    public float jumpDuration = 0.5f;
    public float jumpInterruptFactor = 100;
    public float forceCrouchVelocity = 25;
    public float forceCrouchDuration = 0.5f;

	[Header("Graphics")]
    public Transform graphicsRoot;

    public CharacterController controller;
    public Vector3 velocity = Vector3.zero;
    Vector3 lastVelocity = Vector3.zero;
    bool lastGrounded = false;
    float jumpEndTime = 0;
    bool jumpInterrupt = false;
    float forceCrouchEndTime;
    Quaternion flippedRotation = Quaternion.Euler(0, 180, 0);

    void Awake()
    {
        instance = this;
        controller = GetComponent<CharacterController>();
        controller.detectCollisions = false;
        hitArea = GameObject.Find("HitArea");
    }
    public Vector3 showControllerVelocity;
    public Vector3 hitAreaPos = new Vector3(2.2f, 0, 0);
    void Update()
    {
        //control inputs
        float x = Input.GetAxis(XAxis);
        hitArea.transform.RotateAround(transform.position, -Vector3.forward, x * Time.deltaTime * rotateSpeed);

        //control the localposition of hitarea to fit the fix line 
        if (x == 0&& hitArea.transform.localPosition!=Vector3.zero)
        {
            hitAreaPos = hitArea.transform.localPosition;
            hitArea.transform.localPosition = Vector3.zero;
        }
        else if(x != 0&&hitArea.transform.localPosition == Vector3.zero)
        {
            hitArea.transform.localPosition = hitAreaPos;
        }


        showControllerVelocity = controller.velocity;
        velocity.y = enableGravity? -gravity :0;
        velocity.y = -gravity;
        controller.Move(velocity * Time.deltaTime);

        //flip left or right
        if (x > 0)
            graphicsRoot.localRotation = Quaternion.identity;
        else if (x < 0)
            graphicsRoot.localRotation = flippedRotation;
    }
    public void dash(Vector3 pos)
    {
        if (Vector3.Distance(transform.position, pos) < dashDistance)
        {
            iTween.MoveTo(gameObject, pos, dashDuration);
            if (BeatLine.Instance.hitList.Count > 1)
                BeatLine.Instance.hitList.RemoveAt(0);
            else BeatLine.Instance.isUpdateHits = true;//update the hitlist
        }
        else
        {
            iTween.MoveTo(gameObject, transform.position+dashDistance * (pos-transform.position).normalized, dashDuration);
        }
    }
}
