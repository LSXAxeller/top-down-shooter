using UnityEngine;

public class SaveFileBrowser : MonoBehaviour {

    public GameObject Map;
	public GUISkin[] skins;
	public Texture2D file,folder,back,drive;

    private TileMapGameBahaviour tileMapGame;
	private FileBrowser fb = new FileBrowser();

    void Start () {
        fb.setDirectory(Application.streamingAssetsPath);
		fb.guiSkin = skins[0];
		fb.fileTexture = file; 
		fb.directoryTexture = folder;
		fb.backTexture = back;
		fb.driveTexture = drive;
		fb.showSearch = true;
        fb.setLayout(1);
		fb.searchRecursively = true;
        tileMapGame = FindObjectOfType<TileMapGameBahaviour>();
	}

    void OnEnable()
    {
        fb.setDirectory(Application.streamingAssetsPath);
    }

    void OnGUI() {
		if(fb.draw())
        {
            tileMapGame.ExportMap();
            gameObject.SetActive(false);
		}
	}
}
