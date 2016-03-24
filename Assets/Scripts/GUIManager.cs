using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using BeardedManStudios.Network;

public class GUIManager : MonoBehaviour {

    public enum MenuCanvas
    {
        HUD = 0,
        Scoreboard = 1,
        GameSetup = 2,
        Pause = 3
    };

    public int SelectedMapIndex
    {
        get
        {
            return SelectedMapIndex;
        }
        set
        {
            SelectedMapIndex = value;
            BMSByte data = GameManager.Instance.gameData;
            GameManager.Instance.SerializeGameData(MapNameInput.text, data[1], data.GetString(2), data.GetBasicType<int>(3), data.GetBasicType<int>(4));
            GameManager.Instance.ChangeMap(value);
        }
    }
    
    public CanvasGroup HUD;
    public CanvasGroup Scoreboard;
    public CanvasGroup GameSetup;
    public CanvasGroup Pause;
    public InputField PlayerNameInput;
    public InputField BanPlayerInput;
    public InputField ServerNameInput;
    public InputField MapNameInput;
    public InputField TeamInput;
    public InputField PointLimitInput;
    public Dropdown GameTypeInput;
    public Text NotificationText;
    public Text PlayersReadyText;
    public Slider HealthBarSlider;
    public Text AmmoBulletsText;
    public Text SelectedWeaponText;
    public Image PrimaryWeaponIcon;
    public Image SecondaryWeaponIcon;
    public GameObject LeftTeamList;
    public GameObject RightTeamList;
    public GameObject VerticalScrollItem;
    public Color[] TeamColors;

    private List<string> pendingNotifications = new List<string>();
    private string currentNotification;
    private float notificationTimer = 1.0f;
    private static GUIManager _instance; 

