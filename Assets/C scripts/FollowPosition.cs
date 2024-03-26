using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{

    public Transform Target;

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position = Target.position;
    }

    private void OnEnable()
    {
        GetComponent<ParticleSystem>().Play();
    }

    private void OnDisable()
    {
        GetComponent<ParticleSystem>().Stop();
    }

}
