using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    
    public class Player
    {
        public string playerVoted;
        public bool meetingInProgress;
        public bool isAlive;
        public bool voted;
        public string color;
        public bool pressedEmergancy;
        public GameObject playerObject;
        public int id;
        public bool gameOver;

        public Player(GameObject gameObject) 
        {
            isAlive = true;
            voted = false;
            pressedEmergancy = false;
            meetingInProgress = false;
            gameOver = false;
            playerObject = gameObject;
        }

        public void Apear() 
        {
            this.playerObject.GetComponent<SpriteRenderer>().enabled = this.isAlive;
        }

        public void Meeting(Player[] otherPlayers) 
        {
            GameObject.Find("meetingBoard").GetComponent<Canvas>().enabled = true;

            for (int i = 0; i < otherPlayers.Length; i++)
            {
                Player player = otherPlayers[i];
                GameObject.Find("meetingBoard/buttons").transform.Find("vote" + player.color + "Button").gameObject.SetActive(player.isAlive);
            }

            GameObject.Find("meetingBoard/buttons").transform.Find("vote" + this.color + "Button").gameObject.SetActive(this.isAlive);
            GameObject.Find("meetingBoard/buttons").transform.Find("SkipButton").gameObject.SetActive(true);

            for (int i = 0; i < GameObject.Find("meetingBoard/buttons").transform.childCount; i++)
            {
                GameObject.Find("meetingBoard/buttons").transform.GetChild(i).gameObject.GetComponent<Button>().enabled = true;
            }
        }
        public void EndMeeting(Player[] otherPlayers) 
        {
            GameObject.Find("meetingBoard").GetComponent<Canvas>().enabled = false;

            for (int i = 0; i < otherPlayers.Length; i++)
            {
                Player player = otherPlayers[i];
                GameObject.Find("meetingBoard/buttons").transform.Find("vote" + player.color + "Button").gameObject.SetActive(false);
            }

            GameObject.Find("meetingBoard/buttons").transform.Find("vote" + this.color + "Button").gameObject.SetActive(false);
            GameObject.Find("meetingBoard/buttons").transform.Find("SkipButton").gameObject.SetActive(false);
        }

    }
    public class Impostor:Player 
    {
        public List<GameObject> arrows;
        public CoolDown killCooldown;
        public CoolDown sabotageCooldown;
        public Sabotage sabotage;
        public bool inVent;
        public Vent currentVent;
        public Vent[] vents;



        public Impostor(GameObject gameObject) : base(gameObject) { this.killCooldown = new CoolDown(0, 5);
            this.sabotageCooldown = new CoolDown(0, 5);
            this.sabotage = new Sabotage();
            this.inVent = false;
            vents = Vent.GetVents();
        }
        public bool Kill(Player player) 
        {
            if (Vector2.Distance(base.playerObject.GetComponent<Transform>().position, player.playerObject.GetComponent<Transform>().position) < 10 && player.isAlive) 
            {
                player.isAlive = false;
                player.pressedEmergancy = true;
                this.killCooldown.startingTime = 10;
                player.Apear();
                return true;
            }
            return false;
        }
        public void KillForSure(Player player)
        {
            player.pressedEmergancy = true;
            player.isAlive = false;
            this.killCooldown.startingTime = 10;
            player.Apear();
        }
        public bool HopToVent() 
        {
            Client client = Client.instance;
            MessageType message = new MessageType();
            if (inVent) 
            {
                playerObject.GetComponent<SpriteRenderer>().enabled = true;
                playerObject.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y, -1);
                inVent = false;
                for (int i = 0; i < arrows.Count; i++)
                {
                    GameObject.Destroy(arrows[i]);
                }
                message.type = "exitVent";
                message.playerPositionId = (this.playerObject.transform.position, this.id);
                client.SendData(message);
                return false;
            }
            Vent bestVent = BestVent();
            if (bestVent != null) 
            {
                this.Travel(bestVent);
                playerObject.GetComponent<SpriteRenderer>().enabled = false;
                this.inVent = true;
                message.type = "enterVent";
                message.playerPositionId = (this.playerObject.transform.position, this.id);
                client.SendData(message);
                return true;
            }
            return false;
        }

        public void Travel(Vent vent) 
        {
            double angel=0;
            double x;
            double y;
            this.arrows = new List<GameObject>();
            base.playerObject.GetComponent<Transform>().position = vent.ventPos;
            if (playerObject.GetComponent<PlayerMovement>() != null)
            {
                for (int i = 0; i < Vent.travelToVents[vent.ventID - 1].Count; i++)
                {
                    GameObject arrow = GameObject.Instantiate((GameObject)Resources.Load("arrow"));
                    arrow.transform.position = new Vector3(vent.ventPos.x, vent.ventPos.y, -0.7f);
                    arrow.transform.localScale = new Vector3(0.055f, 0.055f, 0);
                    x = vent.ventPos.x - Vent.travelToVents[vent.ventID - 1][i].ventPos.x;
                    y = vent.ventPos.y - Vent.travelToVents[vent.ventID - 1][i].ventPos.y;
                    angel = (Math.Atan2(y, x) * 180 / Math.PI) - 180;
                    arrow.transform.position = new Vector3(vent.ventPos.x + (float)(Math.Cos(angel * Math.PI / 180)) * 0.7f, vent.ventPos.y + (float)Math.Sin(angel * Math.PI / 180) * 0.7f, -0.7f);
                    arrow.transform.rotation = Quaternion.Euler(0, 0, (float)angel);
                    arrow.AddComponent<BoxCollider2D>().isTrigger = true;
                    Arrow arrow1 = arrow.AddComponent<Arrow>();
                    arrow1.arrows = arrows;
                    arrow1.vent = Vent.travelToVents[vent.ventID - 1][i];
                    arrow1.player = this;
                    this.arrows.Add(arrow);
                }
            }

        }

        public Vent BestVent() 
        {
            float minD = float.MaxValue;
            int pos = -1;
            for (int i = 0; i < this.vents.Length; i++)
            {
                if (Vector2.Distance(vents[i].ventPos,playerObject.transform.position)<minD && Vector2.Distance(vents[i].ventPos, playerObject.transform.position) < 2)
                {
                    minD = Vector2.Distance(vents[i].ventPos, playerObject.transform.position);
                    pos = i;
                }
            }
            if (pos == -1)
            {
                return null;
            }
            return vents[pos];
        } 
    }
}
