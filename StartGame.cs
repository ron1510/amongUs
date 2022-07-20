using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts;

public class StartGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void TaskOnClick()
    {
        SceneManager.LoadScene(1);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
