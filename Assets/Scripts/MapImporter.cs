using System.IO;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MapImporter : MonoBehaviour
{
    public GameObject textObj;
    public Material tileMapMaterial;
    [HideInInspector]
    public int[,] sampleMap2D;
    public Texture2D textureMap;
    [HideInInspector]
    public Texture2D[] splitTextures;
    public int width, height;
    Vector2[] offsets;
    private Vector2[] tileMap = new Vector2[256];

    public void Split()
    {
        Split(textureMap, 32, 32);

    }

    [HideInInspector] public List<Vector2> collisions = new List<Vector2>();
    [HideInInspector] public List<Vector2> spawns = new List<Vector2>();
    [HideInInspector] public List<Vector2> waypoints = new List<Vector2>();
    [HideInInspector] public Dictionary<Vector2, Vector2> tiles;

    private Mesh mesh;
    [HideInInspector]
    public Vector2[,] tileMapping;

    public string importLocation;
    public string saveLocation;

    public void Start()
    {
        Import();
    }

    public void Import()
    {
        string pathToGame = Application.streamingAssetsPath;
        LoadMapTextFile(pathToGame+importLocation);
        //StartCoroutine(LoadTileset(pathToGame+"/tilemap.png"));       
        
        for (int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                SetTile(pos, tiles[pos]);
            }
        }
        Flush();      
    }

    enum Section
    {
        Null,
        Tiles,
        Collisions,
        Spawns,
        Waypoints
    }

    private IEnumerator LoadTileset(string filePath)
    {
        string directory = "file://" + filePath;
        WWW www = new WWW(directory);
        yield return www;
        textureMap = www.texture;
    }

    public void LoadMapTextFile(string filePath)
    {
        collisions = new List<Vector2>();
        spawns = new List<Vector2>();
        waypoints = new List<Vector2>();
        tiles = new Dictionary<Vector2, Vector2>();
        String input = File.ReadAllText(filePath);       
        Section section = Section.Null;

        int i = 0, j = 0;
        foreach (string row in input.Split('\n'))
        {
            if (row.StartsWith("tiles")) section = Section.Tiles;
            else if (row.StartsWith("collisions")) section = Section.Collisions;
            else if (row.StartsWith("spawns")) section = Section.Spawns;
            else if (row.StartsWith("waypoints")) section = Section.Waypoints;

            switch (section) {
                case Section.Tiles:
                    if (row.StartsWith("tiles")) break;                 
                    j = 0;
                    foreach (string col in row.Split(';'))
                    {
                        string[] val = col.Split(',');
                        tiles.Add(new Vector2(i, j), new Vector2(int.Parse(val[0]), int.Parse(val[1])));
                    }
                    i++;
                    break;
                case Section.Collisions:
                    if (row.StartsWith("collisions")) break;
                    foreach (string col in row.Split(';'))
                    {
                        string[] val = col.Split(',');
                        collisions.Add(new Vector2(int.Parse(val[0]), int.Parse(val[1])));
                    }
                    break;
                case Section.Spawns:
                    if (row.StartsWith("spawns")) break;
                    foreach (string col in row.Split(';'))
                    {
                        string[] val = col.Split(',');
                        spawns.Add(new Vector2(int.Parse(val[0]), int.Parse(val[1])));
                    }
                    break;
                case Section.Waypoints:
                    if (row.StartsWith("waypoints")) break;
                    foreach (string col in row.Split(';'))
                    {
                        string[] val = col.Split(',');
                        waypoints.Add(new Vector2(int.Parse(val[0]), int.Parse(val[1])));
                    }
                    break;
            }
        }       
    }

    public void SaveMapTextFile(string filePath)
    {
        Section section = Section.Tiles;
        List<string> lines = new List<string>();
        switch (section)
        {
            case Section.Tiles:
                lines.Add("tiles");

                for (int row = 0; row < height; row++)
                {
                    string line = string.Empty;
                    for (int col = 0; col < width; col++)
                    {
                        Vector2 tile = GetTile(new Vector2(col, row));
                        line += tile.x.ToString();
                        line += ",";
                        line += tile.y.ToString();
                        if (col < width-1)
                            line += ";";
                    }
                    Debug.Log(line);
                    lines.Add(line);
                    
                }
                //section = Section.Collisions;
                break;
            case Section.Collisions:

                break;
            case Section.Spawns:

                break;
            case Section.Waypoints:

                break;

        }
        File.Delete(filePath);
        File.WriteAllLines(filePath, lines.ToArray());
    }

    public void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Map";
        Vector3[] vertices = new Vector3[width * height * 4];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        tileMap = new Vector2[vertices.Length];

        int[] triangles = new int[vertices.Length * 6];

        float x, y;
        for (y = 0; y < height; y++)
        {
            for (x = 0; x < width; x++)
            {
                int squareVal = (int)((y * width + x) * 4);

                vertices[squareVal + 0] = new Vector3(x, -y, 0);
                vertices[squareVal + 1] = new Vector3(x + 1, -y, 0);
                vertices[squareVal + 2] = new Vector3(x, -y - 1, 0);
                vertices[squareVal + 3] = new Vector3(x + 1, -y - 1, 0);

                normals[squareVal + 0] = -Vector3.forward;
                normals[squareVal + 1] = -Vector3.forward;
                normals[squareVal + 2] = -Vector3.forward;
                normals[squareVal + 3] = -Vector3.forward;

                uv[squareVal + 0] = new Vector2((x + 0f) / width, 1f - (y + 0f) / height);
                uv[squareVal + 1] = new Vector2((x + 1f) / width, 1f - (y + 0f) / height);
                uv[squareVal + 2] = new Vector2((x + 0f) / width, 1f - (y + 1f) / height);
                uv[squareVal + 3] = new Vector2((x + 1f) / width, 1f - (y + 1f) / height);

                tileMap[squareVal + 0] = Vector2.zero;
                tileMap[squareVal + 1] = Vector2.zero;
                tileMap[squareVal + 2] = Vector2.zero;
                tileMap[squareVal + 3] = Vector2.zero;

                int triangleVal = squareVal * 6;

                triangles[triangleVal + 0] = squareVal + 0;
                triangles[triangleVal + 1] = squareVal + 1;
                triangles[triangleVal + 2] = squareVal + 2;

                triangles[triangleVal + 3] = squareVal + 1;
                triangles[triangleVal + 4] = squareVal + 3;
                triangles[triangleVal + 5] = squareVal + 2;
            }
        }

        //Populate mesh with the data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.uv2 = tileMap;

        if (gameObject.GetComponent<MeshCollider>() != null)
            GetComponent<MeshCollider>().sharedMesh = mesh;
        else
            gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;

        for (int i = 0; i < tileMap.Length; i++)
            Debug.Log(tileMap[i]);

        tileMapMaterial.SetVector("_MapSize", new Vector4(width, height));
    }

    public void SetTile(Vector2 tile, Vector2 sprite)
    {
        int squareVal = (int)((tile.y * 16 + tile.x) * 4);

            tileMap[squareVal + 0] = sprite * 0.0625f;
            tileMap[squareVal + 1] = sprite * 0.0625f;
            tileMap[squareVal + 2] = sprite * 0.0625f;
            tileMap[squareVal + 3] = sprite * 0.0625f;

    }

    public Vector2 GetTile(Vector2 tile)
    {
        int squareVal = (int)((tile.y * 16 + tile.x) * 4);
        return tileMap[squareVal] / (0.0625f);
    }

    public void Flush()
    {
        mesh.uv2 = tileMap;
    }


    public void Split(Texture2D image, int width, int height)
    {
        Texture2D[] list = new Texture2D[256];
        bool perfectWidth = image.width % width == 0;
        bool perfectHeight = image.height % height == 0;

        int lastWidth = width;
        if (!perfectWidth)
        {
            lastWidth = image.width - ((image.width / width) * width);
        }

        int lastHeight = height;
        if (!perfectHeight)
        {
            lastHeight = image.height - ((image.height / height) * height);
        }

        int rows = image.width / width + (perfectWidth ? 0 : 1);
        int cols = image.height / height + (perfectHeight ? 0 : 1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int tileWidth = i == rows - 1 ? lastWidth : width;
                int tileHeight = j == cols - 1 ? lastHeight : height;

                list[(15-i) * rows + j] = CropTexture(new Rect(j * width, i * height, tileWidth, tileHeight));
            }
        }
        splitTextures = list;
    }

    private Texture2D CropTexture(Rect cropRect)
    {
        // Make sure the crop rectangle stays within the original Texture dimensions
        cropRect.x = Mathf.Clamp(cropRect.x, 0, this.textureMap.width);
        cropRect.width = Mathf.Clamp(cropRect.width, 0, textureMap.width - cropRect.x);
        cropRect.y = Mathf.Clamp(cropRect.y, 0, textureMap.height);
        cropRect.height = Mathf.Clamp(cropRect.height, 0, textureMap.height - cropRect.y);
        if (cropRect.height <= 0 || cropRect.width <= 0) return null; // dont create a Texture with size 0

        Texture2D newTexture = new Texture2D((int)cropRect.width, (int)cropRect.height, TextureFormat.RGBA32, false);
        Color[] pixels = textureMap.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height, 0);
        newTexture.SetPixels(pixels);
        newTexture.Apply();
        return newTexture;
    }
}