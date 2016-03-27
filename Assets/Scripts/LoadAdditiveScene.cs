using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadAdditiveScene : MonoBehaviour {

    public string SceneToLoad;
    public int delayTime; 

    void Awake()
    {
        StartCoroutine(WaitUntilLoad(delayTime));
    }

    IEnumerator WaitUntilLoad(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(Camera.main.gameObject);
        SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Additive);
    }
}
