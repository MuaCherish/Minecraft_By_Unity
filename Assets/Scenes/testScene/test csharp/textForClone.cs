//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;

//public class textForClone : MonoBehaviour
//{
//    public World world;
//    public byte blocktype;
//    public Vector3 position;

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Mouse0))
//        {
//            CreateDropBox(position, blocktype);
//        }
//    } 

//    //创造掉落物(坐标,类型)
//    void CreateDropBox(Vector3 _pos, byte _blocktype)
//    {
//        //创建父类
//        GameObject DropBlock = new GameObject("DropBlock");
//        DropBlock.AddComponent<FloatingCube>();
//        DropBlock.transform.position = _pos;

//        //有贴图用贴图，没贴图用icon
//        if (world.blocktypes[_blocktype].sprite != null)
//        {
//            //Top
//            GameObject _Top = new GameObject("Top");
//            _Top.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].top_sprit;
//            _Top.transform.SetParent(DropBlock.transform);
//            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
//            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

//            //Buttom
//            GameObject _Buttom = new GameObject("Buttom");
//            _Buttom.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].top_sprit;
//            _Buttom.transform.SetParent(DropBlock.transform);
//            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
//            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

//            //Left
//            GameObject _Left = new GameObject("Left");
//            _Left.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Left.transform.SetParent(DropBlock.transform);
//            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
//            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

//            //Right
//            GameObject _Right = new GameObject("Right");
//            _Right.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Right.transform.SetParent(DropBlock.transform);
//            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
//            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

//            //Forward
//            GameObject _Forward = new GameObject("Forward");
//            _Forward.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Forward.transform.SetParent(DropBlock.transform);
//            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
//            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

//            //Back
//            GameObject _Back = new GameObject("Back");
//            _Back.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Back.transform.SetParent(DropBlock.transform);
//            _Back.transform.localPosition = new Vector3(0, 0.08f, -0.08f);
//            _Back.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
//        }
//        else
//        {
//            //Top
//            GameObject _Top = new GameObject("Top");
//            _Top.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Top.transform.SetParent(DropBlock.transform);
//            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
//            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

//            //Buttom
//            GameObject _Buttom = new GameObject("Buttom");
//            _Buttom.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Buttom.transform.SetParent(DropBlock.transform);
//            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
//            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

//            //Left
//            GameObject _Left = new GameObject("Left");
//            _Left.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Left.transform.SetParent(DropBlock.transform);
//            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
//            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

//            //Right
//            GameObject _Right = new GameObject("Right");
//            _Right.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Right.transform.SetParent(DropBlock.transform);
//            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
//            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

//            //Forward
//            GameObject _Forward = new GameObject("Forward");
//            _Forward.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Forward.transform.SetParent(DropBlock.transform);
//            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
//            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

//            //Back
//            GameObject _Back = new GameObject("Back");
//            _Back.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
//            _Back.transform.SetParent(DropBlock.transform);
//            _Back.transform.localPosition = new Vector3(0, 0.08f, -0.08f);
//            _Back.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
//        }

        


//    }

//}
