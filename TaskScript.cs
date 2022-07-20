using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskScript : MonoBehaviour
{    
    public int counter = 0;
    public void Task(int taskNumber) 
    {
        Client client = Client.instance;
        MessageType message = new MessageType();
        counter++;
        if (counter == 10)
        {
            GameObject.Find("tasksCanvas/tasks/task" + taskNumber).gameObject.GetComponent<Button>().enabled = false;
            message.type = "finishTask";
            message.finishedTaskNumber = taskNumber;
            client.SendData(message);
        }
    }
}
