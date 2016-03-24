using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnityTileMap
{

    public enum TileProperty
    {
        Null,
        Wall,
        Obstacle,
        Floor,
        Deadly
    }

    [ExecuteInEditMode]
    [Serializable]
    public class TileMapBehaviour : MonoBehaviour, IEnumerable<KeyValuePair<Vector2Int, int>>
    {
        public GameObject[] entities;

        [SerializeField]
        private TileMapData m_tileMapData;

        [SerializeField]
        private TileMeshSettings m_tileMeshSettings;

        [SerializeField]
        private TileSheet m_tileSheet;

        [SerializeField]
        internal Dictionary<Vector2Int, EntitiesData.EntityID> m_entities = new Dictionary<Vector2Int, EntitiesData.EntityID>();

        [SerializeField]
        private bool m_activeInEditMode = true;

        private TileChunkManager m_chunkManager;

        private TileMapVisibilityBehaviour m_visibility;
        private string assetsPath;

        /// <summary>
        /// When ActiveInEditMode the mesh for the tilemap will be created and rendered in edit mode.
        /// This is useful if you want to use the map editing GUI.
        /// A benefit to disabling it is a smaller file size on the scene (especially when using MeshMode.SingleQuad),
        /// since the data is still stored and the mesh will be generated when entering play mode.
        /// </summary>
        public bool ActiveInEditMode
        {
            get { return m_activeInEditMode; }
            set
            {
                if (m_activeInEditMode == value)
                    return;
                m_activeInEditMode = value;

                if (Application.isEditor)
                {
                    if (m_activeInEditMode)
                        CreateMesh();
                    else
                        DestroyMesh();
                }
            }
        }
        
        public TileMeshSettings MeshSettings
        {
            get { return m_tileMeshSettings; }
            set
            {
                ChunkManager.Settings = value;

                if (m_tileMeshSettings != null)
                {
                    if (m_tileMeshSettings.TileResolution != value.TileResolution)
                    {
                        m_tileMapData.Clear();
                    }
                }

                m_tileMeshSettings = value;
                m_tileMapData.SetSize(m_tileMeshSettings.TilesX, m_tileMeshSettings.TilesY);
            }
        }
        public TileSheet TileSheet
        {
            get { return m_tileSheet; }
        }
        public int TileCount
        {
            get
            {
                return m_tileMapData.Count;
            }
        }
        
        public List<int> WallTileProperty;
        public List<int> ObstacleTileProperty;
        public List<int> FloorTileProperty;
        public List<int> DeadlyTileProperty;

        public void ClearTiles()
        {
            m_tileMapData.Clear();
        }

        private TileChunkManager ChunkManager
        {
            get
            {
                if (m_chunkManager == null)
                {
                    Debug.Log("Recreating TileMeshGrid");
                    m_chunkManager = new TileChunkManager();
                    m_chunkManager.Initialize(this, m_tileMeshSettings);
                }
                return m_chunkManager;
            }
        }
        
        public bool HasMesh
        {
            get { return ChunkManager.Chunk != null; }
        }

        public void AddEntities(Dictionary<Vector2Int, EntitiesData.EntityID> entities)
        {
            foreach(KeyValuePair<Vector2Int, EntitiesData.EntityID> entity in entities)
            {
                AddEntity(entity.Key, entity.Value);
            }
        }

        public void AddEntity(Vector2Int position, EntitiesData.EntityID entity)
        {
            m_entities[position] = entity;
        }

        public void CreateEntities()
        {
            DestroyEntities();

            foreach(KeyValuePair<Vector2Int, EntitiesData.EntityID> entry in m_entities)
            {
                GameObject entity;
                if (entry.Value == EntitiesData.EntityID.Info_T)
                {
                    entity = Instantiate(entities[(int)EntitiesData.EntityID.Info_T_Active]);
                }
                else
                {
                    entity = Instantiate(entities[(int)entry.Value]);
                }
                entity.tag = "Entity_Active";
                entity.transform.position = entry.Key.ToTileVector2();                               
            }
        }

        public void DisplayEntities()
        {
            DestroyEntities();

            foreach (KeyValuePair<Vector2Int, EntitiesData.EntityID> entry in m_entities)
            {
                GameObject entity = Instantiate(entities[(int)entry.Value]);
                entity.tag = "Entity";
                entity.transform.position = entry.Key.ToTileVector2();
            }
        }

        public void RemoveEntity(Vector2Int position)
        {
            if (m_entities.ContainsKey(position))
                m_entities.Remove(position);

            DestroyEntity(position);
        }

        public void DestroyEntity(Vector2Int pos)
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Entity"))
            {
                if(Mathf.FloorToInt(g.transform.position.x) == pos.x && Mathf.FloorToInt(g.transform.position.y) == pos.y)
                {
                    Destroy(g);
                    return;
                }
            }
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Entity_Active"))
            {
                if (Mathf.FloorToInt(g.transform.position.x) == pos.x && Mathf.FloorToInt(g.transform.position.y) == pos.y)
                {
                    Destroy(g);
                    return;
                }
            }
        }

        public void DestroyEntities()
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Entity"))
            {
                Destroy(g);
            }
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Entity_Active"))
            {
                Destroy(g);
            }
        }

        protected virtual void Awake()
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            assetsPath = Application.streamingAssetsPath;

            if (m_tileMeshSettings == null)
                m_tileMeshSettings = new TileMeshSettings(2, 2, 16, 1f, MeshMode.SingleQuad);

            if (m_tileSheet == null)
                m_tileSheet = ScriptableObject.CreateInstance<TileSheet>();

            if (m_chunkManager == null)
            {
                m_chunkManager = new TileChunkManager();
                m_chunkManager.Initialize(this, m_tileMeshSettings);
            }

            if (m_tileMapData == null)
            {
                m_tileMapData = new TileMapData();
                m_tileMapData.SetSize(m_tileMeshSettings.TilesX, m_tileMeshSettings.TilesY);
            }

            if (Application.isPlaying || m_activeInEditMode)
                CreateMesh();
        }

        public int this[int x, int y]
        {
            get { return m_tileMapData[x, y]; }
            set
            {
                //Debug.Log(string.Format("Setting tile ({0}, {1}) = {2}", x, y, value));
                SetTile(x, y, value);
                m_tileMapData[x, y] = value;
            }
        }

        public void CreateMesh()
        {
            // initialize mesh grid
            if (!ChunkManager.Initialized)
                ChunkManager.Initialize(this, m_tileMeshSettings);
            else
                ChunkManager.Settings = m_tileMeshSettings;

            // restore tilemap data
            for (int x = 0; x < m_tileMapData.SizeX; x++)
            {
                for (int y = 0; y < m_tileMapData.SizeY; y++)
                {
                    var id = m_tileMapData[x, y];
                    if (id < 0)
                        continue;
                    SetTile(x, y, id);
                }
            }
        }

        public void CreateColliders()
        {
            for(int x=0; x<m_tileMeshSettings.TilesX; x++)
            {
                for(int y=0; y<m_tileMeshSettings.TilesY; y++)
                {
                    if (gameObject.transform.FindChild(string.Format("Wall_{0}_{1}", x, y))) continue;
                    if (WallTileProperty.Contains(m_tileMapData[x,y]))
                    {
                        var go = new GameObject(string.Format("Wall_{0}_{1}", x, y), typeof(BoxCollider2D));
                        go.transform.SetParent(transform);
                        var box = go.GetComponent<BoxCollider2D>();
                        var rect = GetTileBoundsLocal(x, y);
                        go.transform.localPosition = rect.position;
                        box.offset = new Vector2(rect.width / 2, rect.height / 2);
                        box.size = rect.size;
                    }
                    else if (ObstacleTileProperty.Contains(m_tileMapData[x, y]))
                    {
                        var go = new GameObject(string.Format("Obstacle_{0}_{1}", x, y), typeof(BoxCollider2D));
                        go.transform.SetParent(transform);
                        go.layer = 13; // Ignores bullets hopefully
                        var box = go.GetComponent<BoxCollider2D>();
                        var rect = GetTileBoundsLocal(x, y);
                        go.transform.localPosition = rect.position;
                        box.offset = new Vector2(rect.width / 2, rect.height / 2);
                        box.size = rect.size;
                    }
                }
            }
        }

        public TileProperty GetTileProperty(int id)
        {
            if (WallTileProperty.Contains(id)) return TileProperty.Wall;
            else if (ObstacleTileProperty.Contains(id)) return TileProperty.Obstacle;
            else if (FloorTileProperty.Contains(id)) return TileProperty.Floor;
            else if (DeadlyTileProperty.Contains(id)) return TileProperty.Deadly;
            else return TileProperty.Null;
        }

        public void SetTileProperty(int id, TileProperty prop)
        {
            WallTileProperty.Remove(id);
            ObstacleTileProperty.Remove(id);
            FloorTileProperty.Remove(id);
            DeadlyTileProperty.Remove(id);

            switch (prop)
            {
                case TileProperty.Wall:
                    WallTileProperty.Add(id);
                    break;
                case TileProperty.Obstacle:
                    ObstacleTileProperty.Add(id);
                    break;
                case TileProperty.Floor:
                    FloorTileProperty.Add(id);
                    break;
                case TileProperty.Deadly:
                    DeadlyTileProperty.Add(id);
                    break;
                case TileProperty.Null:
                    FloorTileProperty.Add(id);
                    break;
            }
        }

        public void DestroyColliders()
        {
            for (int x = 0; x < m_tileMeshSettings.TilesX; x++)
            {
                for (int y = 0; y < m_tileMeshSettings.TilesY; y++)
                {
                    if (gameObject.transform.FindChild(string.Format("Collider_{0}_{1}", x, y)) != null)
                    {
                        Destroy(gameObject.transform.FindChild(string.Format("Collider_{0}_{1}", x, y)).gameObject);
                    }
                    
                }
            }
        }

        public void DestroyMesh()
        {
            ChunkManager.DeleteAllChunks();
        }

        /// <summary>
        /// Get the bounding box of a single tile in local coordinates.
        /// This is useful for positioning GameObjects that are children of the Tilemap.
        /// </summary>
        public Rect GetTileBoundsLocal(int x, int y)
        {
            return ChunkManager.Chunk.GetTileBoundsLocal(x, y);
        }

        /// <summary>
        /// Get the bounding box of a single tile in world/scene coordinates.
        /// This is useful for positioning GameObjects that are not children of the Tilemap.
        /// </summary>
        public Rect GetTileBoundsWorld(int x, int y)
        {
            return ChunkManager.Chunk.GetTileBoundsWorld(x, y);
        }

        // TODO this method can currently only be called from code, would be nice with gui checkbox
        // TODO returning the behiaviour like this doesnt feel pretty
        public TileMapVisibilityBehaviour EnableVisibility()
        {
            //if (m_visibility != null)
            //    throw new InvalidOperationException("Visibility already enabled");

            if (m_visibility == null)
            {
                var gameObject = new GameObject("TileMapVisibility", typeof(TileMapVisibilityBehaviour));
                gameObject.transform.parent = transform;
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localPosition = new Vector3(0, -2f, transform.position.z - 1f); // place above tilemap
                gameObject.transform.localScale = Vector3.one;

                m_visibility = gameObject.GetComponent<TileMapVisibilityBehaviour>();
            }
            return m_visibility;
        }

        public bool IsInBounds(int x, int y)
        {
            return m_tileMapData.IsInBounds(x, y);
        }

        public void PaintTile(int x, int y, Color color)
        {
            var child = ChunkManager.Chunk;
            if (child == null)
                throw new InvalidOperationException("MeshGrid has not yet been created.");
            var singleQuad = child as TileMeshSingleQuadBehaviour;
            if (singleQuad == null)
                throw new InvalidOperationException("Painting tiles is only supported in SingleQuad MeshMode");
            singleQuad.SetTile(x, y, color);
        }

        private void SetTile(int x, int y, Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException("sprite");
            ChunkManager.Chunk.SetTile(x, y, sprite);
        }

        private void SetTile(int x, int y, int id)
        {
            var sprite = m_tileSheet.Get(id);
            ChunkManager.Chunk.SetTile(x, y, sprite);
        }

        public IEnumerator<KeyValuePair<Vector2Int, int>> GetEnumerator()
        {
            for (int x = 0; x < m_tileMeshSettings.TilesX; x++)
            {
                for (int y = 0; y < m_tileMeshSettings.TilesY; y++)
                {
                    yield return new KeyValuePair<Vector2Int, int>(new Vector2Int(x, y), m_tileMapData[x, y]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [Serializable]
        public struct TileMapFile
        {
            public TileMapData tileMapData;
            public Dictionary<Vector2Int, EntitiesData.EntityID> entities;
            public float version;

            public TileMapFile(TileMapData data, Dictionary<Vector2Int, EntitiesData.EntityID> ent)
            {
                tileMapData = data;
                entities = ent;
                version = Deftly.Options.MapVersion;
            }
        }

        public void ExportMap(string fileName)
        {
            if (fileName == String.Empty || fileName == null) return; 
            string formattedFileName = fileName.Replace(" ", "_");
            if(File.Exists(assetsPath + "/" + formattedFileName + ".map"))
            {
                File.Delete(assetsPath + "/" + formattedFileName + ".map");
            }
            Serialize(new TileMapFile(m_tileMapData, m_entities), assetsPath + "/" + formattedFileName + ".map");
        }

        public void ImportMap(string filePath)
        {
            TileMapFile mapFile = (TileMapFile)Deserialize(filePath);
            Debug.Log("Imported map file version: " + mapFile.version);
            if (mapFile.version > Deftly.Options.MapVersion)
            {
                Debug.LogError("Map version file requieres version: " + mapFile.version + "! Please download the latest build");
                return;
            }
            TileMapData mapData = mapFile.tileMapData;
            MeshSettings.TilesX = mapData.SizeX;
            MeshSettings.TilesY = mapData.SizeY;

            m_tileMapData = mapData;

            DestroyMesh();
            CreateMesh();

            DestroyEntities();
            m_entities = new Dictionary<Vector2Int, EntitiesData.EntityID>();

            AddEntities(mapFile.entities);
            DisplayEntities();            
        }

        public static void Serialize(object t, string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, t);
            }
        }

        public static object Deserialize(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                return bformatter.Deserialize(stream);
            }
        }

    }
}
