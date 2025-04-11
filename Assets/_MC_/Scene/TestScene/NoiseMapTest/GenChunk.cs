
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

                    // ���������û���ҵ� "Chunks"������ѡ�񴴽��������� NullReferenceException
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

                    // ���������û���ҵ� "MainCamera"������ѡ�񴴽�һ���µ� Camera
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

            // �����µ�����
            foreach (var _pos in _result)
                GenerateChunk(_pos);

            // ��¼��ɾ��������
            List<Vector3> _WaitToDel = new List<Vector3>();

            // ���� AllChunks
            foreach (var item in AllChunks) 
            {
                float _dis = (item.Key - MainCamera.transform.position).magnitude;

                if (_dis > RenderRange)
                    _WaitToDel.Add(item.Key);
            }

            // ͳһɾ�����飬��ֹ����ʱ�޸ļ��� 
            foreach (var _pos in _WaitToDel)
            {
                DeleteChunk(_pos);
            }
        }

        #endregion


        #region Camera

        //��ȡ������߽�
        private static void GetCameraBounds(out Vector2 Xrange, out Vector2 Yrange)
        {
            Camera cam = Camera.main;
            float size = cam.orthographicSize;  // ��ȡ�����ߴ�
            float aspect = cam.aspect;          // ��Ļ��߱�

            float width = size * 2 * aspect;    // �����ӿڿ��
            float height = size * 2;            // �����ӿڸ߶�

            Vector3 CameraPos = cam.transform.position;

            // X �᷶Χ���� -> �ң�
            Xrange = new Vector2(CameraPos.x - width / 2, CameraPos.x + width / 2);
            // Y �᷶Χ���� -> �ϣ�
            Yrange = new Vector2(CameraPos.y - height / 2, CameraPos.y + height / 2);

            //��������ȡ��
            Xrange = new Vector2(Mathf.Floor(Xrange.x), Mathf.Floor(Xrange.y));
            Yrange = new Vector2(Mathf.Floor(Yrange.x), Mathf.Floor(Yrange.y));
        }

        //��ȡ�����������Ұ�ڵ�δ��ǵ����еĵ�
        private static void GetNewChunkPos(out List<Vector3> _posList)
        {
            _posList = new List<Vector3>();

            //����Camera����
            Vector3 CameraPos = MainCamera.transform.position; CameraPos = new Vector3(Mathf.Floor(CameraPos.x), Mathf.Floor(CameraPos.y), 0);
            GetCameraBounds(out var Xrange, out var Yrange);

            //�������е�
            for (float _x = Xrange.x; _x <= Xrange.y; _x++)
            {
                for (float _y = Yrange.x; _y <= Yrange.y; _y++)
                {
                    Vector3 _thisPos = new Vector3(_x, _y, 0); 

                    //��ǰ����-����������Ѵ���
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

        private static float ChunkWidth = 1f;  //������
        private static int VoxelWidth = 16;    //���ؿ��

        private static void GenerateChunk(Vector3 _pos)
        {
            // ����һ���µ� GameObject
            GameObject chunkObj = new GameObject($"Chunk_{_pos}");
            chunkObj.transform.SetParent(Chunkparent.transform, false);
            chunkObj.transform.position = _pos * ChunkWidth + new Vector3(ChunkWidth / 2f, ChunkWidth / 2f, 0f);
            chunkObj.transform.rotation = Quaternion.Euler(-90, 0, 0); // ��תʹ�䳯�� Z- ����
            chunkObj.transform.localScale = new Vector3(ChunkWidth, ChunkWidth, ChunkWidth);

            // ��� MeshFilter �������� Mesh ���ݣ�
            MeshFilter meshFilter = chunkObj.AddComponent<MeshFilter>();
            meshFilter.mesh = GenMesh.CreateQuadMesh(); // ���� 1m*1m ������������

            // ��� MeshRenderer �����������Ⱦ��
            MeshRenderer meshRenderer = chunkObj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Unlit/Texture")); // ʹ�� Unlit ������ɫ��

            // ����������ֵ������
            //Texture2D texture = GenTexture.GetBlackWhiteTexture();
            //float[,] noiseMap = GenNoise.GenerateNoiseMap(VoxelWidth, VoxelWidth, gameManager.seed, gameManager.noiseScale, gameManager.octaves, gameManager.persistance, gameManager.lacunarity, new Vector2(_pos.x, _pos.y) * VoxelWidth + gameManager.offset, gameManager.normalizeMode);
            //float[,] noiseMap = GenNoise.GenerateVoronoiMap(new Vector2Int((int)_pos.x, (int)_pos.y), VoxelWidth, VoxelWidth, gameManager.VoronoiSeed, gameManager.VoronoiPointCount, gameManager.Voronoioffset);
            //Texture2D texture = GenTexture.GetPerlinNoiseTexture(noiseMap);
            int[] VoronoiInts = new int[1] { 2 };
            Texture2D texture = GenVoronoi.RenderVoronoiGraph(VoxelWidth, VoxelWidth, gameManager.VoronoiSeed, VoronoiInts);
            meshRenderer.material.mainTexture = texture;
             
            // �����ֵ�
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
