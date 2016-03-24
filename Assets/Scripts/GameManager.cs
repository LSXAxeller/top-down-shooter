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
    GameOver
}

public class GameManager : NetworkedMonoBehavior {
    
    public readonly float version = 0.1f;

    public enum GameMode
    {
        Survival = 0,
        TeamDeathMatch,
        DeathMatch,
        CaptureTheFlag
    }

    public delegate void ScoreChangedEventHandler();
    public ScoreChangedEventHandler OnScoreChanged;

    /// <summary>
    /// Paths to map files from StreamingAssets folder
    /// </summary>
    public List<string> maps = new List<string>();

    public int FragLimit;

    public GameState State
    {
        get
        {
            return State;
        }
        set
        {
            switch(value)
            {
                case GameState.Waiting:
                    GUIManager.Instance.ShowCanvas(2);
                    break;
                case GameState.Ready:
                    GUIManager.Instance.ShowCanvas(0);
                    break;
                case GameState.GameOver:
                    GUIManager.Instance.ShowCanvas(2);
                    break;
            }
        }
    }
    public GameMode CurGameMode;
    public List<Subject> PlayerList;
    public int PlayersReady = 0;

    private static GameManager _instance;
    public string MapName;
    public string MapType;
    public UnityTileMap.TileMapBehaviour MapManager;
    /// <summary>
    /// string name, int teamID, int subjectSkin
    /// </summary>
    public BMSByte ownerPlayerData = new BMSByte();
    /// <summary>
    /// string mapName, int gameMode, string serverName, int maxPlayers, int pointLimit
    /// </summary>
    public BMSByte gameData = new BMSByte();

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

    private void Awake()
    {
        maps.Add("Test_Map_1");
        MapManager.ImportMap(Application.streamingAssetsPath + "/" + maps[0] + ".map");

        ownerPlayerData = ObjectMapper.MapBytes(ownerPlayerData, "Test Subject 1", 1, 0);
        gameData = ObjectMapper.MapBytes(gameData, "aaa", 1, "Test Server 1", 16, 10);
    }

    public void Start()
    {
        PlayerList = new List<Subject>();
        State = GameState.Waiting;
        NetworkingManager.Socket.playerConnected += Socket_playerConnected;
    }

    private void Socket_playerConnected(NetworkingPlayer player)
    {
        GUIManager.Instance.AddVerticalScrollItem(player.Name, 0);
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        GUIManager.Instance.UpdatePlayersReadyText();
        OwningNetWorker.playerConnected += (player) =>
        {
            AddToPlayerList(player.PlayerObject.GetComponent<Subject>());
        };
    }

    public void StartGame(string name, int teamId, int skinId)
    {
        State = GameState.Ready;

        ConnectPlayer();

        SerializePlayerData(GUIManager.Instance.PlayerNameInput.text, int.Parse(GUIManager.Instance.TeamInput.text), 0);

        MyPlayer.GetComponent<Subject>().SetInputPermission(true, false, false);

        StartCoroutine(CountDown(5));
    }

    private IEnumerator CountDown(int time)
    {
        int seconds = time;
        while (seconds >= 0)
        {
            GUIManager.Instance.AddNotification(seconds.ToString(), 1f);
            seconds--;
            yield return new WaitForSeconds(1.0f);
        }
        MyPlayer.GetComponent<Subject>().SetInputPermission(true, true, true);
    }

    private void SetGameMode()
    {
        if (CurGameMode == GameMode.TeamDeathMatch)
        {          
            // GUIManager.Instance.TeamSelectionMenu = GUIManager.Instance.TeamSelectionTDMMenu;
            // GUIManager.Instance.Scoreboard = GUIManager.Instance.TDMScoreboard;
        }
        else if (CurGameMode == GameMode.DeathMatch)
        {
            // GUIManager.Instance.TeamSelectionMenu = GUIManager.Instance.TeamSelectionFFAMenu;
            // GUIManager.Instance.Scoreboard = GUIManager.Instance.FFAScoreboard;
        }
        else if (CurGameMode == GameMode.CaptureTheFlag)
        {
            // GUIManager.Instance.TeamSelectionMenu = GUIManager.Instance.TeamSelectionCTFMenu;
            // GUIManager.Instance.Scoreboard = GUIManager.Instance.TDMScoreboard;
        }
    }

    public void ChangeMap(int index)
    {
        RPC("ChangeMapRPC", index);
    }