    public static GUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GUIManager>();
                if (_instance == null)
                {
                    _instance = GameObject.FindGameObjectWithTag("GUIManager").GetComponent<GUIManager>();
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<GUIManager>();
                        if (_instance == null)
                        {
                            return null;
                        }
                    }
                }
            }
            return _instance;
        }
    }

    public void GUIStartGame()
    {
        if (string.IsNullOrEmpty(PlayerNameInput.text))
        {
            AddNotification("A player name is required to start the game!");
            throw new MissingReferenceException("Player name cannot be empty!");
        }

        if(string.IsNullOrEmpty(TeamInput.text))
        {
            AddNotification("A team ID is required to start the game!");
            throw new MissingReferenceException("Team ID cannot be empty!");
        }

        GameManager.Instance.StartGame(PlayerNameInput.text, int.Parse(TeamInput.text), 0);
    }

    public void GUIChangeMap(int index)
    {
        SelectedMapIndex = index;
    }

    public void GUIChangeMap(string name)
    {
        SelectedMapIndex = GameManager.Instance.maps.IndexOf(name);
    }

    public void UpdatePlayersReadyText()
    {
        PlayersReadyText.text = GameManager.Instance.PlayersReady + "/" + NetworkingManager.ControllingSocket.ServerPlayerCounter + " players are ready.";
    }

    public void GUIReadyUp()
    {
        GameManager.Instance.OwnerReadyUp(0);
        UpdatePlayersReadyText();
    }

    public void GUIReadyDown()
    {
        GameManager.Instance.OwnerReadyUp(1);
        UpdatePlayersReadyText();
    }

    public void GUIGameType()
    {
        BMSByte data = GameManager.Instance.gameData;
        GameManager.Instance.SerializeGameData(data.GetString(0), GameTypeInput.value, data.GetString(2), data.GetBasicType<int>(3), data.GetBasicType<int>(4));
    }

    public void GUIServerName()
    {
        BMSByte data = GameManager.Instance.gameData;
        GameManager.Instance.SerializeGameData(data.GetString(0), data.GetBasicType<int>(0), ServerNameInput.text, data.GetBasicType<int>(3), data.GetBasicType<int>(4));
    }

    public void GUIPlayerName()
    {
        BMSByte data = GameManager.Instance.ownerPlayerData;
        GameManager.Instance.SerializePlayerData(PlayerNameInput.text, data.GetBasicType<int>(2), data.GetBasicType<int>(2));
    }

    public void GUITeamID()
    {
        BMSByte data = GameManager.Instance.ownerPlayerData;
        GameManager.Instance.SerializePlayerData(data.GetString(0), int.Parse(TeamInput.text), data.GetBasicType<int>(2));
    }

    public void GUIPointLimit()
    {
        BMSByte data = GameManager.Instance.gameData;
        GameManager.Instance.SerializeGameData(data.GetString(0), data.GetBasicType<int>(1), data.GetString(2), data.GetBasicType<int>(3), int.Parse(PointLimitInput.text));
    }

    public void GUIBanPlayer()
    {
        string playerName = BanPlayerInput.text;
        GameManager.Instance.BanPlayer(playerName, 15);
        AddNotification(playerName + " has been banned for 15 minutes!");
    }

    public void GUIQuit()
    {
        Application.Quit();
    }

    private void Update ()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ShowCanvas(1);
        }
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            ShowCanvas(0);
        }
    }

    public void AddVerticalScrollItem(string name, int teamID)
    {
        GameObject item = Instantiate(VerticalScrollItem);

        if(teamID == 0)
        {          
            item.transform.SetParent(LeftTeamList.transform);
            item.transform.SetAsLastSibling();
            item.GetComponent<Image>().color = TeamColors[0];
        }
        else if(teamID == 1)
        {
            item.transform.SetParent(RightTeamList.transform);
            item.transform.SetAsLastSibling();
            item.GetComponent<Image>().color = TeamColors[1];
        }

        item.GetComponentInChildren<Text>().text = "Name: " + name;
    }

    public void AddNotification(string notification)
    {
        notificationTimer = 1.0f;
        pendingNotifications.Add(notification);
    }

    public void AddNotification(string notification, float time)
    {
        notificationTimer = time;
        pendingNotifications.Add(notification);
    }

    IEnumerator DisplayNotification()
    {
        while (true)
        {
            if (pendingNotifications.Count > 0)
            {
                currentNotification = pendingNotifications[0];
                pendingNotifications.RemoveAt(0);
                NotificationText.text = currentNotification;
                yield return new WaitForSeconds(notificationTimer);
            }
            else
            {
                NotificationText.text = string.Empty;
                yield return new WaitForSeconds(notificationTimer);
            }
        }
    }

    public void UpdateWeaponStats(Sprite primary, Sprite secondary, string selectedWeaponName)
    {
        PrimaryWeaponIcon.sprite = primary;
        SecondaryWeaponIcon.sprite = secondary;
        SelectedWeaponText.text = selectedWeaponName.TrimEnd('(');
    }

    public void SetHealthBar(float health, float maxHealth)
    {
        HealthBarSlider.maxValue = maxHealth;
        HealthBarSlider.value = health;
    }

    public void SetAmmoBar(float ammo)
    {
        string ammoText = "";
        for(int i=0; i<ammo; i++)
        {
            ammoText += "I";
        }
        AmmoBulletsText.text = ammoText;
    }

    private void Start()
    {
        StartCoroutine(DisplayNotification());
    }

    public void ShowCanvas(int index)
    {
        switch (index)
        {
            case 0:
                HUD.alpha = 1.0f;
                HUD.blocksRaycasts = true;
                HUD.interactable = true;
                Scoreboard.alpha = 0.0f;
                Scoreboard.blocksRaycasts = false;
                Scoreboard.interactable = false;
                GameSetup.alpha = 0.0f;
                GameSetup.blocksRaycasts = false;
                GameSetup.interactable = false;
                Pause.alpha = 0.0f;
                Pause.blocksRaycasts = false;
                Pause.interactable = false;
                break;
            case 1:
                HUD.alpha = 0.0f;
                HUD.blocksRaycasts = false;
                HUD.interactable = false;
                Scoreboard.alpha = 1.0f;
                Scoreboard.blocksRaycasts = true;
                Scoreboard.interactable = true;
                GameSetup.alpha = 0.0f;
                GameSetup.blocksRaycasts = false;
                GameSetup.interactable = false;
                Pause.alpha = 0.0f;
                Pause.blocksRaycasts = false;
                Pause.interactable = false;
                break;
            case 2:
                HUD.alpha = 0.0f;
                HUD.blocksRaycasts = false;
                HUD.interactable = false;
                Scoreboard.alpha = 0.0f;
                Scoreboard.blocksRaycasts = false;
                Scoreboard.interactable = false;
                GameSetup.alpha = 1.0f;
                GameSetup.blocksRaycasts = true;
                GameSetup.interactable = true;
                Pause.alpha = 0.0f;
                Pause.blocksRaycasts = false;
                Pause.interactable = false;
                break;
            case 3:
                HUD.alpha = 0.0f;
                HUD.blocksRaycasts = false;
                HUD.interactable = false;
                Scoreboard.alpha = 0.0f;
                Scoreboard.blocksRaycasts = false;
                Scoreboard.interactable = false;
                GameSetup.alpha = 0.0f;
                GameSetup.blocksRaycasts = false;
                GameSetup.interactable = false;
                Pause.alpha = 1.0f;
                Pause.blocksRaycasts = true;
                Pause.interactable = true;
                break;
        }
    }
}
