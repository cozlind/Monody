using UnityEngine;
using System.Collections;

public class DebugManager : MonoBehaviour {

    [SerializeField]
    public bool isDebug = true;
    private static DebugManager instance;
    public static DebugManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("DebugManager instance does not exist");
            }
            return instance;
        }
    }
    void Awake()
    {
        instance = this;
    }
}