    public void ConnectPlayer()
    {
        if (NetworkingManager.Socket.Connected)
        {
            SpawnPlayer();
        }
        else
        {
            NetworkingManager.Socket.connected += delegate ()
            {
                SpawnPlayer();
            };
        }
    }

    private void SpawnPlayer()
    {
        Networking.PrimarySocket.AddCustomDataReadEvent((uint)55000, (NetworkingPlayer sender, NetworkingStream stream) => {
            ownerPlayerData = (BMSByte)ObjectMapper.MapBMSByte(stream);
            Subject sub = sender.PlayerObject.GetComponent<Subject>();
            sub.Stats.Title = ownerPlayerData.GetString(0);
            sub.Stats.TeamId = ownerPlayerData.GetBasicType<int>(1);
            sub.Stats.SubjectSkin = ownerPlayerData.GetBasicType<int>(2);
            AddToPlayerList(sub);
        });
        Networking.PrimarySocket.AddCustomDataReadEvent((uint)22000, (NetworkingPlayer sender, NetworkingStream stream) => {
            gameData = (BMSByte)ObjectMapper.MapBMSByte(stream);
            MapName = gameData.GetString(0);
            MapManager.ImportMap(Application.streamingAssetsPath + "/" + MapName);
        });
        if (ownerPlayerData[1] == 2)
        {
            Networking.Instantiate("Spectator", SpawnManager.Instance.GetRandomSpawnPosition(), Quaternion.identity, NetworkReceivers.AllBuffered);
        }
        else
        {
            Networking.Instantiate("Player", SpawnManager.Instance.GetTeamSpawnPosition(ownerPlayerData[1]), Quaternion.identity, NetworkReceivers.AllBuffered, (player) => {
                player.GetComponent<Subject>().SetSkin(0);
            });
        }

        Camera.main.GetComponent<DeftlyCamera>().enabled = true;
    }

    public void SerializePlayerData(string name, int teamID, int subjectSkin)
    {
        if (NetworkingManager.Socket.Connected)
        {
            ownerPlayerData.Clone(ObjectMapper.MapBytes(ownerPlayerData, teamID, subjectSkin));
            Networking.WriteCustom((uint)55000, NetworkingManager.Socket, ownerPlayerData, true);
        }
        else
        {
            NetworkingManager.Socket.connected += delegate ()
            {
                ownerPlayerData.Clone(ObjectMapper.MapBytes(ownerPlayerData, teamID, subjectSkin));
                Networking.WriteCustom((uint)55000, NetworkingManager.Socket, ownerPlayerData, true);
            };
        }
    }

    public void SerializeGameData(string mapName, int gameMode, string serverName, int maxPlayers, int pointLimit)
    {
        gameData.Clone(ObjectMapper.MapBytes(gameData, mapName, gameMode, serverName, maxPlayers, pointLimit));
        Networking.WriteCustom(22000, OwningNetWorker, gameData, true);
    }
    
    public void BanPlayer(string playerName, int timePeriod)
    {
        OwningNetWorker.BanPlayer(FindPlayer(playerName), timePeriod);
    }

    public NetworkingPlayer FindPlayer(string name)
    {
        return OwningNetWorker.Players.Find(player => player.Name == name);
    }

    public void AddToPlayerList(Subject player)
    {
        bool _addToList = true;
        foreach (Subject p in PlayerList)
        {
            if (p == player)
            {
                _addToList = false;
                break;
            }
        }

        if (_addToList)
        {
            PlayerList.Add(player);
        }
    }

    public void EndGame()
    {
        State = GameState.GameOver;
        RPC("CloseRoom", NetworkingManager.Socket, NetworkReceivers.All);
    }

    public void OwnerReadyUp(byte state)
    {
        RPC("ReadyUp", state);
    }

    [BRPC]
    private void ChangeMapRPC(int index)
    {
        MapManager.ImportMap(Application.streamingAssetsPath + "/" + maps[index]);
        Debug.Log("Map changed to " + maps[index]);
    }

    [BRPC]
    private void ReadyUp(byte state)
    {
        if (state == 0)
            PlayersReady++;
        else if (state == 1)
            PlayersReady--;
    }

    [BRPC]
    void CloseRoom()
    {
        Debug.Log("My master, my sir LEFT ME!");
        Networking.Disconnect();
        SceneManager.LoadScene(1);
    }

    protected override void NetworkDisconnect()
    {
        base.NetworkDisconnect();
        SceneManager.LoadScene(1);
    }
}
