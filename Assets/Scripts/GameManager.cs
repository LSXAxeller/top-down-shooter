using System;
using System.Collections.Generic;
using System.Linq;
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

    public int FragLimit;

    public GameState State = GameState.Waiting;
    public GameMode CurGameMode;
    public List<Deftly.Subject> PlayerList;
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
        ownerPlayerData = ObjectMapper.MapBytes(ownerPlayerData, "Test Subject 1", 1, 0);
        gameData = ObjectMapper.MapBytes(gameData, "aaa", 1, "Test Server 1", 16, 10);
    }

    public void Start()
    {
        PlayerList = new List<Deftly.Subject>();
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

    public void SpawnPlayer(string name, int teamID, int subjectSkin)
    {
        if (string.IsNullOrEmpty(name)) return;

        SerializePlayerData(name, teamID, subjectSkin);
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (NetworkingManager.Socket.Connected)
        {
            Networking.PrimarySocket.AddCustomDataReadEvent(55000, (NetworkingPlayer sender, NetworkingStream stream) => {
                ownerPlayerData = (BMSByte)ObjectMapper.MapBMSByte(stream);
                Subject sub = sender.PlayerObject.GetComponent<Subject>();
                sub.Stats.Title = ownerPlayerData.GetString(0);
                sub.Stats.TeamId = ownerPlayerData.GetBasicType<int>(1);
                sub.Stats.SubjectSkin = ownerPlayerData.GetBasicType<int>(2);
                AddToPlayerList(sub);
            });
            Networking.PrimarySocket.AddCustomDataReadEvent(22000, (NetworkingPlayer sender, NetworkingStream stream) => {
                gameData = (BMSByte)ObjectMapper.MapBMSByte(stream);
                MapName = gameData.GetString(0);
                MapManager.ImportMap(Application.streamingAssetsPath + "/" + MapName);
            });
            if (ownerPlayerData[1] == 0)
            {
                Networking.Instantiate("Spectator", SpawnManager.Instance.GetRandomSpawn(), Quaternion.identity, NetworkReceivers.AllBuffered);
            }
            else
            {
                Networking.Instantiate("Player", NetworkReceivers.AllBuffered);
            }
        }
        else
        {
            NetworkingManager.Instance.OwningNetWorker.connected += delegate ()
            {
                Networking.PrimarySocket.AddCustomDataReadEvent(55000, (NetworkingPlayer sender, NetworkingStream stream) => {
                    ownerPlayerData = (BMSByte)ObjectMapper.MapBMSByte(stream);
                    Subject sub = sender.PlayerObject.GetComponent<Subject>();
                    sub.Stats.Title = ownerPlayerData.GetString(0);
                    sub.Stats.TeamId = ownerPlayerData.GetBasicType<int>(1);
                    sub.Stats.SubjectSkin = ownerPlayerData.GetBasicType<int>(2);
                    AddToPlayerList(sub);
                });
                Networking.PrimarySocket.AddCustomDataReadEvent(22000, (NetworkingPlayer sender, NetworkingStream stream) => {
                    gameData = (BMSByte)ObjectMapper.MapBMSByte(stream);
                    MapName = gameData.GetString(0);
                    MapManager.ImportMap(Application.streamingAssetsPath + "/" + MapName);
                });
                if (ownerPlayerData[1] == 0)
                {
                    Networking.Instantiate("Spectator", SpawnManager.Instance.GetRandomSpawn(), Quaternion.identity, NetworkReceivers.AllBuffered);
                }
                else
                {
                    Networking.Instantiate("Player", NetworkReceivers.AllBuffered);
                }
            };
        }
    }

    public void SerializePlayerData(string name, int teamID, int subjectSkin)
    {
        ownerPlayerData.Clone(ObjectMapper.MapBytes(ownerPlayerData, teamID, subjectSkin));
        Networking.WriteCustom(55000, OwningNetWorker, ownerPlayerData, true);
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
            GUIManager.Instance.AddVerticalScrollItem(ownerPlayerData);
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
}
