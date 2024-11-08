using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// ��Դ�����࣬��Ҫ����
/// </summary>
public class OnLoadResource : MonoBehaviour
{
    public static OnLoadResource Data { get; private set; }

    // ���ڴ洢���ص� MusicBase ��Դ
    public AudioClip[] GlobalClips { get; private set; }

    private void Awake()
    {
        
        // ȷ��ֻ��һ��ʵ������
        if (Data != null && Data != this)
        {
            Destroy(gameObject);
            return;
        }
        Data = this;

        // ����Ϊ�������٣����������ڳ���֮�䱣��
        //DontDestroyOnLoad(gameObject);

        // ������Դ
        LoadGoods();
    }

    private void LoadGoods()
    {
        // ��ȷ�ļ���·���������� Assets �� .asset ��׺
        //var musicBase = Resources.Load<MusicBase>("OnLoadMusic/GlobalClips");

        //// ȷ�����ص��Ķ����ǿ�
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
