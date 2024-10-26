using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{

    #region Update

    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    // Update is called once per frame
    void Update()
    {
        CreateTNT();
    }


    #endregion


    #region TNT

    public GameObject TNTprefeb;
    public Vector3 emitPos;
    public bool CreateATNT;
    public GameObject Particle_Explosion;
    public void CreateTNT()
    {
        if (CreateATNT)
        {
            GameObject tnt = GameObject.Instantiate(TNTprefeb);
            tnt.GetComponent<Entity_TNT>().OnStartEntity(emitPos, false);

            CreateATNT = false;
        }
    }

    #endregion


}
