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


        #region ���ں���

        private void Start()
        {
            _StartChunk();

        }

        private void Update()
        {
            _Update_Render();
        }

        #endregion


        #region ��ʼ��Chunk

        [Foldout("Chunk����", true)]
        [HideInInspector] public GameObject ChunksParent;

        //start����
        void _StartChunk()
        {
            CreateChunk(new Vector3(0f, 0f, 0f));
            CreateChunk(new Vector3(1f, 0f, 0f));

            ChunksParent = new GameObject();
        }

        //����Chunk
        void CreateChunk(Vector3 _vec)
        {
            NewChunk ChunkTemp = new NewChunk(this, _vec);
            
        }


        #endregion


        #region ��ȾЭ��

        [Foldout("��ȾЭ��", true)]
        [Header("��Ⱦ����")] public Queue<NewChunk> RenderQueue;
        [Header("��Ⱦһ�����ӳ�")] public float RenderDelay = 1f;
        Coroutine RenderCoroutine;

        //����Update
        void _Update_Render()
        {
            //�����Ⱦ�߳��Ƿ������
            if (RenderCoroutine == null)
            {
                RenderCoroutine = StartCoroutine(RenderIEnumerator());
            }
        }

        //��ȾЭ��
        IEnumerator RenderIEnumerator()
        {
            while (true)
            {
                // �����в���Ⱦ
                if (RenderQueue.TryDequeue(out NewChunk chunktemp))
                {
                    chunktemp.CreateMesh(); // ������Ч�Ķ�����
                    yield return new WaitForSeconds(RenderDelay); // �ӳ�
                }
                else
                {
                    yield return null; // �������Ϊ�գ��ȴ���һ֡
                }
            }
        }




        #endregion


    }

}

