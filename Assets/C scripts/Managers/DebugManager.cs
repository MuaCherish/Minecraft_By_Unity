using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static UnityEngine.Animations.AimConstraint;

public class DebugManager : MonoBehaviour
{


    #region ״̬

    [Header("״̬")]
    [ReadOnly] public bool isDebug = false;

    #endregion


    #region ���ں���

    private ManagerHub managerHub;
    private void Start()
    {
        managerHub = VoxelData.GetManagerhub();
    }

    private void FixedUpdate()
    {
        if (isDebug)
        {
            UpdateScreen();
        }
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
        else if (managerHub.world.game_state == Game_State.Playing)
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                isDebug = !isDebug;
                DebugScreen.SetActive(!DebugScreen.activeSelf);
            }


        }



    }


    #endregion


    #region ������Ļ

    public Camera FirstPersonCamera;
    public GameObject DebugScreen;
    public TextMeshProUGUI LeftText;
    public TextMeshProUGUI RightText;




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
        LeftText.text += $"�ٶ�: {managerHub.player.velocity}\n";
        LeftText.text += $"����: {CalculateFacing()}\n";
        LeftText.text += $"ʵ�ʳ���: {managerHub.player.FactFacing}\n";
        LeftText.text += $"ʵ���˶�����: {managerHub.player.ActualMoveDirection}\n";
        //LeftText.text += $"�µ��˶�����: {managerHub.player.momentum}\n";
        LeftText.text += $"����: {managerHub.player.keyInput}\n";
        LeftText.text += $"�۾�����: {managerHub.player.cam.position}\n";
        LeftText.text += $"ʵʱ����: {managerHub.player.verticalMomentum}\n";
        LeftText.text += $"��������: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        LeftText.text += $"�������: {managerHub.world.GetRelalocation(footlocation)}\n";
        LeftText.text += $"�ѱ��淽������: {managerHub.world.EditNumber.Count}\n";
        LeftText.text += $"��ײ�������:{managerHub.player.CollisionNumber}\n";
        //LeftText.text += $"����ģʽ����߹���·��: {managerHub.player.accumulatedDistance:F2}m\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Chunk]\n";
        LeftText.text += $"��������: {managerHub.world.GetChunkLocation(footlocation)}\n";
        LeftText.text += $"��ʼ������ƽ����Ⱦʱ��: {(managerHub.world.OneChunkRenderTime * 1000f):F3}ms\n";
        LeftText.text += $"\n";
        //LeftText.text += $"[Noise]\n";




        //RightText.text += $"\n";
        RightText.text = $"[System]\n";
        RightText.text += $"ʵʱ��Ⱦ����: {CameraOnPreRender()}��\n";
        

    }

    

    #endregion


    #region DEBUG-����FPS

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


    #endregion


    #region DEBUG-����String����

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

    #endregion


    #region DEBUG-Camera��Ⱦ����

    //��Ⱦ����
    private int CameraOnPreRender()
    {
        int triangleCount = 0;

        // ��ȡ�����е����� MeshRenderer
        MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            // ��������Ƿ��������Ұ��
            if (meshRenderer.isVisible)
            {
                // ��ȡ MeshFilter ���
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // �ۼ���������
                    triangleCount += meshFilter.sharedMesh.triangles.Length / 3; // ÿ����������3���������
                }
            }
        }

        return triangleCount;
    }

    #endregion


}
