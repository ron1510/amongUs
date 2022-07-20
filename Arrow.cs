using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System.Net;
using System.Net.Sockets;

public class Arrow : MonoBehaviour
{
    public List<GameObject> arrows = new List<GameObject>();
    public Vent vent;
    public Impostor player;

    public void OnMouseDown()
    {
        Client client = Client.instance;
        MessageType message = new MessageType();
        player.Travel(vent);
        for (int i = 0; i < arrows.Count; i++)
        {
            Destroy(arrows[i]);
        }
        message.type = "travelToOtherVent";
        message.playerPositionId = (player.playerObject.transform.position, player.id);
        client.SendData(message);
    }
}
