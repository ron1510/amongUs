using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System.Net;
using System.Net.Sockets;

public class Move : MonoBehaviour
{
    public Player player;
    private float speed = 5;
    private bool isImpostor;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body;
    private float horizontalInput;
    private float verticalInput;
    public Player[] players;
    float currentHTime = -1;
    float currentVTime = -1;
    List<KeyPressed> buttons;

    
    public void Initialize(Player p, bool isImpostor, Player[] otherPlayers, float _speed)
    {
        player = p;
        players = otherPlayers;
        for (int i = 0; i < otherPlayers.Length; i++)
        {
            Debug.Log(otherPlayers[i]);
        }
        this.isImpostor = isImpostor;
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        speed = _speed;
        buttons = new List<KeyPressed>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.buttons==null||this.buttons.Count==0)
        {
            //body.velocity = new Vector2(0, 0);
        }
        if ((isImpostor && !((Impostor)player).inVent) || !isImpostor) 
        {
            body.velocity = new Vector2(horizontalInput * speed, verticalInput * speed);
        }
        player.playerObject.transform.rotation = Quaternion.Euler(0,0,0);
        body.angularVelocity=0;
        float zeroTime = Time.realtimeSinceStartup;
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
    }

    public void UpdateMovement(List<KeyPressed> buttons)
    {
        this.buttons = buttons;
        if (isImpostor)
        {
            ((Impostor)player).killCooldown.currentTime -= 1 * Time.deltaTime;
        }

        //body.velocity = new Vector2(0, 0);
        for(int i=0; i<buttons.Count; i++)
        {
            //if ((buttons[i].key==(KeyCode.V)&&buttons[i].pressed && isImpostor) && ((Impostor)player).inVent)
            //{
               // ((Impostor)player).HopToVent();
            //}
            
            
            if ((buttons[i].key==(KeyCode.D)&&buttons[i].pressed))
            {
                body.transform.localScale = new Vector3(1,1,1);
            }
            else if ((buttons[i].key==(KeyCode.A)&&buttons[i].pressed))
            {
                body.transform.localScale = new Vector3(-1, 1, 1);
            }
            
            if((buttons[i].key==(KeyCode.A)&&buttons[i].pressed))
            {
                horizontalInput=-1;
                currentHTime = Time.realtimeSinceStartup;
            }
            if((buttons[i].key==(KeyCode.D)&&buttons[i].pressed))
            {
                horizontalInput=1;
                currentHTime = Time.realtimeSinceStartup;
            }
            
            if ((buttons[i].key==(KeyCode.W)&&buttons[i].pressed))
            {
                verticalInput = 1;
                currentVTime = Time.realtimeSinceStartup;
            }
            if ((buttons[i].key==(KeyCode.S)&&buttons[i].pressed))
            {
                verticalInput = -1;
                currentVTime = Time.realtimeSinceStartup;
            }
            
        }
        this.buttons=null;
    }
}
