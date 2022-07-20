using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class buttonHandler : MonoBehaviour
{
    bool isVoted;
    public void Click(string color) 
    {
        Client client = Client.instance;
        MessageType message = new MessageType();
        if (client.GetPlayerbyId(client.myId).isAlive == false)
        {
            for (int i = 0; i < GameObject.Find("meetingBoard/buttons").transform.childCount; i++)
            {
                GameObject.Find("meetingBoard/buttons").transform.GetChild(i).gameObject.GetComponent<Button>().enabled = false;
            }
            isVoted = true;
        }
        else if (!isVoted)
        {

            for (int i = 0; i < GameObject.Find("meetingBoard/buttons").transform.childCount ; i++)
            {
                GameObject.Find("meetingBoard/buttons").transform.GetChild(i).gameObject.GetComponent<Button>().enabled = false;
            }

            message.type = "voting";
            message.votedFor = color;
            isVoted = true;

            client.SendData(message);
        }
    }
    public void QuitGame() 
    {
        Application.Quit();
    }
}
