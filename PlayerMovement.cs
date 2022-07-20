using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System.Net;
using System.Net.Sockets;


public class PlayerMovement : MonoBehaviour
{
    int buttonNumber = 0;
    public Player player;
    [SerializeField] private float speed = 5;
    [SerializeField] private bool isImpostor;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private float horizontalInput;
    private float verticalInput;
    public Player[] players;
    float currentHTime;
    float currentVTime;
    bool isVenting = false;
    public void Start()
    {
    }


    public void Initialize(Player p, bool isImpostor, Player[] otherPlayers, float _speed)
    {
        player = p;
        players = otherPlayers;
        this.isImpostor = isImpostor;
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        speed = _speed;

    }
    public void Update()
    {
        Client client = Client.instance;
        MessageType message = new MessageType();

        if (!player.meetingInProgress && !player.gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) && isImpostor && ((Impostor)player).killCooldown.currentTime <= 0)
            {
                message.buttons.Add(new KeyPressed { key = KeyCode.Space, pressed = true });
                for (int i = 0; i < players.Length; i++)
                {
                    Player checkPlayer;
                    if (players[i].playerObject.GetComponent<Move>() != null)
                        checkPlayer = players[i].playerObject.GetComponent<Move>().player;
                    else
                        checkPlayer = players[i].playerObject.GetComponent<PlayerMovement>().player;
                    if (((Impostor)player).Kill(checkPlayer))
                    {
                        ((Impostor)player).killCooldown.currentTime = ((Impostor)player).killCooldown.startingTime;
                        message.type = "killAction";
                        message.id = checkPlayer.id;
                        message.senderID = player.id;
                        string deadPlayerColor = checkPlayer.color+"Dead";
                        GameObject.Find(deadPlayerColor).transform.position = checkPlayer.playerObject.transform.position;
                        GameObject.Find(deadPlayerColor).GetComponent<SpriteRenderer>().enabled = true;
                        client.SendData(message);
                        break;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.V) && isImpostor)
            {
                isVenting = true;
                ((Impostor)player).HopToVent();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                horizontalInput = -1;
                currentHTime = Time.realtimeSinceStartup;
                message.buttons.Add(new KeyPressed { key = KeyCode.A, pressed = true });
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                verticalInput = -1;
                currentVTime = Time.realtimeSinceStartup;
                message.buttons.Add(new KeyPressed { key = KeyCode.S, pressed = true });
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                horizontalInput = 1;
                currentHTime = Time.realtimeSinceStartup;
                message.buttons.Add(new KeyPressed { key = KeyCode.D, pressed = true });
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                verticalInput = 1;
                currentVTime = Time.realtimeSinceStartup;
                message.buttons.Add(new KeyPressed { key = KeyCode.W, pressed = true });
            }
            if (Input.GetKeyDown(KeyCode.M) && !player.pressedEmergancy)
            {
                player.pressedEmergancy = true;
                player.meetingInProgress = true;
                message.type = "startMeeting";
                player.Meeting(players);
                client.SendData(message);
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                buttonNumber++;
                message.buttons.Add(new KeyPressed { key = KeyCode.A, pressed = false });
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                buttonNumber++;
                message.buttons.Add(new KeyPressed { key = KeyCode.S, pressed = false });
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                buttonNumber++;
                message.buttons.Add(new KeyPressed { key = KeyCode.D, pressed = false });
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                buttonNumber++;
                message.buttons.Add(new KeyPressed { key = KeyCode.W, pressed = false });
            }
            message.type = "buttonPressed";
            message.id = player.id;
            if (message.buttons.Count > 0)
                client.SendData(message);

            if (buttonNumber >= 5 && !client.isHost)
            {
                message.type = "updatePositions";
                client.SendData(message);
                buttonNumber = 0;
            }
        }
    }
    public void FixedUpdate()
    {
        if (isImpostor)
        {
            ((Impostor)player).killCooldown.currentTime -= 1 * Time.deltaTime;
        }

        body.velocity = new Vector2(0, 0);

        float zeroTime;
        zeroTime = Time.realtimeSinceStartup;

        if (currentHTime != -1 && zeroTime - currentHTime >= 0.1f)
        {
            horizontalInput = 0;
            currentHTime = -1;
        }
        if (currentVTime != -1 && zeroTime - currentVTime >= 0.1f)
        {
            verticalInput = 0;
            currentVTime = -1;
        }


        if (horizontalInput > 0.01f)
        {
            body.transform.localScale = new Vector3(1,1,1);
        }
        else if (horizontalInput < -0.01f)
        {
            body.transform.localScale = new Vector3(-1, 1, 1);
        }

        if ((isImpostor && !((Impostor)player).inVent) || !isImpostor) 
        {
            body.velocity = new Vector2(horizontalInput * speed, verticalInput * speed);
        }
        GameObject.Find("Main Camera").GetComponent<Transform>().position = GetComponent<Transform>().position;
        Vector3 posCamera = GameObject.Find("Main Camera").GetComponent<Transform>().position;
        GameObject.Find("Main Camera").GetComponent<Transform>().position = new Vector3(posCamera.x, posCamera.y, -2);
        player.playerObject.transform.rotation = Quaternion.Euler(0,0,0);
        body.angularVelocity = 0f;
    }
}
