using UnityEngine;

public class LoadFileBrowser : MonoBehaviour {

    public GameObject Map;
	public GUISkin[] skins;
	public Texture2D file,folder,back,drive;

    private UnityTileMap.TileMapBehaviour tileMap;
    private TileMapGameBahaviour tileMapGame;
	private FileBrowser fb = new FileBrowser();
    private string output = "no file";

    void Start () {
        fb.setDirectory(Application.streamingAssetsPath);
        fb.setLayout(1);
		fb.guiSkin = skins[0];
		fb.fileTexture = file; 
		fb.directoryTexture = folder;
		fb.backTexture = back;
		fb.driveTexture = drive;
		fb.showSearch = true;
		fb.searchRecursively = true;
        tileMap = FindObjectOfType<UnityTileMap.TileMapBehaviour>();
        tileMapGame = FindObjectOfType<TileMapGameBahaviour>();
	}

    void OnEnable()
    {
        fb.setDirectory(Application.streamingAssetsPath);
    }
	
	void OnGUI() {
		if(fb.draw())
        {
            output = (fb.outputFile==null)?"cancel hit":fb.outputFile.ToString();
            if (output.EndsWith(".png") || output.EndsWith(".jpg") || output.EndsWith(".JPG") || output.EndsWith(".PNG"))
            {
                Debug.Log("Image file detected;");
                tileMapGame.ImportTexture(output);
            }
            else if(output.EndsWith(".map") || output.EndsWith(".MAP"))
            {
                Debug.Log("Game file detected");
                tileMap.ImportMap(output);
            }
            gameObject.SetActive(false);
		}
	}
}
