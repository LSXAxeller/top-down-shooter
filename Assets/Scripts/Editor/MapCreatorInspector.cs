using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MapCreator))]
public class MapCreatorInspector : Editor
{
    int width = 50;
    int height = 50;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.BeginVertical();
        GUILayout.Label("Map size:");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Width: " + width);
        width = (int)GUILayout.HorizontalSlider(width, 1, 125);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Height: " + height);
        height = (int)GUILayout.HorizontalSlider(height, 1, 125);
        GUILayout.EndHorizontal();
        MapCreator map = (MapCreator)target;

        if (GUILayout.Button("Create empty map"))
        {
            map.GenerateGrid(width, height);
        }
        
        EditorGUI.DrawPreviewTexture(new Rect(25, 60, 100, 100), FindObjectOfType<MapImporter>().splitTextures[FindObjectOfType<MapCreator>().selectedTileIndex]);

        GUILayout.BeginHorizontal();
        for (int x = 0; x < 16; x++)
        {
            GUILayout.BeginVertical();
            for (int y = 0; y < 16; y++)
            {
                if (GUILayout.Button((1 + (y * 16 + x)).ToString()))
                {
                    SetSelectedTile(x, y);
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        
    }

    private void SetSelectedTile(int x, int y)
    {
        MapCreator map = FindObjectOfType<MapCreator>();
        map.selectedTile = new Vector2(x, y);
        map.selectedTileIndex = (y * 16) + x;
    }

    public void OnSceneGUI()
    {
        Vector2 guiPosition = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
        RaycastHit hit;
        Event e = Event.current;
        if (e.type == EventType.MouseDown || e.type == EventType.KeyDown)
        {
            if (e.button == 0 || e.keyCode == KeyCode.Space)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.GetComponent<MapImporter>() != null)
                    {
                        Vector3 tile = hit.transform.InverseTransformPoint(hit.point);
                        hit.collider.GetComponent<MapImporter>().SetTile(new Vector2(Mathf.Round(tile.x - 0.5f), Mathf.Round(-tile.y - 0.5f)), FindObjectOfType<MapCreator>().selectedTile);
                        hit.collider.GetComponent<MapImporter>().Flush();
                    }
                }
            }
        }
    }
}
