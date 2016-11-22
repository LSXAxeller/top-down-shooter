using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using BeardedManStudios.Network;
using Deftly;
using UnityEngine.SceneManagement;

public enum GameState
{
    Waiting = 0,
    Ready,
    Playing,
    GameOver,
    Counting
}

public class GameManager : NetworkedMonoBehavior
{

    [NetSync]
    public GameMode CurGameMode;
    [NetSync("OnMapChanged", NetworkCallers.Everyone)]
    public int SelectedMapIndex;
    [NetSync("OnGameStateChanged", NetworkCallers.Everyone)]
    public GameState State;
    private const ushort PORT = 15937;

    public void OnMapChanged()
    {
        State = GameState.Waiting;
        MapManager.ImportMap(Application.streamingAssetsPath + "/" + maps[SelectedMapIndex]);
        Debug.Log("Map changed to " + maps[SelectedMapIndex]);
    }

    public void OnGameStateChanged()
    {
        switch (State)
        {
            case GameState.Waiting:
                GUIManager.Instance.ShowCanvas(GUIManager.DisplayCanvas.GameSetup);
                break;
            case GameState.Counting:
                GUIManager.Instance.ShowCanvas(GUIManager.DisplayCanvas.GameSetup);
                break;
            case GameState.Ready:
                GUIManager.Instance.ShowCanvas(GUIManager.DisplayCanvas.GameSetup);
                StartGame();
                break;
            case GameState.Playing:
                GUIManager.Instance.ShowCanvas(GUIManager.DisplayCanvas.HUD);
                OnGameStart();
                break;
            case GameState.GameOver:
                GUIManager.Instance.ShowCanvas(GUIManager.DisplayCanvas.Scoreboard);
                OnGameOver();
                break;
        }
    }

    public void OnPlayersReadyChanged()
    {
        GUIManager.Instance.UpdatePlayersReadyText();

        if (PlayersReady >= (int)Networking.PrimarySocket.ServerPlayerCounter)
        {
            State = GameState.Ready;
        }
    }

    public GameObject playerObject;
    public readonly float version = 0.1f;
    public delegate void ScoreChangedEventHandler();
    public ScoreChangedEventHandler OnScoreChanged;
    public List<string> maps = new List<string>();
    public UnityTileMap.TileMapBehaviour MapManager;
    public int Timer = 10;

    private static GameManager _instance;
    private bool isBusyFindingLan = false;

