using UnityEditor;
using UnityEditor.SceneManagement;

public static class UpgradeScenes
{
    private const string progressTitle = "Resaving Scenes";

    [MenuItem("Window/Resave All Scenes")]
    private static void ResaveAllScenes()
    {
        EditorUtility.DisplayProgressBar(progressTitle, "Finding all scenes", 0);

        var scenes = AssetDatabase.FindAssets("t:SceneAsset");

        for (int i = 0; i < scenes.Length; ++i)
        {
            float progress = (float)i / scenes.Length;
            string scenePath = AssetDatabase.GUIDToAssetPath(scenes[i]);

            if (EditorUtility.DisplayCancelableProgressBar(progressTitle, "Opening scene: " + scenePath, progress)) { break; }
            EditorSceneManager.OpenScene(scenePath);

            if (EditorUtility.DisplayCancelableProgressBar(progressTitle, "Saving scene: " + scenePath, progress)) { break; }
            EditorSceneManager.SaveOpenScenes();
        }

        EditorUtility.ClearProgressBar();
    }
}