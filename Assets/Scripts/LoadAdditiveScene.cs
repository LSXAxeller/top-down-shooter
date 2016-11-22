using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadAdditiveScene : MonoBehaviour {

    public string SceneToLoad;
    public int delayTime;
    public float moveHeight;
    public Transform Title;

    private float m_startTitlePosY;

    void Awake()
    {
        StartCoroutine(WaitUntilLoad(delayTime));

        if (Title == null)
        {
            Debug.LogError("Failed to initialize since there is no title reference specialized!");
            return;
        }

        m_startTitlePosY = Title.position.y;
    }

    IEnumerator WaitUntilLoad(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(Camera.main.gameObject);
        SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Additive);
        m_startTitlePosY = Title.position.y;
        StartCoroutine(SmoothMoveUp(moveHeight));
    }

    IEnumerator SmoothMoveUp(float height)
    {
        while (Title.position.y != m_startTitlePosY + height)
        {
            Title.position = new Vector3(Title.position.x, Mathf.Lerp(m_startTitlePosY, m_startTitlePosY + height, delayTime), Title.position.z);
            yield return true;
        }
    }
}