    public enum GameMode
    {
        Survival = 0,
        TeamDeathMatch,
        DeathMatch,
        CaptureTheFlag
    }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    return null;
                }
            }
            return _instance;
        }
    }

    private int playersReady;
    internal bool isReady;

    public int PlayersReady
    {
        get
        {
            return playersReady;
        }
        set
        {
            playersReady = value;
            OnPlayersReadyChanged();
        }
    }

    public void Start()
    {
        maps.Add("Test_Map_1");

        QuickStart();
    }

    public void QuickStart()
    {
        //AttemptHosting();
    }

    public void StartClient()
    {
        NetWorker socket = null;

        Networking.LanDiscovery(PORT);
        Networking.lanEndPointFound += (target) =>
        {
            isBusyFindingLan = false;
            if (target == null)
            {
                Debug.Log("No server found on LAN");
                return;
            }

            string ipAddress = string.Empty;
            ushort targetPort = PORT;

            ipAddress = target.Address.ToString();
            targetPort = (ushort)target.Port;

            socket = Networking.Connect(ipAddress, targetPort, Networking.TransportationProtocolType.UDP, false, false);

            Networking.SetPrimarySocket(socket);
        };
    }

    public void AttemptHosting()
    {
        NetWorker socket = null;
        socket = Networking.Host(PORT, Networking.TransportationProtocolType.UDP, 16, false);
        socket.error += PrimarySocket_error;

        if (socket == null)
        {
            StartClient();
        }

        Networking.SetPrimarySocket(socket);
    }

    private void PrimarySocket_error(System.Exception exception)
    {
        StartClient();
        if (exception is System.Net.Sockets.SocketException)
        {
            // Trying to connect to same port error code is 10048, we only want to retry on that one
            if (((System.Net.Sockets.SocketException)exception).ErrorCode == 10048)
            {
                Debug.Log("Port " + PORT + " is in use, trying next port");
            }
            else
                Debug.LogException(exception);
        }
        else
        {
            Debug.LogException(exception);
        }
    }

    public void JoinServer(string ip)
    {
        Networking.Connect(ip, PORT, Networking.TransportationProtocolType.UDP);
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();

        GUIManager.Instance.AddNotification("Sucessfully connected online!");

        State = GameState.Waiting;

        GUIManager.Instance.UpdatePlayersReadyText();

        if (IsServerOwner)
        {
            GUIManager.Instance.SetupGUIServer();
        }
        else
        {
            GUIManager.Instance.SetupGUIClient();
        }

        NetworkingManager.Instance.PollPlayerList((players) =>
        {
            foreach (var player in players)
            {
                GUIManager.Instance.AddPlayerToList(player.Name, player.MessageGroup);
            }
        });

        Networking.PrimarySocket.playerConnected += OnPlayerLobbyJoin;
    }

    private void OnPlayerLobbyJoin(NetworkingPlayer player)
    {
        GUIManager.Instance.AddPlayerToList(player.Name, player.MessageGroup);
        GUIManager.Instance.AddNotification(player.Name + " has joined!");
    }

    private void StartGame()
    {
        if (State != GameState.Playing)
        {
            GUIManager.Instance.AddNotification("Game is about to start!");
            StartCoroutine(CountDown(5, Timer));
        }
    }

    private IEnumerator CountDown(int seconds, int minutes)
    {
        State = GameState.Counting;

        int startTime = seconds;
        while (startTime >= 0)
        {
            GUIManager.Instance.AddNotification(startTime.ToString(), 1f);
            startTime--;
            yield return new WaitForSeconds(1.0f);
        }

        State = GameState.Playing;

        int gameTime = minutes * 60;
        while (gameTime >= 0)
        {
            GUIManager.Instance.AddNotification(gameTime.ToString(), 1f);
            gameTime--;
            yield return new WaitForSeconds(1.0f);
        }

        State = GameState.GameOver;
    }

    private void OnGameOver()
    {
        RPC("CloseRoom", Networking.PrimarySocket, NetworkReceivers.All);
    }

    private void OnGameStart()
    {
        MapManager.ImportMap(Application.streamingAssetsPath + "/" + maps[0] + ".map");
        MapManager.CreateEntities();

        InitiatePlayer("AK47", "M4A1", null);
    }

    public void BroadcastChangeMap(int index)
    {
        if (IsServerOwner)
        {
            RPC("ChangeMap", index);
        }
    }

    public void InitiatePlayer(string primary, string secondary, string grenade)
    {
        /*
        GUIManager.Instance.OnWeaponChanged -= InitiatePlayer;
        object[] weaponIndicies = new object[]
                                      {
                                          PrimaryWeaponsList.IndexOf(primary), 
                                          SecondaryWeaponsList.IndexOf(secondary),
                                          GranadeList.IndexOf(granade)
                                      };
        GameObject player = PhotonNetwork.Instantiate("Player", _spawnPositions[Random.Range(0, _spawnPositions.Length)].transform.position, Quaternion.identity, 0, weaponIndicies);
        player.GetComponent<Player>().OnScoreChanged += ScoreChanged;
        */
        Networking.Instantiate(playerObject, SpawnManager.Instance.GetTeamSpawnPosition(0), Quaternion.identity, NetworkReceivers.All, (player) =>
        {
            //TODO: Assaign weapon data here
        });
        Camera.main.GetComponent<DeftlyCamera>().enabled = true;
    }

    public void BanPlayer(string playerName, int timePeriod)
    {
        Networking.PrimarySocket.BanPlayer(FindPlayer(playerName), timePeriod);
    }

    public static NetworkingPlayer FindPlayer(string name)
    {
        return Networking.PrimarySocket.Players.Find(player => player.Name == name);
    }

    public void ReadyUp(bool state)
    {
        isReady = state;
        RPC("RPCReadyUp", state);
    }

    [BRPC]
    public void RPCReadyUp(bool state)
    {

        if (state == true)
            PlayersReady++;
        else if (state == false)
            PlayersReady--;
    }

    [BRPC]
    void CloseRoom()
    {
        Debug.Log("My master, my sir LEFT ME!");
        Networking.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    protected override void NetworkDisconnect()
    {
        base.NetworkDisconnect();
        GUIManager.Instance.RemovePlayerFromList(OwningPlayer.Name, OwningPlayer.MessageGroup);
        SceneManager.LoadScene(1);
    }
}
