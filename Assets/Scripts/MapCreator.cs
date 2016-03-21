using UnityEngine;

[ExecuteInEditMode]
public class MapCreator : MonoBehaviour {

    private MapImporter map;
    public Vector2 selectedTile;
    public int selectedTileIndex = 0;

    void Start()
    {
        map = GetComponent<MapImporter>();
    }

    public void GenerateGrid(int width, int height)
    {
        map = FindObjectOfType<MapImporter>();
        map.width = width;
        map.height = height;
        map.Generate();
    }
}
