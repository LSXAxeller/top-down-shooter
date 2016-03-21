using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AutoSaveScene : EditorWindow
{
    [MenuItem("Tools/Auto Save")]
    public static void OpenAutoSave()
    {
        GetWindow<AutoSaveScene>().Show();
        SetWindowMinSize();
    }

    private static void SetWindowMinSize()
    {
        GetWindow<AutoSaveScene>().minSize = new Vector2(100, 5);
    }

    private const string TimeTillSaveLabelFormat = "Time Till Save {0}";

    private const string IntervalPrefKey = "AUTO_SAVE_INTERVAL";
    private const float IntervalDefaultValue = 60f;
    private const string IntervalLabel = "Auto Save Interval";

    private const string SaveAssetsPrefKey = "AUTO_SAVE_SAVE_ASSETS";
    private const bool SaveAssetsDefaultValue = true;
    private const string SaveAssetsLabel = "Save Assets";

    private const string SaveScenesPrefKey = "AUTO_SAVE_SAVE_SCENES";
    private const bool SaveScenesDefaultValue = true;
    private const string SaveScenesLabel = "Save Scenes";

    private const string PlayModePrefKey = "AUTO_SAVE_PLAYMODE";
    private const bool PlayModeDefaultValue = false;
    private const string PlayModeLabel = "Save in play mode (Note: Scenes Will not be saved)";

    private const string OnPrefKey = "AUTO_SAVE_ON";
    private const bool OnDefaultValue = true;
    private const string OnLabel = "Auto Save On";

    private const string TimeRemainingNoPlayMode = "N/A";

    private static float _lastFullTime;
    private static float _currentTime;

    private float _interval
    {
        get
        {
            return EditorPrefs.GetFloat(IntervalPrefKey, IntervalDefaultValue);
        }
        set
        {
            EditorPrefs.SetFloat(IntervalPrefKey, value);
        }
    }

    private bool _saveScenes
    {
        get
        {
            return EditorPrefs.GetBool(SaveScenesPrefKey, SaveScenesDefaultValue);
        }
        set
        {
            EditorPrefs.SetBool(SaveScenesPrefKey, value);
        }
    }

    private bool _saveAssets
    {
        get
        {
            return EditorPrefs.GetBool(SaveAssetsPrefKey, SaveAssetsDefaultValue);
        }
        set
        {
            EditorPrefs.SetBool(SaveAssetsPrefKey, value);
        }
    }

    private bool _on
    {
        get
        {
            return EditorPrefs.GetBool(OnPrefKey, OnDefaultValue);
        }
        set
        {
            EditorPrefs.SetBool(OnPrefKey, value);
        }
    }

    private string _timeRemaining
    {
        get
        {
            if (((EditorApplication.isPlaying && _saveInPlaymode)
                || !EditorApplication.isPlaying) && _on)
            {
                return (_interval - _currentTime)
                    .ToString("F2")
                    .Replace(".", ":");
            }
            return TimeRemainingNoPlayMode;
        }
    }

    private bool _saveInPlaymode
    {
        get
        {
            return EditorPrefs.GetBool(PlayModePrefKey, PlayModeDefaultValue);
        }
        set
        {
            EditorPrefs.SetBool(PlayModePrefKey, value);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(string.Format(TimeTillSaveLabelFormat, _timeRemaining));

        _interval = EditorGUILayout.FloatField(IntervalLabel, _interval);
        _on = EditorGUILayout.ToggleLeft(OnLabel, _on);
        _saveInPlaymode = EditorGUILayout.ToggleLeft(PlayModeLabel, _saveInPlaymode);
        _saveScenes = EditorGUILayout.ToggleLeft(SaveScenesLabel, _saveScenes);
        _saveAssets = EditorGUILayout.ToggleLeft(SaveAssetsLabel, _saveAssets);
    }

    private void Update()
    {
        if (!_on) return;
        if (!_saveInPlaymode && EditorApplication.isPlaying) return;

        IncressTime();
        SaveIfTimeElasped();
        Repaint();
    }

    private void SaveIfTimeElasped()
    {
        if (_currentTime >= _interval)
        {
            SaveScenes();
            SaveAssets();
            ResetTime();
        }
    }

    private void SaveScenes()
    {
        if (_saveScenes && !EditorApplication.isPlaying)
        {
            EditorSceneManager.SaveOpenScenes();
        }
    }

    private void SaveAssets()
    {
        if (_saveAssets)
        {
            EditorApplication.SaveAssets();
        }
    }

    private void ResetTime()
    {
        _currentTime = 0;
    }

    private void IncressTime()
    {
        _currentTime += Time.realtimeSinceStartup - _lastFullTime;
        _lastFullTime = Time.realtimeSinceStartup;
    }
}
