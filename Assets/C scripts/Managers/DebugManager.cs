using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Animations.AimConstraint;

public class DebugManager : MonoBehaviour
{
    //获取组件
    [Header("Transforms")]
    public GameObject DebugScreen;
    public Transform Content;
    public GameObject blockitem;
    public TextMeshProUGUI LeftText;
    public Player player;
    public World world;

    [Header("状态")]
    public bool isDebug = false;

    private void Start()
    {
        UpdateBlockItem();
    }

    void Update()
    {

        if (world.game_state == Game_State.Start)
        {
            if (DebugScreen.activeSelf)
            {
                DebugScreen.SetActive(false);
            }
            

        }


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
        //LeftText.text += $"\n";
        LeftText.text = $"FPS: {Mathf.Ceil(fps):F2}\n";

        LeftText.text += $"\n";
        LeftText.text += $"[Player]\n";
        LeftText.text += $"Facing: {player.Facing}\n";
        LeftText.text += $"Input: <{player.verticalInput}, {player.horizontalInput}>\n";
        LeftText.text += $"VerticleMoment: {player.verticalMomentum}\n";
        LeftText.text += $"RealPosition: {footlocation}\n";
        LeftText.text += $"RelaPosition: {world.GetRelalocation(footlocation)}\n";
        LeftText.text += $"EditNumber: {world.EditNumber.Count}\n";

        LeftText.text += $"\n";
        LeftText.text += $"[Chunk]\n";
        LeftText.text += $"Position: {world.GetChunkLocation(footlocation)}\n";
         
        

    }


    public void UpdateBlockItem()
    {
        

        for (int index = 0;index < world.blocktypes.Length; index++)
        {
            //初始化item
            GameObject instance = Instantiate(blockitem);
            instance.transform.SetParent(Content, false);
            instance.transform.Find("TMP_index").GetComponent<TextMeshProUGUI>().text = $"{index}";
            instance.transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 200f / 255);

            if (world.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = world.blocktypes[index].sprite;
            }else if (world.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = world.blocktypes[index].top_sprit;
            }
            else
            {
                instance.transform.Find("Image").GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
            
        }

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
