using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {


    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("UIManager instance does not exist");
            }
            return instance;
        }
    }
    void Awake()
    {
        instance = this;
        if (DebugManager.Instance.isDebug)
            resetPanel();
        else openPanel("Panel_Main");
    }
    #region panel
    [SerializeField]
    GameObject[] panelList;
    void resetPanel()
    {
        foreach(var panel in panelList)
        {
            panel.SetActive(false);
        }
    }
    void openPanel(string name)
    {
        resetPanel();
        foreach(var panel in panelList)
        {
            if (panel.name.Equals(name))
            {
                panel.SetActive(true);
                return;
            }
        }
        Debug.LogError("cannot find the panel:" + name);
    }
    #endregion
    #region audio debug
    public Text textDebug;
    
    #endregion
}
