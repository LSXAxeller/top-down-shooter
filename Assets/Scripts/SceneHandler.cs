#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEditor.SceneManagement;

public class SceneHandler : EditorWindow
{
    [MenuItem("Tools/Scene Handler")]
    public static void Open()
    {
        GetWindow<SceneHandler>().Show();
    }

    private const string AscendingPrefKey = "SCENE_HANDLER_ASCENDING";
    private bool _ascending
    {
        get
        {
            return EditorPrefs.GetBool(AscendingPrefKey, true);
        }
        set
        {
            EditorPrefs.SetBool(AscendingPrefKey, value);
        }
    }
    private Vector2 _scroll;
    private string _search;

    private void OnGUI()
    {
        RenderButton("New Scene", NewScene);
        _search = EditorGUILayout.TextField("Search", _search);
        _ascending = EditorGUILayout.ToggleLeft("Ascending", _ascending);
        RenderScrollView(() =>
        {
            RenderScenes();
        });
    }

    private void RenderScrollView(Action renderAction)
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        {
            renderAction();
        }
        EditorGUILayout.EndScrollView();
    }

    private void RenderScenes()
    {
        foreach (var scenePath in GetScenePaths())
        {
            RenderSceneRow(scenePath);
        }
    }

    private void RenderSceneRow(string scenePath)
    {
        RenderRow(() =>
        {
            EditorGUILayout.LabelField(Path.GetFileNameWithoutExtension(scenePath));
            RenderButton("Open", () => OpenScene(scenePath));
            RenderButton("Add", () => AddScene(scenePath));
        });
    }

    private void RenderRow(Action renderAction)
    {
        GUILayout.BeginHorizontal(GUI.skin.box);
        {
            renderAction();
        }
        GUILayout.EndHorizontal();
    }

    private void RenderButton(string label, Action onClick)
    {
        if (GUILayout.Button(label))
        {
            onClick();
        }
    }

    private void OpenScene(string scenePath)
    {
        EditorSceneManager.OpenScene(scenePath);
    }

    private void AddScene(string scenePath)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
    }

    private void NewScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
    }

    private IEnumerable<string> Sort(IEnumerable<string> sceneList)
    {
        if (_ascending)
            return sceneList.OrderBy(s => Path.GetFileNameWithoutExtension(s));
        else
            return sceneList.OrderByDescending(s => Path.GetFileNameWithoutExtension(s));
    }

    private IEnumerable<string> Search(IEnumerable<string> sceneList)
    {
        if (!string.IsNullOrEmpty(_search))
            return sceneList.Where(scene =>
                Path.GetFileNameWithoutExtension(scene)
                    .ToLower()
                    .Contains(_search.ToLower()));
        else
            return sceneList;
    }

    private IEnumerable<string> GetScenePaths()
    {
        var sceneList = AssetDatabase.FindAssets("t:Scene")
            .Select(assetGuid => AssetDatabase.GUIDToAssetPath(assetGuid))
            .ToList();
        return Sort(Search(sceneList));
    }
}
#endif