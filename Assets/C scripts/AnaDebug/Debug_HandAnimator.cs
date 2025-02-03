using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_HandAnimator : MonoBehaviour
{
    Animator animator;
    public bool isMoving;
    public bool Toggle_isWave;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isMoving)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        if (Toggle_isWave)
        {
            animator.SetTrigger("isWave");
            Toggle_isWave = false;
        }

    }


}
