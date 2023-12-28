using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandShake : MonoBehaviour
{

    public GameObject FirstCamera;
    PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = FirstCamera.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

        //if (playerController.HandShake)
        //{
        //    Debug.Log("Moving!");
        //}
        //else
        //{
        //    Debug.Log("Stop");
        //}


    }
}
