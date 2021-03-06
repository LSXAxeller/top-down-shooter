﻿using UnityEngine;
using UnityTileMap;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(TileMapBehaviour))]
public class TileMapGameBahaviour : MonoBehaviour {
    
    public GameObject playerPrefab, cameraPrefab;
    public Text TileDetailText;
    public InputField MapNameInput;
    public CanvasGroup MainCanvas;
    public GameObject MainPanel;
    public GameObject TileGrid;
    public GameObject EntityGrid;
    public GameObject GridItem;
    public GameObject TilePropertiesList;
    public GameObject TilePropertiesItem;
    public UnityEngine.UI.Extensions.ScrollSnap TilePropertiesScrollSnap;
    public Dropdown TilePropertiesDropdown;
    public Text TilePropertiesInfoText;
    public GameObject ColorPicker;
    public Slider TilesXSlider;
    public Slider TilesYSlider;
    public EditMode editMode = EditMode.Tiles;
    public Vector2Int lastMapTileSelected;

    private const float FLOAT_PICKER_MARGIN = 24f;
    private const float FLOAT_ITEM_MARGIN = 6f;
    private TileMapBehaviour m_tileMap;
    private TileMapGrid m_tileMapGrid;
    private TileMeshSettings m_tileMeshSettings;
    private int m_setTileID = 0;
    internal Vector3 m_mouseHitPos = Vector3.zero;
    private TileSheet m_tileSheet;
    private int[] m_tileSheetIds;
    private Texture2D m_tempTexture;
    private int ids;
    private int[] idArray;
    internal string replyText = "Hello";
    private GameObject lastSelectedTile;
    private bool isPlaying = false;
    private Camera m_cam;
    private int m_setEntityID = 0;
    private bool isHidden;
    private bool isMovingPanel;

    private void Start()
    {
        m_cam = GameObject.FindGameObjectWithTag("MapEditorCamera").GetComponent<Camera>();
        m_tileMapGrid = FindObjectOfType<TileMapGrid>();
        m_tileMap = FindObjectOfType<TileMapBehaviour>();
        m_tileSheet = m_tileMap.TileSheet;
        m_tempTexture = new Texture2D(64, 64);
        m_tileSheetIds = m_tileSheet.Ids.ToArray();
        m_tileMeshSettings = m_tileMap.MeshSettings;
        
        m_tileMap.GetComponentInChildren<TileMeshBehaviour>().transform.localPosition = Vector3.zero;

        RefreshTiles();

        StartCoroutine(IUpdateMouseHit());

       // FormatMap();
    }
    
    /// <summary>
    /// All the processes that include starting playmode
    /// - Hide the Canvas
    /// - Setup main character + camera
    /// - Setting up collisions
    /// - Instantiation of entities
    /// </summary>
    public void StartPlayMode()
    {
        isPlaying = true;

        MainCanvas.alpha = 0f;
        MainCanvas.interactable = false;
        MainCanvas.blocksRaycasts = false;
        TilePropertiesItem.SetActive(false);;
        MainCanvas.interactable = false;
        MainCanvas.blocksRaycasts = false;
        m_tileMapGrid.showSelection = false;
        m_tileMapGrid.showGrid = false;
        m_cam.gameObject.SetActive(false);


        if (GameObject.FindGameObjectWithTag("MainCamera") == null)
        {
            var camera = Instantiate(cameraPrefab);
            camera.GetComponent<Camera>().backgroundColor = m_tileMapGrid.backgroundColor;
        }
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            if (lastMapTileSelected != null)
            {
                Instantiate(playerPrefab, lastMapTileSelected.ToVector2(), Quaternion.identity);
            }
            else
            {
                Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            }
        }

        foreach (Deftly.Subject subject in FindObjectsOfType<Deftly.Subject>())
        {
            subject.GetComponent<Rigidbody2D>().isKinematic = false;
            subject.SetInputPermission(true, true, true);
        }

        m_tileMap.DestroyColliders();
        m_tileMap.CreateColliders();

