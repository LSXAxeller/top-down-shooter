using UnityEngine;
using UnityTileMap;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(TileMapBehaviour))]
public class TileMapGameBahaviour : MonoBehaviour {

    public GameObject[] entities;
    public GameObject playerPrefab, cameraPrefab;
    public Text TileDetailText;
    public InputField MapNameInput;
    public CanvasGroup MainCanvas;
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
    private int m_tilesX;
    private float m_tileSize;
    private int m_tilesY;
    private int m_setTileID = 0;
    internal Vector3 m_mouseHitPos = Vector3.zero;
    private TileSheet m_tileSheet;
    private Texture2D m_tempTexture;
    private int ids;
    private int[] idArray;
    internal string replyText = "Hello";
    private GameObject lastSelectedTile;
    private bool isPlaying = false;
    private Camera m_cam;
    private int m_setEntityID = 0;

    private void Start()
    {
        m_cam = GameObject.FindGameObjectWithTag("MapEditorCamera").GetComponent<Camera>();
        m_tileMapGrid = FindObjectOfType<TileMapGrid>();
        m_tileMap = FindObjectOfType<TileMapBehaviour>();
        m_tileSheet = m_tileMap.TileSheet;
        m_tempTexture = new Texture2D(64, 64);

        var meshSettings = m_tileMap.MeshSettings;

        if (meshSettings != null)
        {
            m_tilesX = meshSettings.TilesX;
            m_tilesY = meshSettings.TilesY;
            m_tileSize = meshSettings.TileSize;
        }
        m_tileMap.GetComponentInChildren<TileMeshBehaviour>().transform.localPosition = Vector3.zero;

        RefreshTiles();

        StartCoroutine(IUpdateMouseHit());
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
        DisplayEntities();
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
                if (m_setTileID < 0)
                {
                    return;
                }

                if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButton(0))
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
                        Vector2 pos = new Vector2(Mathf.FloorToInt(m_mouseHitPos.x), Mathf.FloorToInt(m_mouseHitPos.y));
                        
                        m_tileMap.AddEntity(pos, (EntitiesData.EntityID)Mathf.Abs(m_setEntityID));

                        GameObject entity = Instantiate(entities[m_setEntityID]);
                        entity.tag = "Entity";
                        entity.transform.position = pos;
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
        m_mouseHitPos = new Vector3(m_mouseHitPos.x * m_tileSize, m_mouseHitPos.y * m_tileSize, m_mouseHitPos.z);

        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (m_mouseHitPos.x >= 0 && m_mouseHitPos.x < m_tilesX * m_tileSize && m_mouseHitPos.y >= 0 && m_mouseHitPos.y < m_tilesY * m_tileSize)
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
        Image tile = g.GetComponent<Image>();
        tile.sprite = m_tileSheet.Get(m_tileSheet.Ids.ToArray()[id]);
        Button button = g.GetComponent<Button>();
        button.onClick.AddListener(() => {
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
        g.name = "Tile #" + id;
        Image tile = g.GetComponent<Image>();
        tile.sprite = m_tileSheet.Get(m_tileSheet.Ids.ToArray()[id]);
        Button button = g.GetComponent<Button>();
        button.onClick.AddListener(() => {

        });
    }

    public void DisplayEntities()
    {
        m_tileMap.DestroyEntities();

        foreach (KeyValuePair<Vector2, EntitiesData.EntityID> entry in m_tileMap.m_entities)
        {
            GameObject entity = Instantiate(entities[(int)entry.Value]);
            entity.tag = "Entity";
            entity.transform.position = entry.Key;
        }
    }

    public void RefreshTileProperties()
    {
        TilePropertiesScrollSnap.onPageChange += (int id) =>
        {
            TilePropertiesDropdown.value = (int)m_tileMap.GetTileProperty(id);
            m_tileMap.SetTileProperty(id, (TileProperty)TilePropertiesDropdown.value);
            TilePropertiesInfoText.text = string.Format("Tile#{0}, {1}", id, m_tileMap.GetTileProperty(id).ToString("D"));
        };
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

    /// <summary>
    /// This function is invoked when an entity is picked from selection.
    /// </summary>
    public void SetInstanceId(int id)
    {
        editMode = EditMode.Entities;
        if (id < 0)
        {
            m_setEntityID = 0;
        }
        else
        {
            m_setEntityID = id;
        }
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

