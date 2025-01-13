using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityDeadTest : MonoBehaviour
{
    Animation ani;

    // Start is called before the first frame update
    void Start()
    {
        ani = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (ani != null && ani.GetClip("EntityDead") != null)
            {
                ani.Play("EntityDead");
            }
            else
            {
                print("’“≤ªµΩ");
            }

           
        }
        


    }
}
