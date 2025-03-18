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
            _ReferUpdate_OnValidate();
            _ReferUpdate_OnGUIDebug();
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
        [Header("Mode")] public GenNoise.NormalizeMode normalizeMode;
        [Header("����ƫ����")] public Vector2 offset; private Vector2 offsetLastValue;

        void _ReferUpdate_OnValidate()
        {
            if (
                offset != offsetLastValue
                )
            {
                offsetLastValue = offset;
                ApplySetting = true;
            }
        }

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


        #region OnGUI

        private Rect windowRect = new Rect(50, 50, 250, 150); // ���ڳ�ʼλ�úʹ�С
        private float sliderValue1 = 5f;
        private float sliderValue2 = 5f;
        private bool showDebug = true;

        void _ReferUpdate_OnGUIDebug()
        {
            // �� F1 ��ʾ/���ص��Դ���
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showDebug = !showDebug;
            }
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private void OnGUI()
        {
            if (!showDebug) return;

            // ���ƴ��ڣ���ʹ����϶�
            windowRect = GUI.Window(0, windowRect, DrawDebugWindow, "���Դ���");
        }

        private void DrawDebugWindow(int windowID)
        {
            GUILayout.BeginVertical(); // ��ֱ���� Item

            // Item 1
            GUILayout.Label($"Item 1: {sliderValue1:F1}");
            sliderValue1 = GUILayout.HorizontalSlider(sliderValue1, 0, 10);

            // Item 2
            GUILayout.Label($"Item 2: {sliderValue2:F1}");
            sliderValue2 = GUILayout.HorizontalSlider(sliderValue2, 0, 10);

            GUILayout.EndVertical();

            // �����϶�����
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
#endif

        #endregion

    }
}
