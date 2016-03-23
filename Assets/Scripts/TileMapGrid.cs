using UnityEngine;
using UnityTileMap;

public class TileMapGrid : MonoBehaviour {

    public Color backgroundColor { get; set; }
    public Color gridColor = new Color(0f, 1f, 0f, 1f);
    public Color highlightColor = new Color(1f, 0f, 0f, 0.5f);
    public Color highlightColor1 = new Color(0f, 0f, 1f, 0.5f);
    public Color selectColor = new Color(1f, 0f, 0f, 0.9f);
    public Color selectColor1 = new Color(0f, 0f, 1f, 0.9f);
    public Color tileMapSelectColor = new Color(0f, 1f, 0f, 0.5f);

    internal bool showGrid = true;
    internal bool showSelection = true;
    private TileMapBehaviour m_tileMap;
    private TileMapGameBahaviour m_tileMapGame;
    public Material lineMaterial;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public void UpdateBackgroundColor()
    {
        cam.backgroundColor = backgroundColor;
    }

    public void ToggleGrid()
    {
        showGrid = !showGrid;
    }

    void Start()
    {
        m_tileMap = FindObjectOfType<TileMapBehaviour>();
        m_tileMapGame = FindObjectOfType<TileMapGameBahaviour>();
    }

    void OnPostRender()
    {
        float tileSize = m_tileMap.MeshSettings.TileSize;

        if (showGrid)
        {
            float gridWidth = m_tileMap.MeshSettings.TilesX * m_tileMap.MeshSettings.TileSize;
            float gridHeight = m_tileMap.MeshSettings.TilesY * m_tileMap.MeshSettings.TileSize;

            // set the current material
            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(gridColor);

            //Layers
            for (float j = 0; j <= gridHeight; j += tileSize)
            {
                 GL.Vertex3(0, j, 0);
                 GL.Vertex3(gridWidth, j, 0);               
            }

            for (float k = 0; k <= gridWidth; k += tileSize)
            {
                GL.Vertex3(k, 0, 0);
                GL.Vertex3(k, gridHeight, 0);
            }

            GL.End();
        }
        if (showSelection)
        {
            GL.Begin(GL.QUADS);
            if (Input.GetMouseButton(0))
            {
                if (m_tileMapGame.editMode == EditMode.Tiles)
                    GL.Color(selectColor);
                else
                    GL.Color(selectColor1);
            }
            else
            {
                if (m_tileMapGame.editMode == EditMode.Tiles)
                    GL.Color(highlightColor);
                else
                    GL.Color(highlightColor1);
            }

            int tileX = Mathf.Clamp(Mathf.FloorToInt(m_tileMapGame.m_mouseHitPos.x), 0, m_tileMap.MeshSettings.TilesX);
            int tileY = Mathf.Clamp(Mathf.FloorToInt(m_tileMapGame.m_mouseHitPos.y), 0, m_tileMap.MeshSettings.TilesY);

            GL.Vertex3(tileX, tileY, 0);
            GL.Vertex3(tileX, tileY + tileSize, 0);
            GL.Vertex3(tileX + tileSize, tileY + tileSize, 0);
            GL.Vertex3(tileX + tileSize, tileY, 0);

            GL.End();
            if (m_tileMapGame.lastMapTileSelected != null)
            {
                GL.Begin(GL.QUADS);
                GL.Color(tileMapSelectColor);

                int lastTileX = m_tileMapGame.lastMapTileSelected.x;
                int lastTileY = m_tileMapGame.lastMapTileSelected.y;

                GL.Vertex3(lastTileX, lastTileY, 0);
                GL.Vertex3(lastTileX, lastTileY + tileSize, 0);
                GL.Vertex3(lastTileX + tileSize, lastTileY + tileSize, 0);
                GL.Vertex3(lastTileX + tileSize, lastTileY, 0);

                GL.End();
            }
        }
    }
}
