using Homebrew;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewGame;

namespace NewGame
{

    public class NewWorld : MonoBehaviour
    {


        #region 周期函数

        private void Start()
        {
            _StartChunk();

        }

        private void Update()
        {
            _Update_Render();
        }

        #endregion


        #region 初始化Chunk

        [Foldout("Chunk设置", true)]
        [HideInInspector] public GameObject ChunksParent;

        //start引用
        void _StartChunk()
        {
            CreateChunk(new Vector3(0f, 0f, 0f));
            CreateChunk(new Vector3(1f, 0f, 0f));

            ChunksParent = new GameObject();
        }

        //创建Chunk
        void CreateChunk(Vector3 _vec)
        {
            NewChunk ChunkTemp = new NewChunk(this, _vec);
            
        }


        #endregion


        #region 渲染协程

        [Foldout("渲染协程", true)]
        [Header("渲染链表")] public Queue<NewChunk> RenderQueue;
        [Header("渲染一个的延迟")] public float RenderDelay = 1f;
        Coroutine RenderCoroutine;

        //引用Update
        void _Update_Render()
        {
            //检查渲染线程是否出问题
            if (RenderCoroutine == null)
            {
                RenderCoroutine = StartCoroutine(RenderIEnumerator());
            }
        }

        //渲染协程
        IEnumerator RenderIEnumerator()
        {
            while (true)
            {
                // 检查队列并渲染
                if (RenderQueue.TryDequeue(out NewChunk chunktemp))
                {
                    chunktemp.CreateMesh(); // 处理有效的队列项
                    yield return new WaitForSeconds(RenderDelay); // 延迟
                }
                else
                {
                    yield return null; // 如果队列为空，等待下一帧
                }
            }
        }




        #endregion


    }

}

