using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private PlayerController playercontroller;
    public GameObject FirstCamera;
    string Block_Direction;

    void Start()
    {
        textMeshPro.font = Resources.Load<TMP_FontAsset>("Fonts/Roboto-Regular SDF");
        textMeshPro.fontSize = 20;
        textMeshPro.color = Color.black;

        playercontroller = FirstCamera.GetComponent<PlayerController>();
    }

    void Update()
    {
        GetBlockDirection();
        ShowText();
    }

    void GetBlockDirection()
    {
        switch (playercontroller.Face_flag)
        {
            case 0: Block_Direction = "ForWard"; break;
            case 1: Block_Direction = "Back"; break;
            case 2: Block_Direction = "Left"; break;
            case 3: Block_Direction = "Right"; break;
            case 4: Block_Direction = "Top"; break;
            //case 5:isGround≤ª¥¶¿Ì
            case 6: Block_Direction = "ForWard+Left"; break;
            case 7: Block_Direction = "ForWard+Right"; break;
            case 8: Block_Direction = "Back+Left"; break;
            case 9: Block_Direction = "Back+Right"; break;
        }
    }

    void ShowText()
    {
        //textMeshPro.text += $"\n";
        textMeshPro.text = $"PlayerInput:<{playercontroller.verticalInput},{playercontroller.horizontalInput}>\n";
        //textMeshPro.text += $"theta:{playercontroller.theta}\n";
        textMeshPro.text += $"BlockDirection:{Block_Direction}\n";
        textMeshPro.text += $"Ray.Length: {playercontroller.ray_length:F2}\n";
    }

}
