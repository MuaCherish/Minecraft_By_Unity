using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// 资源加载类，需要挂载
/// </summary>
public class OnLoadResource : MonoBehaviour
{
    public static OnLoadResource Data { get; private set; }

    // 用于存储加载的 MusicBase 资源
    public AudioClip[] GlobalClips { get; private set; }

    private void Awake()
    {
        
        // 确保只有一个实例存在
        if (Data != null && Data != this)
        {
            Destroy(gameObject);
            return;
        }
        Data = this;

        // 设置为不被销毁，这样可以在场景之间保留
        //DontDestroyOnLoad(gameObject);

        // 加载资源
        LoadGoods();
    }

    private void LoadGoods()
    {
        // 正确的加载路径，不包括 Assets 和 .asset 后缀
        //var musicBase = Resources.Load<MusicBase>("OnLoadMusic/GlobalClips");

        //// 确保加载到的对象不是空
        //if (musicBase != null)
        //{
        //    GlobalClips = musicBase.Clips;
        //    Debug.Log("Successfully loaded GlobalClips asset with clip count: " + GlobalClips.Length);
        //}
        //else
        //{
        //    Debug.LogWarning("Failed to load GlobalClips asset. Make sure it exists in Resources/OnLoadMusic.");
        //}
    }


     
}
