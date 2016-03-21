using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MapImporter))]
public class MapImporterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapImporter map = (MapImporter)target;

        if (GUILayout.Button("Import Map"))
        {
            map.Import();
        }
        if (GUILayout.Button("Split Map"))
        {
            map.Split ();
        }
        if (GUILayout.Button("Save Map"))
        {
            map.SaveMapTextFile(Application.dataPath + map.saveLocation);
            AssetDatabase.Refresh();
        }
    }
}
