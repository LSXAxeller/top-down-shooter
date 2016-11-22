using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using BeardedManStudios.Network;

public class GUIManager : MonoBehaviour
{

    public enum MenuCanvas
    {
        HUD = 0,
        Scoreboard = 1,
        GameSetup = 2,
        Pause = 3
    };

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
    public GameObject[] ServerOnlyGameSetupList;
    public GameObject VerticalScrollItem;
    public Color[] TeamColors;
    public GameObject StartGame;
    public Button MapNameButton;

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

    public void GUIChangeMap()
    {
        ChangeMap(MapNameInput.text);
    }

    public void ChangeMap(int index)
    {
        GameManager.Instance.SelectedMapIndex = index;
    }

    public void ChangeMap(string name)
    {
        GameManager.Instance.SelectedMapIndex = GameManager.Instance.maps.IndexOf(name);
    }

    public void UpdatePlayersReadyText()
    {
        NetworkingManager.Instance.PollPlayerList((playerList) =>
        {
            PlayersReadyText.text = GameManager.Instance.PlayersReady + "/" + playerList.Count + " players are ready.";
        });
    }

    public void GUIReadyUp()
    {
        GameManager.Instance.ReadyUp(true);

        UpdatePlayersReadyText();
    }

    public void SetupGUIServer()
    {
        StartGame.GetComponentInChildren<Text>().text = "Start Game";
        StartGame.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            if (GameManager.Instance.isReady)
            {
                GUIReadyDown();
                StartGame.GetComponentInChildren<Button>().gameObject.GetComponentInChildren<Text>().text = "Ready Up";
            }
            else
            {
                GUIReadyUp();
                StartGame.GetComponentInChildren<Button>().gameObject.GetComponentInChildren<Text>().text = "Ready Down";
            }
        });
    }

    public void SetupGUIClient()
    {
        StartGame.GetComponentInChildren<Text>().text = "Join Game";
        StartGame.GetComponentInChildren<Button>().gameObject.GetComponentInChildren<Text>().text = "Ready Up";
        StartGame.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            if (GameManager.Instance.isReady)
            {
                GUIReadyDown();
                StartGame.GetComponentInChildren<Button>().gameObject.GetComponentInChildren<Text>().text = "Ready Up";
            }
            else
            {
                GUIReadyUp();
                StartGame.GetComponentInChildren<Button>().gameObject.GetComponentInChildren<Text>().text = "Ready Down";
            }
        });

        foreach (GameObject panel in ServerOnlyGameSetupList)
        {
            panel.SetActive(false);
        }
    }

    public void GUIReadyDown()
    {
        GameManager.Instance.ReadyUp(false);
    }

    public void GUIGameType()
    {

    }

    public void GUIServerName()
    {
        ForgeMasterServer.RegisterServer(Networking.GetExternalIPAddress(), (ushort)Networking.PrimarySocket.Port, 16, ServerNameInput.text, ((GameManager.GameMode)GameTypeInput.value).ToString("F"), "Insert comment here", null, MapNameInput.text);
    }

    public void GUIPlayerName()
    {
        RemovePlayerFromList(Networking.PrimarySocket.Me.Name, Networking.PrimarySocket.Me.MessageGroup);
        Networking.PrimarySocket.Me.SetName(PlayerNameInput.text);
        AddPlayerToList(Networking.PrimarySocket.Me.Name, Networking.PrimarySocket.Me.MessageGroup);
        AddNotification("Your name has been set to: " + Networking.PrimarySocket.Me.Name);
    }

    public void GUITeamID()
    {
        Networking.PrimarySocket.Me.SetMessageGroup(ushort.Parse(TeamInput.text));
        SetTeamPlayerList(Networking.PrimarySocket.Me.Name, Networking.PrimarySocket.Me.MessageGroup);
        AddNotification("Your team has been set to id: " + Networking.PrimarySocket.Me.MessageGroup);
    }

    public void GUIPointLimit()
    {
        GameManager.Instance.Timer = int.Parse(PointLimitInput.text);
        AddNotification("Your timer has been set to: " + GameManager.Instance.Timer + " minutes");
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

    private void Update()
    {
        if (GameManager.Instance.State == GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ShowCanvas(DisplayCanvas.GameSetup);
            }
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                ShowCanvas(DisplayCanvas.HUD);
            }
        }
    }

    public void RemovePlayerFromList(string name, int teamID)
    {
        if (teamID == 0)
        {
            Destroy(LeftTeamList.transform.FindChild(name).gameObject);
        }
        else if (teamID == 1)
        {
            Destroy(LeftTeamList.transform.FindChild(name).gameObject);
        }
        else
        {
            if (LeftTeamList.transform.FindChild(name) != null)
                Destroy(LeftTeamList.transform.FindChild(name).gameObject);
            if (RightTeamList.transform.FindChild(name) != null)
                Destroy(RightTeamList.transform.FindChild(name).gameObject);
        }
    }

    public void SetTeamPlayerList(string name, int teamID)
    {
        if (LeftTeamList.transform.FindChild(name) != null)
            LeftTeamList.transform.FindChild(name).SetParent(LeftTeamList.transform);
        if (RightTeamList.transform.FindChild(name) != null)
            RightTeamList.transform.FindChild(name).SetParent(LeftTeamList.transform);
    }

    public void AddPlayerToList(string name, int teamID)
    {
        UpdatePlayersReadyText();

        GameObject item = Instantiate(VerticalScrollItem);

        if (teamID == 0)
        {
            item.transform.SetParent(LeftTeamList.transform);
            item.transform.SetAsLastSibling();
            item.GetComponent<Image>().color = TeamColors[0];
        }
        else if (teamID == 1)
        {
            item.transform.SetParent(RightTeamList.transform);
            item.transform.SetAsLastSibling();
            item.GetComponent<Image>().color = TeamColors[1];
        }
        else
        {
            item.transform.SetParent(LeftTeamList.transform);
            item.transform.SetAsLastSibling();
            item.GetComponent<Image>().color = TeamColors[0];
        }

        if (string.IsNullOrEmpty(name))
        {
            gameObject.name = "Unknown_" + UnityEngine.Random.Range(100, 999);
        }
        else
        {
            gameObject.name = name;
        }

        item.GetComponentInChildren<Text>().text = "Name: " + gameObject.name;

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
        for (int i = 0; i < ammo; i++)
        {
            ammoText += "I";
        }
        AmmoBulletsText.text = ammoText;
    }

    private void Start()
    {
        StartCoroutine(DisplayNotification());
    }

    public void ShowCanvas(DisplayCanvas index)
    {
        switch ((int)index)
        {
            case 0:
                HUD.alpha = 1.0f;
                Scoreboard.alpha = 0.0f;
                GameSetup.alpha = 0.0f;
                Pause.alpha = 0.0f;

                Scoreboard.blocksRaycasts = false;
                GameSetup.blocksRaycasts = false;
                Pause.blocksRaycasts = false;
                break;
            case 1:
                HUD.alpha = 0.0f;
                Scoreboard.alpha = 1.0f;
                GameSetup.alpha = 0.0f;
                Pause.alpha = 0.0f;

                Scoreboard.blocksRaycasts = true;
                GameSetup.blocksRaycasts = false;
                Pause.blocksRaycasts = false;
                break;
            case 2:
                HUD.alpha = 0.0f;
                Scoreboard.alpha = 0.0f;
                GameSetup.alpha = 1.0f;
                Pause.alpha = 0.0f;

                Scoreboard.blocksRaycasts = false;
                GameSetup.blocksRaycasts = true;
                Pause.blocksRaycasts = false;
                break;
            case 3:
                HUD.alpha = 0.0f;
                Scoreboard.alpha = 0.0f;
                GameSetup.alpha = 0.0f;
                Pause.alpha = 1.0f;

                Scoreboard.blocksRaycasts = false;
                GameSetup.blocksRaycasts = false;
                Pause.blocksRaycasts = true;
                break;
        }
    }

    public enum DisplayCanvas
    {
        HUD,
        Scoreboard,
        GameSetup,
        Pause
    }
}
