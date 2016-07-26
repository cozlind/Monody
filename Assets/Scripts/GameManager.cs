using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour {

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
