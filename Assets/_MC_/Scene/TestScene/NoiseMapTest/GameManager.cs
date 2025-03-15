using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoiseMapTest
{
    public class GameManager : MonoBehaviour
    {

        #region CycleLife

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            GenChunk.RenderChunks();
        }

        private void Update()
        {
            _ReferUpdate_Handle_Drag();
            _ReferUpdate_Handle_Destroy();
            _ReferUpdate_Handle_Scale();
            _ReferUpdate_ChunkReLoad();
        }

        #endregion


        #region Drag and Scale

        [Foldout("Drag and Scale", true)]
        [Header("��ק������")] public float moveSpeed = 0.1f;
        [Header("����������")] public float ScrollSensitivity = 2f;
        [Header("���ŷ�Χ")] public Vector2 ScaleRange = new Vector2(5f, 10f);
        private float currentScale = 5f;
        private bool isDragging = false;
        private Camera mainCamera;

        void _ReferUpdate_Handle_Drag()
        {
            // ��������Ҽ���������겢������קģʽ
            if (Input.GetMouseButtonDown(1)) // �Ҽ�
            {
                MC_Static_Unity.LockMouse(true);
                isDragging = true;
            }

            // �ɿ�����Ҽ������� RenderChunks ���ָ����
            if (Input.GetMouseButtonUp(1)) // �Ҽ�
            {
                MC_Static_Unity.LockMouse(false);
                isDragging = false;
                GenChunk.RenderChunks();
            }

            // �� XOY ƽ���϶����
            if (isDragging)
            {
                float moveX = -Input.GetAxis("Mouse X") * moveSpeed;
                float moveY = -Input.GetAxis("Mouse Y") * moveSpeed;

                // ������ƶ���ת��Ϊ���������ƶ�
                Vector3 move = new Vector3(moveX, moveY, 0);
                mainCamera.transform.position += mainCamera.transform.rotation * move;
            }
        }

        void _ReferUpdate_Handle_Scale()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // ��ȡ���������� (-1 ~ 1)

            if (Mathf.Abs(scrollInput) > 0.01f) // ���⼫Сֵ����
            {
                currentScale -= scrollInput * ScrollSensitivity; // ��ֵ������С
                currentScale = Mathf.Clamp(currentScale, ScaleRange.x, ScaleRange.y); // ���Ʒ�Χ
            }

            // ֱ���޸������������ͶӰ�ߴ�
            if (mainCamera != null)
            {
                mainCamera.orthographicSize = currentScale;
            }
        }


        #endregion


        #region Noise

        [Foldout("Noise", true)]
        [Header("�����Ŵ�ϵ��")] public float noiseScale;
        [Header("�˶����׵�����")][Range(1, 12)] public int octaves;
        [Header("�־���")][Range(0, 1)] public float persistance;
        [Header("��϶��")] public float lacunarity;
        [Header("����")] public int seed;
        [Header("����ƫ����")] public Vector2 offset;

        #endregion


        #region Reload

        [Foldout("Reload", true)]
        [Header("��������")] public bool ApplySetting;


        void _ReferUpdate_ChunkReLoad()
        {
            if (ApplySetting)
            {
                ChunkReLoad();
                ApplySetting = false;
            }
        }

        void ChunkReLoad()
        {

            GenChunk.GetAllChunksObjct(out var _Objs);

            foreach (var item in _Objs)
            {
                AddToDestroy(item);
            }

            GenChunk.ClearAllChunks();

            GenChunk.RenderChunks();

        }


        #endregion


        #region Destroy

        private List<GameObject> WaitToDestroy = new List<GameObject>();

        void _ReferUpdate_Handle_Destroy()
        {
            if (WaitToDestroy.Count > 0)
            {
                for (int i = WaitToDestroy.Count - 1; i >= 0; i--)
                {
                    Destroy(WaitToDestroy[i]);
                    WaitToDestroy.RemoveAt(i);
                }
            }
        }

        public void AddToDestroy(GameObject _obj)
        {
            WaitToDestroy.Add(_obj);
        }


        #endregion

    }
}