        m_tileMap.CreateEntities();
    }

    /// <summary>
    /// All the processes that include starting paused playmode
    /// - Show the Canvas
    /// - Freeze all players and ai
    /// </summary>
    public void PausePlayMode()
    {
        isPlaying = false;
        m_cam.gameObject.SetActive(true);
        MainCanvas.alpha = 1f;
        MainCanvas.interactable = true;
        MainCanvas.blocksRaycasts = true;
        m_tileMapGrid.showSelection = true;
        m_tileMapGrid.showGrid = true;
        foreach(GameObject entity in GameObject.FindGameObjectsWithTag("Entity_Active"))
        {
            entity.GetComponent<Rigidbody2D>().isKinematic = true;
            entity.GetComponent<Deftly.Subject>().SetInputPermission(false, false, false);
        }
    }

    /// <summary>
    /// All the processes that include stopping playmode
    /// - Show the editing Canvas
    /// - Destruction of all players
    /// - Destruction of all colliders
    /// - Destruction of all entities
    /// - Instantiation of entities in editmode/preview mode
    /// </summary>
    public void StopPlayMode()
    {
        isPlaying = false;
        m_cam.gameObject.SetActive(true);
        MainCanvas.alpha = 1f;
        MainCanvas.interactable = true;
        MainCanvas.blocksRaycasts = true;
        m_tileMapGrid.showSelection = true;
        m_tileMapGrid.showGrid = true;
        Destroy(FindObjectOfType<Deftly.DeftlyCamera>().gameObject);
        Destroy(FindObjectOfType<Deftly.PlayerController>().gameObject);
        foreach(Deftly.Subject subject in FindObjectsOfType<Deftly.Subject>())
        {
            Destroy(subject.gameObject);
        }
        m_tileMap.DestroyColliders();
        m_tileMap.DestroyEntities();
        m_tileMap.DisplayEntities();
    }

    /// <summary>
    /// Every input key should go through this method first.
    /// Input depend on Editmode enum Tiles/Entities
    /// </summary>
    private void HandleMouseEvents()
    {
        switch (editMode)
        {
            case EditMode.Tiles:
                // Do not process if no tile is selected
                if (m_setTileID <= 0)
                {
                    return;
                }

                if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
                {
                    if (UpdateMouseHit())
                    {
                        Vector2Int p1 = new Vector2Int(Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y));
                        
                        StartCoroutine(SelectWhileDragging(p1));                        
                    }
                }

                else if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
                {
                    if (UpdateMouseHit())
                    {
                        Vector2Int p1 = new Vector2Int(Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y));

                        Fill(p1.x, p1.y, m_setTileID);
                    }
                }

                else if (Input.GetMouseButton(0))
                {
                    if (UpdateMouseHit())
                    {
                        int tileX = Mathf.FloorToInt(m_mouseHitPos.x);
                        int tileY = Mathf.FloorToInt(m_mouseHitPos.y);

                        m_tileMap[tileX, tileY] = m_setTileID;
                    }
                }

                else if (Input.GetMouseButtonDown(1))
                {
                    if (UpdateMouseHit())
                    {
                        Vector2Int tileSelected = new Vector2Int(Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y));
                        if (lastMapTileSelected != tileSelected)
                        {
                            lastMapTileSelected = tileSelected;
                        }
                        else
                        {
                            lastMapTileSelected = null;
                        }
                    }
                }
                break;

            case EditMode.Entities:
                if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButton(0))
                {
                    if (UpdateMouseHit())
                    {
                        Vector2Int pos = new Vector2Int(Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y));

                        if (m_setEntityID == (int)EntitiesData.EntityID.Tool_Eraser)
                        {
                            m_tileMap.RemoveEntity(pos);
                        }
                        else
                        {
                            m_tileMap.AddEntity(pos, (EntitiesData.EntityID)Mathf.Abs(m_setEntityID));

                            GameObject entity = Instantiate(m_tileMap.entities[m_setEntityID]);
                            entity.tag = "Entity";
                            entity.transform.position = pos.ToTileVector2();
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    if (UpdateMouseHit())
                    {
                        Vector2Int tileSelected = new Vector2Int(Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y));
                        if (lastMapTileSelected != tileSelected)
                        {
                            lastMapTileSelected = tileSelected;
                        }
                        else
                        {
                            lastMapTileSelected = null;
                        }
                    }
                }
                break;
        }
    }

    private IEnumerator SelectWhileDragging(Vector2Int p1)
    {
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));

        if (UpdateMouseHit())
        {
            Vector2Int p2 = new Vector2Int(Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y));

            foreach (Vector2Int pos in SelectionBox(p1, p2))
            {
                m_tileMap[pos.x, pos.y] = m_setTileID;
            }
        }
    }

    private List<Vector2Int> SelectionBox(Vector2Int p1, Vector2Int p2)
    {
        int lowestX = (p2.x > p1.x) ? p1.x : p2.x;
        int lowestY = (p2.y > p1.y) ? p1.y : p2.y;

        int highestX = (p2.x > p1.x) ? p2.x : p1.x;
        int highestY = (p2.y > p1.y) ? p2.y : p1.y;

        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int i = lowestX; i <= highestX; i++)
        {
            for (int j = lowestY; j <= highestY; j++)
            {
                tiles.Add(new Vector2Int(i, j));
            }
        }

        return tiles;
    }

    private void FloodFill(Vector2Int tile, int targetId, int replaceId)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        if (m_tileMap[tile.x, tile.x] != targetId)
            return;

        q.Enqueue(tile);
        while (q.Count > 0)
        {
            Vector2Int n = q.Dequeue();
            if (m_tileMap[n.x, n.y] == targetId)
            {
                Vector2Int e = n;
                Vector2Int w = n;
                while ((w.x != 0) && (m_tileMap[w.x, w.y] == targetId))
                {
                    m_tileMap[w.x, w.y] = replaceId;
                    w = new Vector2Int(w.x - 1, w.y);
                }

                while ((e.x != m_tileMeshSettings.TilesX - 1) && (m_tileMap[e.x, e.y] == targetId))
                {
                    m_tileMap[e.x, e.y] = replaceId;
                    e = new Vector2Int(e.x + 1, e.y);
                }

                for (int i = w.x; i <= e.x; i++)
                {
                    Vector2Int x = new Vector2Int(i, e.y);
                    if (e.y + 1 != m_tileMeshSettings.TilesY - 1)
                    {
                        if (m_tileMap[x.x, x.y + 1] == targetId)
                            q.Enqueue(new Vector2Int(x.x, x.y + 1));
                    }
                    if (e.y - 1 != -1)
                    {
                        if (m_tileMap[x.x, x.y - 1] == targetId)
                            q.Enqueue(new Vector2Int(x.x, x.y - 1));
                    }
                }
            }
        }
    }

    public void Fill(int x, int y, int newInt)
    {
        int initial = m_tileMap[x, y];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(x, y));

        while (queue.Any())
        {
            Vector2Int point = queue.Dequeue();

            if (m_tileMap[point.x, point.y] != initial)
                continue;

            m_tileMap[point.x, point.y] = newInt;

            EnqueueIfMatches(queue, point.x - 1, point.y, initial);
            EnqueueIfMatches(queue, point.x + 1, point.y, initial);
            EnqueueIfMatches(queue, point.x, point.y - 1, initial);
            EnqueueIfMatches(queue, point.x, point.y + 1, initial);
        }
    }

    private void EnqueueIfMatches(Queue<Vector2Int> queue, int x, int y, int initial)
    {
        if (m_tileMap.IsInBounds(x, y) == false)
            return;

        if (m_tileMap[x, y] == initial)
            queue.Enqueue(new Vector2Int(x, y));
    }

    /// <summary>
    /// Returns true if the mouse is hovering above the map
    /// Updates the mouse hit position at m_mouseHitPos
    /// Handles a little GUI function to display tile info
    /// </summary>
    private bool UpdateMouseHit()
    {
        if (isPlaying) return false;

        Plane p = new Plane(m_tileMap.transform.TransformDirection(Vector3.forward), m_tileMap.transform.position);
        Ray ray = m_cam.ScreenPointToRay(Input.mousePosition);

        Vector3 hit = new Vector3();
        float distance;
        if (p.Raycast(ray, out distance))
            hit = ray.origin + (ray.direction.normalized * distance);

        m_mouseHitPos = m_tileMap.transform.InverseTransformPoint(hit);
        m_mouseHitPos = new Vector3(m_mouseHitPos.x, m_mouseHitPos.y, m_mouseHitPos.z);

        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (m_mouseHitPos.x >= 0 && m_mouseHitPos.x < m_tileMeshSettings.TilesX && m_mouseHitPos.y >= 0 && m_mouseHitPos.y < m_tileMeshSettings.TilesY)
            {
                TileDetailText.text = "Tile Position " + Mathf.FloorToInt(m_mouseHitPos.x) + "|" + Mathf.FloorToInt(m_mouseHitPos.y) + "  -  Tile #" + m_tileMap[Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y)];
                return true;
            }
        }
        return false;
    }

    public void AddTileGridItem(int id)
    {       
        GameObject g = Instantiate(GridItem);
        g.transform.SetParent(TileGrid.transform);
        g.transform.SetSiblingIndex(id);
        g.name = "Tile #" + id;

        g.GetComponent<Image>().sprite = m_tileSheet.Get(m_tileSheetIds[id]);
        
        g.GetComponent<Button>().onClick.AddListener(() => {
            editMode = EditMode.Tiles;
            m_setTileID = idArray[id];
            g.AddComponent<AlphaSinLoop>();
            if(lastSelectedTile != null) Destroy(lastSelectedTile.GetComponent<AlphaSinLoop>());
            lastSelectedTile = g;
        });
    }

    public void AddTilePropertiesItem(int id)
    {
        GameObject g = Instantiate(TilePropertiesItem);
        g.transform.SetParent(TilePropertiesList.transform);
        g.transform.SetSiblingIndex(id);
        g.name = "Tile #" + id+1;
        Image tile = g.GetComponent<Image>();
        tile.sprite = m_tileSheet.Get(m_tileSheet.Ids.ToArray()[id]);
        Button button = g.GetComponent<Button>();
        button.onClick.AddListener(() => {
            TilePropertiesDropdown.value = (int)m_tileMap.GetTileProperty(id+1);
            UpdateTilePropertiesInfo(id+1);
        });
    }

    /// <summary>
    /// left: -1
    /// right: 1
    /// </summary>
    /// <param name="direction"></param>
    public void UpdateTileProperty(int direction)
    {
        int id = TilePropertiesScrollSnap.CurrentPage() + 1;

        //TilePropertiesDropdown.value = (int)m_tileMap.GetTileProperty(id + direction);
        UpdateTilePropertiesInfo(id + direction);
    }

    public void SetTileProperty()
    {
        int id = TilePropertiesScrollSnap.CurrentPage() + 1;

        m_tileMap.SetTileProperty(id, (TileProperty)TilePropertiesDropdown.value);

        UpdateTilePropertiesInfo(id);
    }

    public void UpdateTilePropertiesInfo(int id)
    {
        TilePropertiesInfoText.text = string.Format("Tile#{0}, {1}", id, m_tileMap.GetTileProperty(id).ToString());
    }
    
    private void Update()
    {
        HandleMouseEvents();
    }
    
    /// <summary>
    /// To be invoked by a BG Color Button
    /// </summary>
    public void ToggleColorPicker()
    {
        ColorPicker.SetActive(!ColorPicker.activeSelf);
    }
    
    /// <summary>
    /// To be invoked by a Refresh Button or when Tab Switching
    /// </summary>
    public void RefreshMapSize()
    {
        m_tileMap.MeshSettings.TilesX = (int)TilesXSlider.value;
        m_tileMap.MeshSettings.TilesY = (int)TilesYSlider.value;
        m_tileMap.DestroyMesh();
        m_tileMap.CreateMesh();
    }

    /// <summary>
    /// To be invoked by a Refresh Button only
    /// </summary>
    public void RefreshTiles()
    {
        idArray = m_tileSheet.Ids.ToArray();
        ids = idArray.Length;
        Debug.Log(ids);
        if (TileGrid.transform.childCount != ids)
            StartCoroutine(IRefreshTiles());
    }

    private IEnumerator IRefreshTiles()
    {
        foreach (Transform child in TileGrid.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in TilePropertiesList.transform)
        {
            Destroy(child.gameObject);
        }
        for (int id = 0; id < ids; id++)
        {
            AddTilePropertiesItem(id);
            AddTileGridItem(id);
            yield return new WaitForSeconds(0.005f);
        }
    }
    /// <summary>
    /// To be invoked by an Export Button only
    /// </summary>
    public void ExportMap()
    {
        if(!string.IsNullOrEmpty(MapNameInput.text))
            m_tileMap.ExportMap(MapNameInput.text);
    }

    public void FormatMap()
    {
        for (int x = 0; x < m_tileMeshSettings.TilesX; x++)
        {
            for (int y = 0; y < m_tileMeshSettings.TilesY; y++) 
            {
                m_tileMap.PaintTile(x, y, Color.black);
            }
        }
    }

    /// <summary>
    /// This function is invoked when an entity is picked from selection.
    /// </summary>
    public void SetInstanceId(int id)
    {
        editMode = EditMode.Entities;
        m_setEntityID = id;
    }

    /// <summary>
    /// OPTIMIMIZATION NEEDED!!!
    /// Invokes the UpdateMouseHit() method multiple times
    /// </summary>
    /// <returns></returns>
    private IEnumerator IUpdateMouseHit()
    {
        while(true)
        {
            UpdateMouseHit();
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void SlideSmoothly(float x)
    {
        if (isMovingPanel) return;
        if (isHidden)
        {
            StartCoroutine(IMoveSmoothly(new Vector2(MainPanel.transform.position.x + x, MainPanel.transform.position.y)));
            isHidden = false;
        }
        else
        {
            StartCoroutine(IMoveSmoothly(new Vector2(MainPanel.transform.position.x - x, MainPanel.transform.position.y)));
            isHidden = true;
        }
    }

    private IEnumerator IMoveSmoothly(Vector2 pos)
    {
        isMovingPanel = true;
        float t = 0;
        while (t <= 1f)
        {
            Vector2 initPos = MainPanel.transform.position;
            MainPanel.transform.position = Vector2.Lerp(initPos, pos, t);
            yield return t+=0.1f;
        }
        isMovingPanel = false;
    }
      
    #region Experimental
    public void ImportTexture (string filePath)
    {
        StartCoroutine(LoadTexture(filePath, m_tempTexture));
    }

    private IEnumerator LoadTexture(string path, Texture2D tex)
    {
        while (true)
        {
            var uri = new System.Uri(path);
            var www = new WWW(uri.AbsoluteUri);

            yield return www;

            www.LoadImageIntoTexture(tex);

            if (www.isDone)
            {
                TryTextureImport(tex);
                Debug.Log("DOwnloaded texture: " + tex.name);
            }
        }
    }

    private void TryTextureImport(object obj)
    {
        var texture = obj as Texture2D;
        var sprite = obj as Sprite;
        if (texture != null)
            ImportTexture(texture);
        else if (sprite != null)
            ImportSprite(sprite);
    }

    private void ImportTexture(Texture2D texture)
    {
        /*SetTextureImporterFormat(texture, true);
        var assetPath = AssetDatabase.GetAssetPath(texture);
        var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        var sprites = assets.OfType<Sprite>().ToList();
        if (sprites.Count > 0)
        {
            foreach (var sprite in sprites)
            {
                if (m_tileSheet.Contains(sprite.name))
                    continue;
                m_tileSheet.Add(sprite);
            }
            Debug.Log(string.Format("{0} sprites loaded from {1}", sprites.Count, assetPath));
        }
        else
        {
            Debug.LogWarning(string.Format("No sprites found on asset path: {0}", assetPath));
        }*/
    }

    private void ImportSprite(Sprite sprite)
    {
        SetTextureImporterFormat(sprite.texture, true);

        if (m_tileSheet.Contains(sprite.name))
            Debug.LogError(string.Format("TileSheet already contains a sprite named {0}", sprite.name));
        else
            m_tileSheet.Add(sprite);
    }

    public static void SetTextureImporterFormat(Texture2D texture, bool isReadable)
    {
        if (null == texture) return;

        /*string assetPath = AssetDatabase.GetAssetPath(texture);
        var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.textureType = TextureImporterType.Advanced;

            tImporter.isReadable = isReadable;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }*/
    }
    #endregion  
}

public enum EditMode { Tiles, Entities };

