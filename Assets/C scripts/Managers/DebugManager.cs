using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    //获取组件
    [Header("Transforms")]
    public GameObject DebugScreen;
    public TextMeshProUGUI textMeshPro;
    public Player player;
    public World world;

    [Header("状态")]
    public bool isDebug = false;



    void Update()
    {

        if (world.game_state == Game_State.Playing)
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                isDebug = !isDebug;
                DebugScreen.SetActive(!DebugScreen.activeSelf);
            }
        }

    }

    private void FixedUpdate()
    {
        if (isDebug)
        {
            UpdateScreen();
        }
    }



    void UpdateScreen()
    {
        Vector3 footlocation = world.PlayerFoot.position;

        //FPS
        CalculateFPS();


        //update
        //textMeshPro.text += $"\n";
        textMeshPro.text = $"FPS: {Mathf.Ceil(fps):F2}\n";
        textMeshPro.text += $"\n";
        textMeshPro.text += $"[Player]\n";
        textMeshPro.text += $"Facing: {CalculateFacing()}\n";
        textMeshPro.text += $"Input: <{player.verticalInput}, {player.horizontalInput}>\n";
        textMeshPro.text += $"VerticleMoment: {player.verticalMomentum}\n";
        textMeshPro.text += $"RealPosition: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        textMeshPro.text += $"RelaPosition: {world.GetRelalocation(footlocation)}\n";
        textMeshPro.text += $"EditNumber: {world.EditNumber.Count}\n";
        textMeshPro.text += $"\n";
        textMeshPro.text += $"[Chunk]\n";
        textMeshPro.text += $"Position: {world.GetChunkLocation(footlocation)}\n";



    }



    //calculate FPS
    private int count;
    private float deltaTime;
    private float fps;
    void CalculateFPS()
    {
        count++;
        deltaTime += Time.deltaTime;
        if (count % 60 == 0)
        {
            count = 1;
            fps = 60f / deltaTime;
            deltaTime = 0;
        }
    }


    //facing
    string CalculateFacing()
    {
        Vector3 forward = player.transform.forward;
        float angle = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        string facingDirection;

        if (Mathf.Abs(angle - 0) <= 30 || Mathf.Abs(angle - 360) <= 30)
        {
            facingDirection = "East";
        }
        else if (Mathf.Abs(angle - 90) <= 30)
        {
            facingDirection = "North";
        }
        else if (Mathf.Abs(angle - 180) <= 30)
        {
            facingDirection = "West";
        }
        else if (Mathf.Abs(angle - 270) <= 30)
        {
            facingDirection = "South";
        }
        else if (Mathf.Abs(angle - 45) <= 30)
        {
            facingDirection = "NorthEast";
        }
        else if (Mathf.Abs(angle - 135) <= 30)
        {
            facingDirection = "SouthEast";
        }
        else if (Mathf.Abs(angle - 225) <= 30)
        {
            facingDirection = "SouthWest";
        }
        else if (Mathf.Abs(angle - 315) <= 30)
        {
            facingDirection = "NorthWest";
        }
        else
        {
            facingDirection = "Unknown";
        }

        return facingDirection;
    }

}
