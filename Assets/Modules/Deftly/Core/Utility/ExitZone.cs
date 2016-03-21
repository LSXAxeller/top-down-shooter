using Deftly;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitZone : MonoBehaviour
{
    [Space]
    public LayerMask CanTriggerThis;
    public enum LoadLevelPath { Index, String, Restart };
    public LoadLevelPath LoadType;
    public float Delay;

    [Space]
    public int GoToLevelIndex;
    public string GoToLevelName;

    void OnTriggerEnter(Collider col)
    {
        if (!StaticUtil.LayerMatchTest(CanTriggerThis, col.gameObject)) return;

        switch (LoadType)
        {
            case (LoadLevelPath.Index):
                SceneManager.LoadScene(GoToLevelIndex);
                break;
            case (LoadLevelPath.String):
                SceneManager.LoadScene(GoToLevelName);
                break;
            case (LoadLevelPath.Restart):
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }
    }
}