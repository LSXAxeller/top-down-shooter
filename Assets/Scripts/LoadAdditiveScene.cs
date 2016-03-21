using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadAdditiveScene : MonoBehaviour {
    
    public int delayTime; 

    void Awake()
    {
        StartCoroutine(WaitUntilLoad(delayTime));
    }

    IEnumerator WaitUntilLoad(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(1);
    }
}
