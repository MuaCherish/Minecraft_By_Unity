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
    [Header("״̬")]
    [ReadOnly] public bool isDebug = false;

    //��ȡ���
    [Header("����")]
    public ManagerHub managerHub;
    public GameObject DebugScreen;
    public Transform Content;
    public GameObject blockitem;
    public TextMeshProUGUI LeftText;
    //public Player player;
    //public World world;

    

    private void Start()
    {
        UpdateBlockItem();
    }

    void Update()
    {

        if (managerHub.world.game_state == Game_State.Start)
        {
            if (DebugScreen.activeSelf)
            {
                DebugScreen.SetActive(false);
            }
            

        }


        if (managerHub.world.game_state == Game_State.Playing)
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
        Vector3 footlocation = managerHub.world.PlayerFoot.position;

        //FPS
        CalculateFPS();


        //update
        //LeftText.text += $"\n";
        LeftText.text = $"֡��: {managerHub.fpsmManaer.fps:F2}\n";
        LeftText.text += $"��ǰʱ��: {managerHub.timeManager.GetCurrentTime():F2}ʱ\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Player]\n";
        LeftText.text += $"����: {CalculateFacing()}\n";
        LeftText.text += $"ʵ�ʳ���: {managerHub.player.FactFacing}\n";
        LeftText.text += $"ʵ���˶�����: {managerHub.player.ActualMoveDirection}\n";
        LeftText.text += $"����: {managerHub.player.keyInput}\n";
        LeftText.text += $"�۾�����: {managerHub.player.cam.position}\n";
        LeftText.text += $"ʵʱ����: {managerHub.player.verticalMomentum}\n";
        LeftText.text += $"��������: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        LeftText.text += $"�������: {managerHub.world.GetRelalocation(footlocation)}\n";
        LeftText.text += $"�ѱ��淽������: {managerHub.world.EditNumber.Count}\n";
        LeftText.text += $"��ײ�������:{managerHub.player.CollisionNumber}\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Chunk]\n";
        LeftText.text += $"��������: {managerHub.world.GetChunkLocation(footlocation)}\n";
        LeftText.text += $"\n";
        //LeftText.text += $"[Noise]\n";


    }


    public void UpdateBlockItem()
    {
        

        for (int index = 0;index < managerHub.world.blocktypes.Length; index++)
        {
            //��ʼ��item
            GameObject instance = Instantiate(blockitem);
            instance.transform.SetParent(Content, false);
            instance.transform.Find("TMP_index").GetComponent<TextMeshProUGUI>().text = $"{index}";
            instance.transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 200f / 255);

            if (managerHub.world.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = managerHub.world.blocktypes[index].sprite;
            }else if (managerHub.world.blocktypes[index].sprite != null)
            {
                instance.transform.Find("Image").GetComponent<Image>().sprite = managerHub.world.blocktypes[index].top_sprit;
            }
            else
            {
                instance.transform.Find("Image").GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
            
        }

    }



    // FPS������
    private int frameCount;
    private float elapsedTime;
    private float fps;

    // ���²�����FPS
    void CalculateFPS()
    {
        // ÿ֡�ۼ�ʱ��
        elapsedTime += Time.deltaTime;
        frameCount++;

        // ÿ�����һ��FPS
        if (elapsedTime >= 1.0f)
        {
            // ����FPS
            fps = frameCount / elapsedTime;

            // ���ü������� elapsedTime
            frameCount = 0;
            elapsedTime = 0f;
        }
    }







    //facing
    string CalculateFacing()
    {
        Vector3 forward = managerHub.player.transform.forward;
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
