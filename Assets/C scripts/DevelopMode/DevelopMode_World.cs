using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class DevelopModeWorld : MonoBehaviour
{
    [Header("״̬")]
    public bool isLoading = false;

    [Header("Transforms")]
    public GameObject MainCamera;

    [Header("��Ⱦ����")]
    public int RenderWidth = 5; private int _RenderWidth = 5;
    [HideInInspector] public GameObject ChunkPATH;
    

    [Header("��������")]
    public Material material;
    public Material material_Water;
    public Material material_VoxelChunk;
    public BlockType[] blocktypes;


    [Header("Ⱥϵ�������ʺ�����(ֵԽ��ΧԽС)")]
    public float ����Ũ��OxygenDensity;
    public float ��ά�ܶ�Density3d;
    public float ����̶�Aridity;
    public float ����ʪ��MoistureLevel;
    public BiomeNoiseSystem[] biomenoisesystems;


    [Header("���ʷֲ������ϵͳ(n%)")]
    public TerrainLayerProbabilitySystem terrainLayerProbabilitySystem;
    public System.Random rand;



    [Header("�������Լ�")]
    public BiomeClarify[] biomeclarify; private BiomeClarify[] _biomeclarify;


    [Header("OnGui")]
    [HideInInspector] public Rect CreateWorldRect = new Rect();
    [HideInInspector] public Rect CreateWorldRect2 = new Rect();
    [HideInInspector] public Rect CreateWorldRect3 = new Rect();
    [HideInInspector] public Rect CreateWorldRect4 = new Rect();

    //ȫ��Chunkλ��
    public Dictionary<Vector3, DevelopModeChunk> AllBlockChunks = new Dictionary<Vector3, DevelopModeChunk>();
    public Dictionary<Vector3, DevelopModeVoxelChunk> AllVoxelChunks = new Dictionary<Vector3, DevelopModeVoxelChunk>();
    DevelopMode_NoiseDiagram NoiseDiagramTemp;

    //���ݽṹ
    Coroutine CreateCoroutine;
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();

    Coroutine Render_Coroutine;
    public ConcurrentQueue<DevelopModeChunk> WaitToRender = new ConcurrentQueue<DevelopModeChunk>();




    //Coroutine Mesh_Coroutine;
    //public ConcurrentQueue<DevelopModeChunk> WaitToCreateMesh = new ConcurrentQueue<DevelopModeChunk>();


    //----------------------------------------------------------------------------------------------------------



    private void Start()
    {
        //֡��
        Application.targetFrameRate = 90;

        //Self
        ChunkPATH = new GameObject();
        ChunkPATH.name = "ChunkPATH";
        ChunkPATH.transform.SetParent(GameObject.Find("Environment").transform);

        //��������
        terrainLayerProbabilitySystem.Seed = Random.Range(0, 100);
        rand = new System.Random(terrainLayerProbabilitySystem.Seed);

        //��ʼ��һ��NoiseDiagram
        NoiseDiagramTemp = new DevelopMode_NoiseDiagram(this);

        _RenderWidth = RenderWidth;

        // ��ʼ��ǰһ��״̬����
        _biomeclarify = new BiomeClarify[biomeclarify.Length];
        for (int i = 0; i < biomeclarify.Length; i++)
        {
            _biomeclarify[i] = new BiomeClarify
            {
                Domain = biomeclarify[i].Domain,
                color = biomeclarify[i].color
            };
        }

        ChooseToGenerate();

        //������ȾЭ��
        if (Render_Coroutine == null)
        {
            Render_Coroutine = StartCoroutine(RenderCoroutine());
        }


    }

    private void Update()
    {
        if (_RenderWidth != RenderWidth)
        {
            ChooseToGenerate();
            _RenderWidth = RenderWidth;
        }


        for (int i = 0; i < biomeclarify.Length; i++)
        {
            if (HasBiomeClarifyChanged(biomeclarify[i], _biomeclarify[i]))
            {
                // ִ��flash����
                ChooseToGenerate();

                // ���¾ɵ�״̬
                _biomeclarify[i].Domain = biomeclarify[i].Domain;
                _biomeclarify[i].color = biomeclarify[i].color;
            }
        }



    }

    private void OnGUI()
    {
        if (GUI.Button(CreateWorldRect, "NoiseDiagram"))
        {
            GenerateMode = 0;
            ChooseToGenerate();
        }

        if (GUI.Button(CreateWorldRect2, "VoxelChunk"))
        {
            GenerateMode = 1;
            ChooseToGenerate();
        }

        if (GUI.Button(CreateWorldRect3, "BlockChunk"))
        {

            if (!isLoading)
            {
                GenerateMode = 2;
                isLoading = true;
                ChooseToGenerate();
            }

        }

        if (isLoading)
        {
            GUI.Label(CreateWorldRect4, "���ɵ�����...");
        }



    }


    //----------------------------------------------------------------------------------------------------------

    #region ��������ģʽ


    //����ʲô�ı�־
    //0������ͼ
    //1����������
    //2����������
    int GenerateMode = 0;
    //int previous_GenerateMode = 0;

    //����ʲô���ж�
    void ChooseToGenerate()
    {
        MoveCamera();

        //Clear
        switch (GenerateMode)
        {
            case 0:
                ClearNoiseDiagram();
                ClearVoxelChunks();
                ClearBlockChunks();
                break;
            case 1:
                ClearNoiseDiagram();
                ClearVoxelChunks();
                ClearBlockChunks();
                break;
            case 2:
                ClearNoiseDiagram();
                //ClearVoxelChunks();
                ClearBlockChunks();
                break;
            default:
                print("previous_GenerateMode����");
                break;
        }

        //ָ��Create
        switch (GenerateMode)
        {
            case 0:
                CreateNoiseDiagram();
                //previous_GenerateMode = GenerateMode;
                break;
            case 1:
                CreateVoxelChunks();
                //previous_GenerateMode = GenerateMode;
                break;
            case 2:
                CreateBlockChunks();
                //previous_GenerateMode = GenerateMode;
                break;
            default:
                print("GenerateMode����");
                break;
        }


    }



    void CreateNoiseDiagram()
    {
        NoiseDiagramTemp = new DevelopMode_NoiseDiagram(this);
    }
    void ClearNoiseDiagram()
    {

        if (NoiseDiagramTemp.thisobject != null)
        {
            NoiseDiagramTemp.DestroySelf();
        }

    }


    //������������
    void CreateVoxelChunks()
    {

        for (float x = 0; x < RenderWidth; x++)
        {
            for (float z = 0; z < RenderWidth; z++)
            {
                DevelopModeVoxelChunk Chunktemp = new DevelopModeVoxelChunk(new Vector3(x, 0, z), this);
                AllVoxelChunks.Add(new Vector3(x, 0, z), Chunktemp);
            }
        }
    }

    void ClearVoxelChunks()
    {
        if (AllVoxelChunks.Count == 0)
        {
            return;
        }

        foreach (var chunktemp in AllVoxelChunks)
        {
            chunktemp.Value.Destroyself();
        }

        AllVoxelChunks.Clear();
    }


    //���ɷ�������
    private Coroutine CreateBlockChunkCoroutine;
    void CreateBlockChunks()
    {

        if (CreateBlockChunkCoroutine == null)
        {
            if (terrainLayerProbabilitySystem.isRandomSeed)
            {
                terrainLayerProbabilitySystem.Seed = Random.Range(0, 100);
                rand = new System.Random(terrainLayerProbabilitySystem.Seed);
            }

            CreateBlockChunkCoroutine = StartCoroutine(_CreateBlockChunks());
        }


    }

    IEnumerator _CreateBlockChunks()
    {
        for (float x = 0; x < RenderWidth; x++)
        {
            for (float z = 0; z < RenderWidth; z++)
            {
                DevelopModeChunk Chunktemp = new DevelopModeChunk(new Vector3(x, 0, z), this);
                AllBlockChunks.Add(new Vector3(x, 0, z), Chunktemp);
                yield return null;
            }
        }

        CreateBlockChunkCoroutine = null;

    }

    void ClearBlockChunks()
    {
        if (AllBlockChunks.Count == 0)
        {
            return;
        }

        foreach (var chunktemp in AllBlockChunks)
        {
            chunktemp.Value.Destroyself();
        }

        AllBlockChunks.Clear();
    }



    //--------------------------------------------

    #endregion

    //----------------------------------------------------------------------------------------------------------

    #region �������Ե�

    //������
    public float GetSimpleNoise(int _x, int _z, Vector3 _myposition)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x) * 0.01f, (_z + _myposition.z) * 0.01f));
        return smoothNoise;
    }

    //��ƫ�����������������ȷֲ���
    //_offset����ΪVector3(111f,222f)
    //_Scale��������:0.01Ϊ��������,0.1Ϊˮ��ɳ��ֲ�
    public float GetSimpleNoiseWithOffset(int _x, int _z, Vector3 _myposition, Vector2 _Offset, float _Scale)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x + _Offset.x) * _Scale, (_z + _myposition.z + _Offset.y) * _Scale));
        return smoothNoise;
    }

    //�������Ⱥϵ����
    public byte GetBiomeType(int _x, int _z, Vector3 _myposition)
    {

        float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
        float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
        float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
        float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

        ////ɳĮ
        //if (_C >= ����̶�Aridity)
        //{

        //    return 2;

        //}

        //else
        //{

        //    //��ԭ
        //    if (_B >= ��ά�ܶ�Density3d)
        //    {
        //        return 1;
        //    }

        //    //��ԭ
        //    else if (_A >= ����Ũ��OxygenDensity)
        //    {

        //        if (_D >= ����ʪ��MoistureLevel)
        //        {
        //            return 3;
        //        }
        //        else
        //        {
        //            return 0;
        //        }



        //    }
        //    else
        //    {
        //        //��������
        //        return 4;
        //    }

        //}

        //��ԭ
        if (_B >= ��ά�ܶ�Density3d)
        {
            return 1;
        }

        else
        {

            //ɳĮ
            if (_C >= ����̶�Aridity)
            {

                return 2;

            }

            //��ԭ
            else if (_A >= ����Ũ��OxygenDensity)
            {

                if (_D >= ����ʪ��MoistureLevel)
                {
                    return 3;
                }
                else
                {
                    return 0;
                }



            }
            else
            {
                //��������
                return 4;
            }

        }


    }

    //����
    public float GetTotalNoiseHigh(int _x, int _z, Vector3 _myposition)
    {

        //(ƽԭ-ɽ��)��������
        float biome_moutainAndPlane = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((float)_x * 1e-05f + _myposition.x * 1e-05f, (float)_z * 1e-05f + _myposition.z * 1e-05f));


        //С��ƽԭ����
        //��ɽ������
        float soilmax = Mathf.Lerp(50, 64, biome_moutainAndPlane);
        float smooth = Mathf.Lerp(0.002f, 0.04f, biome_moutainAndPlane);
        float steep = Mathf.Lerp(0.004f, 0.05f, biome_moutainAndPlane);


        //��������
        float noise2d_1 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * smooth + _myposition.x * smooth, (float)_z * smooth + _myposition.z * smooth));
        float noise2d_2 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * steep + _myposition.x * steep, (float)_z * steep + _myposition.z * steep));
        float noiseHigh = noise2d_1 * 0.7f + noise2d_2 * 0.3f;
        return noiseHigh;
    }


    //���ݸ���������Ⱥϵ����
    //��ɸ�����Ⱥϵ����
    public float GetTotalNoiseHigh_Biome(int _x, int _z, Vector3 _myposition)
    {
        //Noise
        float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.x);
        float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.y);
        float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.z);
        float noise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[0].Noise_Rank_123.x + noise_2 * biomenoisesystems[0].Noise_Rank_123.y + noise_3 * biomenoisesystems[0].Noise_Rank_123.z);
        float noise_High = Mathf.Lerp(biomenoisesystems[0].HighDomain.x, biomenoisesystems[0].HighDomain.y, noise);

        //���ݶ���
        int BiomeType = -1;
        float BiomeIntensity = 0f;
        float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
        float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
        float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
        float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

        //��õ�ǰȺϵ
        //���Ⱥϵ���ǿ��
        //��ԭ
        if (_B >= ��ά�ܶ�Density3d)
        {
            BiomeType = VoxelData.Biome_Plateau;
            BiomeIntensity = Mathf.InverseLerp(��ά�ܶ�Density3d, 1f, _B);
        }
        else
        {
            
            if (_C >= ����̶�Aridity)
            {
                BiomeType = VoxelData.Biome_Dessert;
                BiomeIntensity = Mathf.InverseLerp(����̶�Aridity, 1f, _C);
            }
            //��ԭ
            else if (_A >= ����Ũ��OxygenDensity)
            {
                if (_D >= ����ʪ��MoistureLevel)
                {
                    BiomeType = VoxelData.Biome_Marsh;
                    BiomeIntensity = Mathf.InverseLerp(����ʪ��MoistureLevel, 1f, _D);
                }
                else
                {
                    BiomeType = VoxelData.Biome_Plain;
                    BiomeIntensity = Mathf.InverseLerp(����Ũ��OxygenDensity, 1f, _A);
                }
            }
            else
            {
                BiomeType = VoxelData.Biome_Plain;
                BiomeIntensity = Mathf.InverseLerp(����Ũ��OxygenDensity, 1f, _A);
            }

        }

        //BiomeType = 1;

        //���Ⱥϵ
        float Mixnoise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.x);
        float Mixnoise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.y);
        float Mixnoise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.z);
        float Mixnoise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[BiomeType].Noise_Rank_123.x + noise_2 * biomenoisesystems[BiomeType].Noise_Rank_123.y + noise_3 * biomenoisesystems[BiomeType].Noise_Rank_123.z);
        float Mixnoise_High = Mathf.Lerp(biomenoisesystems[BiomeType].HighDomain.x, biomenoisesystems[BiomeType].HighDomain.y, Mixnoise);

        float �������� = Mathf.Lerp(noise_High, Mixnoise_High, BiomeIntensity);

        return ��������;
        //return noise_High + �������� * ���������Ŵ���; 
    
    }


    

    //ϸ������
    public static float WorleyNoise(float _x, float _y) 
    {
        float x = _x * 0.05f;
        float y = _y * 0.05f;

        float distance = float.MaxValue;
        for (int Y = -1; Y <= 1; Y++)
        {
            for (int X = -1; X <= 1; X++)
            {
                Vector2 cellPoint = hash(new Vector2((int)x + X, (int)y + Y));
                distance = Mathf.Min(distance, Vector2.Distance(cellPoint, new Vector2(x, y)));
            }
        }

        return 1 - distance;
    }


    //ϸ��������������
    static Vector2 hash(Vector2 p)
    {
        float random = Mathf.Sin(666 + p.x * 5678 + p.y * 1234) * 4321;
        return new Vector2(p.x + Mathf.Sin(random) / 2 + 0.5f, p.y + Mathf.Cos(random) / 2 + 0.5f);
    }




    #endregion

    //----------------------------------------------------------------------------------------------------------

    #region ������

    //�Ƚ������Ƿ�һ��
    bool HasBiomeClarifyChanged(BiomeClarify current, BiomeClarify previous)
    {
        return current.Domain != previous.Domain ||
               current.color != previous.color;
    }

    //��ȾЭ��
    IEnumerator RenderCoroutine()
    {
        while (true)
        {

            if (WaitToRender.TryDequeue(out DevelopModeChunk chunktemp))
            {

                if (chunktemp.isReadyToRender)
                {
                    chunktemp.CreateMesh();
                }
                
            }

            yield return null;
        }
        
    }


    //�ƶ������λ��
    void MoveCamera()
    {
        MainCamera.transform.position = new Vector3((RenderWidth * 16f) * 0.3f, ((RenderWidth - 2f) * 14f) + 69f, RenderWidth * 8f) ;
    }


    //Vector3 --> ����������
    public Vector3 GetChunkLocation(Vector3 vec)
    {

        return new Vector3((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);

    }


    public Vector3 GetRealChunkLocation(Vector3 vec)
    {

        return new Vector3(16f * ((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth), 0, 16f * ((vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth));

    }



    //Vector3 --> ��������������
    public Vector3 GetRelalocation(Vector3 vec)
    {

        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }


    #endregion

    //----------------------------------------------------------------------------------------------------------

}

//���Լ�
[System.Serializable]
public class BiomeClarify
{
    public string name;
    public float Domain;
    public Color color;
}





