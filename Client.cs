using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Assets.Scripts;
using System;

public class Client : MonoBehaviour
{
    public bool done = false;

    public static Client instance;
    public static int bufferSize = 4096;

    public string ip = "127.0.0.1";
    public  int port = 52775;
    public  int myId = 0;
    public  bool isHost = false;
    public int impostorId;

    public  Player[] otherPlayers;
    int[] playersVotes = new int[11];
    int votesCounter = 0;
    int doneTasks = 0;
    string whoWon = "game in progress";
    bool canCallTheCheckWinMethod = false;


    Player  MyPlayer;
    bool  isImpostor;

    public  Socket socket;
    Queue<MessageType> DoThings;
    

    public void Awake()
    {

        if (instance == null)
        {
            instance = this;    
        }
        else
        {
            return;
        }
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        };
        socket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
       
        socket.Connect(IPAddress.Parse(ip),port);
        
        IDictionary<string, string> messageHandler = new Dictionary<string,string>();
        messageHandler.Add("Event", "Connection");
        string message = JsonConvert.SerializeObject(messageHandler);
        
        socket.Send(UTF8Encoding.UTF8.GetBytes(message));
        Debug.Log(message);
        DoThings = new Queue<MessageType>();
        Thread contactThread = new Thread(Contact);
        contactThread.Start();
        Vent.SetTravelVents();
}

    public void UpdateClient()
    {
        
    }

    public void SendData(MessageType dictMessage) 
    {
        string message = JsonConvert.SerializeObject(dictMessage);
        socket.Send(UTF8Encoding.UTF8.GetBytes(message));
    }

    public MessageType ReceiveData() 
    {
        byte[] buffer = new byte[bufferSize];
        int size = socket.Receive(buffer);
        if (size == 0)
            return null;
        string data = UTF8Encoding.UTF8.GetString(buffer);
        MessageType messageHandler = JsonConvert.DeserializeObject<MessageType>(data);
        return messageHandler;
    }


    public void FixedUpdate()
    {
        MessageType messageToSend = new MessageType();
        if (instance.DoThings.Count>0)
        {
            MessageType messageHandler = instance.DoThings.Dequeue();
            string[] colorByID = { "blue","red","green","yellow","white","black","orange","pink","brown","cyan"};
            if (messageHandler == null) return;
            if (messageHandler.type == "host")
            {
                instance.isHost = true;
                instance.myId = 0;
            }
            else if(messageHandler.type == "member")
            {
                instance.myId = messageHandler.id;
            }
            if (messageHandler.type == "playerList")
            {
                instance.otherPlayers = new Player[messageHandler.numberOfPlayers - 1];
                int j=0;
                for (int i = 0; i < messageHandler.numberOfPlayers; i++)
                {
                    if (messageHandler.playerList[i] != instance.myId)
                    {
                        
                        GameObject player = GameObject.Instantiate((GameObject)Resources.Load("Player"));

                        //player.transform.position = new Vector3(7.935f+messageHandler.playerList[i], 11.437f, -1);
                        player.transform.position = new Vector3(7.935f , 11.437f, -1);
                        player.transform.localScale = new Vector3(1, 1, 1);
                        Player p;
                        if(messageHandler.playerList[i] == instance.impostorId)
                        {
                            p = new Impostor(player);
                        }
                        else
                        {
                            p = new Player(player);
                        }
                        p.id = messageHandler.playerList[i];
                        instance.otherPlayers[j] = p;
                        Debug.Log("other players "+instance.otherPlayers[j]);
                        j++;
                    }
                }
            }
            if (messageHandler.type == "role")
            {
                GameObject player = GameObject.Instantiate((GameObject)Resources.Load("Player"));
                //player.transform.position = new Vector3(7.935f+ instance.myId, 11.437f, -1);
                player.transform.position = new Vector3(7.935f , 11.437f, -1);
                GameObject.Find("text").GetComponent<Text>().text = instance.myId.ToString();
                player.transform.localScale = new Vector3(1, 1, 1);
                instance.isImpostor = messageHandler.isImpostor;
                if (messageHandler.isImpostor)
                {
                    instance.MyPlayer = new Impostor(player);
                    GameObject.Find("roleCanvas").GetComponent<Canvas>().enabled = true;
                    GameObject.Find("roleCanvas/playerRole").GetComponent<Text>().text = "Role - Impostor";
                }
                else
                {
                    instance.MyPlayer = new Player(player);
                    GameObject.Find("roleCanvas/playerRole").GetComponent<Text>().text = "Role - Crew Mate";
                }
                instance.MyPlayer.id = instance.myId;

                player.AddComponent<PlayerMovement>().Initialize(instance.MyPlayer, instance.isImpostor, instance.otherPlayers, 12);
                instance.MyPlayer.color = colorByID[instance.myId];
                Sprite sprite = (Resources.Load<Sprite>(instance.MyPlayer.color));

                instance.MyPlayer.playerObject.GetComponent<SpriteRenderer>().sprite = sprite;
                SetColorByID(instance.otherPlayers);
                for (int i = 0; i < instance.otherPlayers.Length; i++)
                {
                    Player[] players = (Player[])instance.otherPlayers.Clone();
                    players[i] = instance.MyPlayer;
                    instance.otherPlayers[i].playerObject.AddComponent<Move>().Initialize(instance.otherPlayers[i], instance.otherPlayers[i] is Impostor, players, 12);
                }
                canCallTheCheckWinMethod = true;
            }

            if (messageHandler.type =="impostor ID")
            {
                instance.impostorId = messageHandler.id;
            }

            if (messageHandler.type == "player")
            {
                messageHandler.playerData.UpdatePlayer(instance.otherPlayers[messageHandler.playerData.id]);
            }

            if (messageHandler.type == "buttonPressed")
            {
                Player p=GetPlayerbyId(messageHandler.id);
                p.playerObject.GetComponent<Move>().UpdateMovement(messageHandler.buttons);
            }
            if (messageHandler.type == "updatePositions" && isHost)
            {
                MessageType messageHandler2 = new MessageType();
                for (int i = 0; i < instance.otherPlayers.Length; i++)
                {
                    messageHandler2.playersPositionsId.Add((instance.otherPlayers[i].playerObject.transform.position, instance.otherPlayers[i].id));
                }
                messageHandler2.playersPositionsId.Add((instance.MyPlayer.playerObject.transform.position,instance.myId));
                messageHandler2.type = "retUpdatePositions";
                SendData(messageHandler2);
            }
            if (messageHandler.type == "retUpdatePositions")
            {
                for (int i = 0; i < instance.otherPlayers.Length+1; i++)
                {
                    ChangePosition(GetPlayerbyId(messageHandler.playersPositionsId[i].Item2), messageHandler.playersPositionsId[i].Item1);
                }
            }
            if (messageHandler.type == "travelToOtherVent")
            {
                instance.GetPlayerbyId(messageHandler.playerPositionId.Item2).playerObject.transform.position = messageHandler.playerPositionId.Item1;
            }
            if (messageHandler.type == "voting")
            {
                votesCounter++;
                if (messageHandler.votedFor == "pink")
                {
                    playersVotes[0]++;
                }
                else if (messageHandler.votedFor == "brown")
                {
                    playersVotes[1]++;
                }
                else if (messageHandler.votedFor == "black")
                {
                    playersVotes[2]++;
                }
                else if (messageHandler.votedFor == "cyan")
                {
                    playersVotes[3]++;
                }
                else if (messageHandler.votedFor == "white")
                {
                    playersVotes[4]++;
                }
                else if (messageHandler.votedFor == "orange")
                {
                    playersVotes[5]++;
                }
                else if (messageHandler.votedFor == "yellow")
                {
                    playersVotes[6]++;
                }
                else if (messageHandler.votedFor == "blue")
                {
                    playersVotes[7]++;
                }
                else if (messageHandler.votedFor == "red")
                {
                    playersVotes[8]++;
                }
                else if (messageHandler.votedFor == "green")
                {
                    playersVotes[9]++;
                }
                else if (messageHandler.votedFor == "skip")
                {
                    playersVotes[10]++;
                }
                if (!instance.ContinueMeeting(votesCounter))
                {
                    Vote votedPlayer = MeetingResults(playersVotes);
                    playersVotes = new int[11];
                    Debug.Log(votedPlayer.tie);
                    if (!votedPlayer.tie && votedPlayer.color != "skip")
                    {
                        string votedPlayerColor = votedPlayer.color;
                        int votedPlayerId = -1;

                        if (votedPlayerColor == "blue")
                        {
                            votedPlayerId = 0;
                        }
                        if (votedPlayerColor == "red")
                        {
                            votedPlayerId = 1;
                        }
                        if (votedPlayerColor == "green")
                        {
                            votedPlayerId = 2;
                        }
                        if (votedPlayerColor == "yellow")
                        {
                            votedPlayerId = 3;
                        }
                        if (votedPlayerColor == "white")
                        {
                            votedPlayerId = 4;
                        }
                        if (votedPlayerColor == "black")
                        {
                            votedPlayerId = 5;
                        }
                        if (votedPlayerColor == "orange")
                        {
                            votedPlayerId = 6;
                        }
                        if (votedPlayerColor == "pink")
                        {
                            votedPlayerId = 7;
                        }
                        if (votedPlayerColor == "brown")
                        {
                            votedPlayerId = 8;
                        }
                        if (votedPlayerColor == "cyan")
                        {
                            votedPlayerId = 9;
                        }
                        Debug.Log("voted player id "+votedPlayerId);
                        EjectPlayer(GetPlayerbyId(votedPlayerId));
                        playersVotes = new int[11];
                    }

                    instance.MyPlayer.EndMeeting(instance.otherPlayers);
                    instance.MyPlayer.meetingInProgress = false;
                    Debug.Log(CheckWhoWon(otherPlayers, doneTasks));
                    votesCounter = 0;
                }
            }
            if (messageHandler.type == "exitVent")
            {
                Player player = GetPlayerbyId(messageHandler.playerPositionId.Item2);
                player.playerObject.transform.position = messageHandler.playerPositionId.Item1;
                player.playerObject.GetComponent<SpriteRenderer>().enabled = true;
            }
            if (messageHandler.type == "enterVent")
            {
                Player player = GetPlayerbyId(messageHandler.playerPositionId.Item2);
                player.playerObject.transform.position = messageHandler.playerPositionId.Item1;
                player.playerObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            if (messageHandler.type == "killAction")
            {
                if (MyPlayer is Impostor)
                {
                    Debug.Log(instance.MyPlayer.id);
                }

                Impostor impostor = (Impostor)GetPlayerbyId(messageHandler.senderID);
                impostor.KillForSure(GetPlayerbyId(messageHandler.id));
                string deadPlayerColor = GetPlayerbyId(messageHandler.id).color+"Dead";
                GameObject.Find(deadPlayerColor).transform.position = GetPlayerbyId(messageHandler.id).playerObject.transform.position;
                GameObject.Find(deadPlayerColor).GetComponent<SpriteRenderer>().enabled = true;
                Destroy(GetPlayerbyId(messageHandler.id).playerObject.GetComponent<PolygonCollider2D>());
            }
            if (messageHandler.type == "startMeeting")
            {
                votesCounter = 0;
                instance.MyPlayer.Meeting(otherPlayers);
                instance.MyPlayer.meetingInProgress = true;
            }
            if (messageHandler.type == "finishTask")
            {
                doneTasks++;
            }
            Debug.Log(messageHandler.type);
            if (canCallTheCheckWinMethod)
            {
                string win = CheckWhoWon(otherPlayers, doneTasks);
                if (win == "impostors")
                {
                    GameObject.Find("endGameCanvas").GetComponent<Canvas>().enabled = true;
                    GameObject.Find("endGameCanvas/whoWon").GetComponent<Text>().text = "Impostors Won!!!";
                    instance.MyPlayer.gameOver = true;
                }
                else if (win == "crewMates")
                {
                    GameObject.Find("endGameCanvas").GetComponent<Canvas>().enabled = true;
                    GameObject.Find("endGameCanvas/whoWon").GetComponent<Text>().text = "Crew mates Won!!!";
                    instance.MyPlayer.gameOver = true;
                }
            }
        }
    }
    public void EjectPlayer(Player playerToEject) 
    {
        playerToEject.isAlive = false;
        playerToEject.pressedEmergancy = true;
        playerToEject.Apear();
        playerToEject.playerObject.GetComponent<PolygonCollider2D>().isTrigger = true;
    }
    public Player GetPlayerbyId(int id)
    {
        for (int i = 0; i < instance.otherPlayers.Length; i++)
        {
            if (instance.otherPlayers[i].id == id)
            {
                return instance.otherPlayers[i];
            }
        }
        if (instance.MyPlayer.id == id)
        {
            return instance.MyPlayer;
        }
        return null;
    }

    public string CheckWhoWon(Player [] otherPlayers, int doneTasks) 
    {
        string whoWon = "game in progress";
        int impostorsAmount = 0;
        int crewMateAmount = 0;
        Debug.Log(doneTasks + " all the done fsdf");
        Debug.Log(instance.otherPlayers.Length + " the lenght ffdfdsfsd");
        if (doneTasks == 7*(instance.otherPlayers.Length))
        {
            whoWon = "crewMates";
        }
        for (int i = 0; i < otherPlayers.Length; i++)
        {
            Player current = otherPlayers[i];
            if (current is Impostor && current.isAlive)
            {
                impostorsAmount++;
            }
            else if(current.isAlive)
            {
                crewMateAmount++;
            }
        }
        if (instance.MyPlayer is Impostor && instance.MyPlayer.isAlive)
        {
            impostorsAmount++;
        }
        else if (instance.MyPlayer.isAlive)
        {
            crewMateAmount++;
        }
        if (crewMateAmount <= impostorsAmount)
        {
            whoWon = "impostors";
        }
        else if (impostorsAmount == 0)
        {
            whoWon = "crewMates";
        }
        return whoWon;
    }
    public Vote MeetingResults(int[] votes) 
    {
        int maxVotes = int.MinValue;
        int place = -1;
        int prevMaxVotes = int.MinValue;
        bool tie = false;

        for (int i = 0; i < votes.Length; i++)
        {
            if (maxVotes <= votes[i])
            {
                prevMaxVotes = maxVotes;
                maxVotes = votes[i];
                place = i;
            }
        }

        if (prevMaxVotes == maxVotes)
        {
            tie = true;
        }

        Vote vote = new Vote();
        vote.tie = tie;
        vote.color = "";

        if (place == 0)
        {
            vote.color = "pink";
        }
        else if (place == 1)
        {
            vote.color = "brown";
        }
        else if (place == 2)
        {
            vote.color = "black";
        }
        else if (place == 3)
        {
            vote.color = "cyan";
        }
        else if (place == 4)
        {
            vote.color = "white";
        }
        else if (place == 5)
        {
            vote.color = "orange";
        }
        else if (place == 6)
        {
            vote.color = "yellow";
        }
        else if (place == 7)
        {
            vote.color = "blue";
        }
        else if (place == 8)
        {
            vote.color = "red";
        }
        else if (place == 9)
        {
            vote.color = "green";
        }
        else if (place == 10)
        {
            vote.color = "skip";
        }
        return vote;
    }
    public bool ContinueMeeting(int counterVotes) 
    {
        int counterAlivePlayers = 0;
        for (int i = 0; i < otherPlayers.Length; i++)
        {
            if (instance.otherPlayers[i].isAlive)
            {
                counterAlivePlayers++;
            }
        }
        if (instance.MyPlayer.isAlive)
        {
            counterAlivePlayers++;
        }
        return !(counterAlivePlayers == counterVotes);
    }
    public struct Vote 
    {
        public bool tie;
        public string color;
    }

    public void SetColorByID(Player[] players) 
    {
        string[] colorByID = { "blue", "red", "green", "yellow", "white", "black", "orange", "cyan" };
        Sprite sprite = Resources.Load<Sprite>("green");
        for (int i = 0; i < players.Length; i++)
        {
            players[i].color = colorByID[players[i].id];
            sprite = (Resources.Load<Sprite>(players[i].color));
            players[i].playerObject.GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }

    public Player ChangePosition(Player p, Vector3 pos)
    {
        p.playerObject.transform.position = pos;
        return p;
    }

    public void Contact() 
    {
        while (true)
        {
            MessageType messageHandler = ReceiveData();
            instance.DoThings.Enqueue(messageHandler);
        }
    }
}
public class MessageType 
{
    public string Event;
    public string type;
    public int id;

    public bool isImpostor;

    public int numberOfPlayers;

    public int[] playerList;

    public int senderID;

    public PlayerData playerData;

    public List<KeyPressed> buttons;

    public (Vector3,int) playerPositionId;

    public List<(Vector3,int)> playersPositionsId;

    public string votedFor;

    public int finishedTaskNumber;


    public MessageType() 
    {
        buttons = new List<KeyPressed>();
        playersPositionsId = new List<(Vector3, int)>();
    }
}

public struct KeyPressed
{
    public KeyCode key;
    public bool pressed;
}


public struct PlayerData
{
    public int id;
    public TransformData transform;
    public bool inVent;
    public bool isAlive;

    public void UpdatePlayer(Player p)
    {
        p.playerObject.transform.position = transform.position;
        p.playerObject.transform.rotation  =  transform.rotation;
        p.playerObject.transform.localScale = transform.scale;
        if (p is Impostor)
        {
            ((Impostor)p).inVent = this.inVent;
        }
        p.isAlive  =  this.isAlive;
    }

    public static PlayerData GetPlayerData(Player p)
    {
        PlayerData pd = new PlayerData();
        pd.id = p.id;
        pd.transform = TransformData.GetTransformData(p.playerObject.transform);
        pd.isAlive = p.isAlive;
        if(p is Impostor)
        {
            pd.inVent  =  ((Impostor)p).inVent;
        }
        else
        {
            pd.inVent  =  false;
        }
        return pd;
    }
}

public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public static TransformData GetTransformData(Transform t)
    {
        TransformData td = new TransformData();
        td.position = t.position;
        td.rotation = t.rotation;
        td.scale = t.localScale;
        return td;
    }
}
