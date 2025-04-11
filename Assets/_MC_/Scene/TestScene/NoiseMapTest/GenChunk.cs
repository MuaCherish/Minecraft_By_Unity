
using System.Collections.Generic;

using UnityEngine;

namespace NoiseMapTest
{
    public static class GenChunk
    {
       
        #region GameObject

        private static GameObject _chunkParent;
        public static GameObject Chunkparent
        {
            get
            {
                if (_chunkParent == null)
                {
                    _chunkParent = GameObject.Find("Chunks");

                    // 如果场景中没有找到 "Chunks"，可以选择创建它，避免 NullReferenceException
                    if (_chunkParent == null)
                    {
                        _chunkParent = new GameObject("Chunks");
                    }
                }

                return _chunkParent;
            }
        }


        private static GameObject _mainCamera;
        public static GameObject MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

                    // 如果场景中没有找到 "MainCamera"，可以选择创建一个新的 Camera
                    if (_mainCamera == null)
                    {
                        _mainCamera = new GameObject("Main Camera");
                        _mainCamera.AddComponent<Camera>();
                        _mainCamera.tag = "MainCamera";
                    }
                }

                return _mainCamera;
            }
        }


        private static GameManager _gameManager;
        public static GameManager gameManager
        {
            get
            {
                if (_gameManager == null)
                {
                    _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                }

                return _gameManager;
            }
        }

        #endregion


        #region Render

        public static float RenderRange = 32f;
        
        public static void RenderChunks()
        {
            GetNewChunkPos(out var _result);

            // 生成新的区块
            foreach (var _pos in _result)
                GenerateChunk(_pos);

            // 记录待删除的区块
            List<Vector3> _WaitToDel = new List<Vector3>();

            // 遍历 AllChunks
            foreach (var item in AllChunks) 
            {
                float _dis = (item.Key - MainCamera.transform.position).magnitude;

                if (_dis > RenderRange)
                    _WaitToDel.Add(item.Key);
            }

            // 统一删除区块，防止遍历时修改集合 
            foreach (var _pos in _WaitToDel)
            {
                DeleteChunk(_pos);
            }
        }

        #endregion


        #region Camera

        //获取摄像机边界
        private static void GetCameraBounds(out Vector2 Xrange, out Vector2 Yrange)
        {
            Camera cam = Camera.main;
            float size = cam.orthographicSize;  // 获取正交尺寸
            float aspect = cam.aspect;          // 屏幕宽高比

            float width = size * 2 * aspect;    // 计算视口宽度
            float height = size * 2;            // 计算视口高度

            Vector3 CameraPos = cam.transform.position;

            // X 轴范围（左 -> 右）
            Xrange = new Vector2(CameraPos.x - width / 2, CameraPos.x + width / 2);
            // Y 轴范围（下 -> 上）
            Yrange = new Vector2(CameraPos.y - height / 2, CameraPos.y + height / 2);

            //进行向下取整
            Xrange = new Vector2(Mathf.Floor(Xrange.x), Mathf.Floor(Xrange.y));
            Yrange = new Vector2(Mathf.Floor(Yrange.x), Mathf.Floor(Yrange.y));
        }

        //获取基于摄像机视野内的未标记的所有的点
        private static void GetNewChunkPos(out List<Vector3> _posList)
        {
            _posList = new List<Vector3>();

            //处理Camera坐标
            Vector3 CameraPos = MainCamera.transform.position; CameraPos = new Vector3(Mathf.Floor(CameraPos.x), Mathf.Floor(CameraPos.y), 0);
            GetCameraBounds(out var Xrange, out var Yrange);

            //遍历所有点
            for (float _x = Xrange.x; _x <= Xrange.y; _x++)
            {
                for (float _y = Yrange.x; _y <= Yrange.y; _y++)
                {
                    Vector3 _thisPos = new Vector3(_x, _y, 0); 

                    //提前跳过-如果该区块已存在
                    if (AllChunks.ContainsKey(_thisPos))
                        continue;

                    _posList.Add(_thisPos);
                }
            }
        }

        #endregion


        #region AllChunks

        private static Dictionary<Vector3, GameObject> AllChunks = new Dictionary<Vector3, GameObject>();

        public static void ClearAllChunks()
        {
            AllChunks.Clear();
        }

        public static void GetAllChunksObjct(out List<GameObject> _Objs)
        {
            _Objs = new List <GameObject>();

            foreach (var item in AllChunks)
            {
                _Objs.Add(item.Value);
            }

        }

        #endregion


        #region Gen and Del

        private static float ChunkWidth = 1f;  //区块宽度
        private static int VoxelWidth = 16;    //像素宽度

        private static void GenerateChunk(Vector3 _pos)
        {
            // 创建一个新的 GameObject
            GameObject chunkObj = new GameObject($"Chunk_{_pos}");
            chunkObj.transform.SetParent(Chunkparent.transform, false);
            chunkObj.transform.position = _pos * ChunkWidth + new Vector3(ChunkWidth / 2f, ChunkWidth / 2f, 0f);
            chunkObj.transform.rotation = Quaternion.Euler(-90, 0, 0); // 旋转使其朝向 Z- 方向
            chunkObj.transform.localScale = new Vector3(ChunkWidth, ChunkWidth, ChunkWidth);

            // 添加 MeshFilter 组件（存放 Mesh 数据）
            MeshFilter meshFilter = chunkObj.AddComponent<MeshFilter>();
            meshFilter.mesh = GenMesh.CreateQuadMesh(); // 创建 1m*1m 的正方形网格

            // 添加 MeshRenderer 组件（用于渲染）
            MeshRenderer meshRenderer = chunkObj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Unlit/Texture")); // 使用 Unlit 纹理着色器

            // 生成纹理并赋值给材质
            //Texture2D texture = GenTexture.GetBlackWhiteTexture();
            //float[,] noiseMap = GenNoise.GenerateNoiseMap(VoxelWidth, VoxelWidth, gameManager.seed, gameManager.noiseScale, gameManager.octaves, gameManager.persistance, gameManager.lacunarity, new Vector2(_pos.x, _pos.y) * VoxelWidth + gameManager.offset, gameManager.normalizeMode);
            //float[,] noiseMap = GenNoise.GenerateVoronoiMap(new Vector2Int((int)_pos.x, (int)_pos.y), VoxelWidth, VoxelWidth, gameManager.VoronoiSeed, gameManager.VoronoiPointCount, gameManager.Voronoioffset);
            //Texture2D texture = GenTexture.GetPerlinNoiseTexture(noiseMap);
            int[] VoronoiInts = new int[1] { 2 };
            Texture2D texture = GenVoronoi.RenderVoronoiGraph(VoxelWidth, VoxelWidth, gameManager.VoronoiSeed, VoronoiInts);
            meshRenderer.material.mainTexture = texture;
             
            // 存入字典
            AllChunks[_pos] = chunkObj;
        }

        private static void DeleteChunk(Vector3 _pos)
        {
            if (!AllChunks.TryGetValue(_pos, out var _obj))
                return;

            AllChunks.Remove(_pos);
            gameManager.AddToDestroy(_obj);
        }

        
        #endregion

    }
}
