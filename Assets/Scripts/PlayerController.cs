using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

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
    public GameObject hitArea;
    public GameObject bow;
    public GameObject shield;
    public GameObject initialShield;

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
    public float dashDuration = 0.5f;

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
    //Vector3 lastVelocity = Vector3.zero;
    //bool lastGrounded = false;
    //float jumpEndTime = 0;
    //bool jumpInterrupt = false;
    float forceCrouchEndTime;
    Quaternion flippedRotation = Quaternion.Euler(0, 180, 0);

    [Header("BeatUnit")]
    public GameObject energyRoot;
    public GameObject player;
    public GameObject arrowPoint;
    public Object arrowSprite;
    public int energy = 0;
    public bool isPrepare = false;
    public GameObject[] energyBar;
    public Color[] energyColor;

    enum PlayerState { move, attack, defense };
    PlayerState state = 0;

    void Awake()
    {
        instance = this;
        controller = GetComponent<CharacterController>();
        controller.detectCollisions = false;
        hitArea = GameObject.Find("HitArea");
        bow = GameObject.Find("Bow");
        shield = GameObject.Find("Shield");
        initialShield = GameObject.Find("InitialShield");
        resetTool();
        hitArea.GetComponent<SpriteRenderer>().enabled = true;

        player = gameObject;
        arrowPoint = GameObject.Find("ArrowPoint");
        arrowSprite = Resources.Load("Prefabs/Arrow");
        energyRoot = GameObject.Find("EnergyRoot");
        energyColor = new Color[energyBar.Length];
        for (int i = 0; i < energyBar.Length; i++)
        {
            energyColor[i] = energyBar[i].GetComponent<SpriteRenderer>().color;
        }
    }
    public Vector3 showControllerVelocity;
    void showEnergy()
    {
        if (energy == 0)
        {
            enableGravity = true;
            isPrepare = false;
        }
        //update the energyBar
        for (int i = 0; i < energyBar.Length; i++)
        {
            if (i < energy) energyBar[i].SetActive(true);
            else energyBar[i].SetActive(false);
            energyBar[i].transform.position = energyRoot.transform.position;
        }
    }
    void resetTool()
    {
        bow.GetComponent<SpriteRenderer>().enabled = false;
        shield.GetComponent<SpriteRenderer>().enabled = false;
        hitArea.GetComponent<SpriteRenderer>().enabled = false;
        initialShield.GetComponent<SpriteRenderer>().enabled = false;
    }
    void Update()
    {
        //control hitArea/bow/shield
        float x = Input.GetAxis(XAxis);
        hitArea.transform.RotateAround(transform.position, -Vector3.forward, x * Time.deltaTime * rotateSpeed);
        bow.transform.position = transform.position + hitArea.transform.right * 2.2f;
        bow.transform.rotation = Quaternion.LookRotation(bow.transform.forward, hitArea.transform.right);
        shield.transform.position = transform.position + hitArea.transform.right * 2.1f;
        shield.transform.rotation = Quaternion.LookRotation(bow.transform.forward, hitArea.transform.right);


        if (state == PlayerState.attack) showEnergy();
        if (Input.GetKey(KeyCode.W))//attack mode
        {
            if (!bow.GetComponent<SpriteRenderer>().enabled)
            {
                energy = 0;
                showEnergy();
                state = PlayerState.attack;
                resetTool();
                bow.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
        else if (Input.GetKey(KeyCode.S))//defense mode
        {
            if (!shield.GetComponent<SpriteRenderer>().enabled)
            {
                energy = 0;
                showEnergy();
                state = PlayerState.defense;
                resetTool();
                shield.GetComponent<SpriteRenderer>().enabled = true;
                initialShield.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
        else//move mode
        {
            energy = 0;
            showEnergy();
            if (!hitArea.GetComponent<SpriteRenderer>().enabled)
            {
                state = PlayerState.move;
                resetTool();
                hitArea.GetComponent<SpriteRenderer>().enabled = true;
            }
            //switch the static mode and dynamic mode of hitArea
            if (x == 0 && hitArea.transform.localPosition != Vector3.zero)
            {
                hitArea.transform.localPosition = Vector3.zero;
            }
            else if (x != 0 && hitArea.transform.localPosition == Vector3.zero)
            {
                hitArea.transform.localPosition = hitArea.transform.right * 2.2f;
            }
        }
        showControllerVelocity = controller.velocity;
        velocity.y = enableGravity ? -gravity : 0;
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
            iTween.MoveTo(gameObject, transform.position + dashDistance * (pos - transform.position).normalized, dashDuration);
        }
    }

    public IEnumerator clickPerfect(GameObject beatUnit, KeyCode key)
    {
        StartCoroutine(AudioManager.Instance.emphasizeVolume());
        //click perfect
        beatUnit.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        beatUnit.transform.localScale /= 2f;
        switch (state)
        {
            case PlayerState.move:
                {
                    hitArea.transform.localScale *= 1.5f;

                    //
                    enableGravity = false;
                    dash(BeatLine.Instance.hitList[0].point);

                    yield return new WaitForSeconds(0.1f);
                    hitArea.transform.localScale /= 1.5f;
                    break;
                }
            case PlayerState.defense:
                {
                    energy++;
                    Color color = shield.GetComponent<SpriteRenderer>().color;
                    shield.GetComponent<SpriteRenderer>().color=new Color(color.r,color.g,color.b, Mathf.Clamp01(energy / 5f));
                    break;
                }
            case PlayerState.attack:
                {
                    energy++;
                    if (energy > 5)
                    {
                        isPrepare = true;
                        GameObject arrow = Instantiate(arrowSprite, arrowPoint.transform.position, bow.transform.localRotation) as GameObject;

                        float strengthTime = 0;
                        while (Input.GetKey(key))
                        {
                            arrow.transform.position = arrowPoint.transform.position;
                            arrow.transform.rotation = arrowPoint.transform.rotation;
                            foreach (var energyone in energyBar)
                            {
                                Random.seed = Mathf.RoundToInt(Time.time * 1000);
                                energyone.transform.position = energyRoot.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                                energyone.GetComponent<SpriteRenderer>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), energyone.GetComponent<SpriteRenderer>().color.a);
                            }
                            Random.seed = Mathf.RoundToInt(Time.time * 1000);
                            arrow.transform.position = arrowPoint.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                            arrow.GetComponent<SpriteRenderer>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
                            yield return null;
                            strengthTime += Time.deltaTime;
                            if (Input.GetKeyUp(key))
                            {
                                arrow.GetComponent<Arrow>().trigger(strengthTime * 10);
                                energy = 0;
                                for (int i = 0; i < energyBar.Length; i++)
                                {
                                    energyBar[i].GetComponent<SpriteRenderer>().color = energyColor[i];
                                }
                                yield break;
                            }
                            if (strengthTime > 4)
                            {
                                arrow.GetComponent<Rigidbody>().velocity = arrow.transform.up * 40;
                                energy = 0;
                                for (int i = 0; i < energyBar.Length; i++)
                                {
                                    energyBar[i].GetComponent<SpriteRenderer>().color = energyColor[i];
                                }
                                yield break;
                            }
                        }
                    }
                    break;
                }
        }
    }

    public void clickFail()
    {
        if (isPrepare) return;
        energy = 0;
    }
}
