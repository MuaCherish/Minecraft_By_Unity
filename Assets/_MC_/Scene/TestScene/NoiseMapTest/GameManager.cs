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
        [Header("拖拽灵敏度")] public float moveSpeed = 0.1f;
        [Header("滚动灵敏度")] public float ScrollSensitivity = 2f;
        [Header("缩放范围")] public Vector2 ScaleRange = new Vector2(5f, 10f);
        private float currentScale = 5f;
        private bool isDragging = false;
        private Camera mainCamera;

        void _ReferUpdate_Handle_Drag()
        {
            // 按下鼠标右键，隐藏鼠标并启用拖拽模式
            if (Input.GetMouseButtonDown(1)) // 右键
            {
                MC_Static_Unity.LockMouse(true);
                isDragging = true;
            }

            // 松开鼠标右键，调用 RenderChunks 并恢复鼠标
            if (Input.GetMouseButtonUp(1)) // 右键
            {
                MC_Static_Unity.LockMouse(false);
                isDragging = false;
                GenChunk.RenderChunks();
            }

            // 在 XOY 平面拖动相机
            if (isDragging)
            {
                float moveX = -Input.GetAxis("Mouse X") * moveSpeed;
                float moveY = -Input.GetAxis("Mouse Y") * moveSpeed;

                // 将鼠标移动量转换为世界坐标移动
                Vector3 move = new Vector3(moveX, moveY, 0);
                mainCamera.transform.position += mainCamera.transform.rotation * move;
            }
        }

        void _ReferUpdate_Handle_Scale()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // 获取鼠标滚轮输入 (-1 ~ 1)

            if (Mathf.Abs(scrollInput) > 0.01f) // 避免极小值干扰
            {
                currentScale -= scrollInput * ScrollSensitivity; // 负值调整大小
                currentScale = Mathf.Clamp(currentScale, ScaleRange.x, ScaleRange.y); // 限制范围
            }

            // 直接修改摄像机的正交投影尺寸
            if (mainCamera != null)
            {
                mainCamera.orthographicSize = currentScale;
            }
        }


        #endregion


        #region Noise

        [Foldout("Noise", true)]
        [Header("噪声放大系数")] public float noiseScale;
        [Header("八度音阶的数量")][Range(1, 12)] public int octaves;
        [Header("持久性")][Range(0, 1)] public float persistance;
        [Header("间隙度")] public float lacunarity;
        [Header("种子")] public int seed;
        [Header("噪声偏移量")] public Vector2 offset;

        #endregion


        #region Reload

        [Foldout("Reload", true)]
        [Header("重新生成")] public bool ApplySetting;


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
