//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using NewGame;
//using System.Threading;
//using Unity.Collections;
//using Unity.Jobs;


///// <summary>
///// JobSystem
///// </summary>
////public class AAA : MonoBehaviour
////{
////    public VoxelStruct[,,] voxelMap = new VoxelStruct[TerrainData.ChunkWidth, TerrainData.ChunkHeight, TerrainData.ChunkWidth];

////    // Mesh
////    int vertexIndex = 0;
////    List<Vector3> vertices = new List<Vector3>();
////    List<int> triangles = new List<int>();
////    List<int> triangles_Water = new List<int>();
////    List<Vector2> uvs = new List<Vector2>();

////    private void Start()
////    {
////        CreateDataJob createDataJob = new CreateDataJob
////        {
////            voxelMap = new NativeArray<VoxelStruct>(TerrainData.ChunkWidth * TerrainData.ChunkHeight * TerrainData.ChunkWidth, Allocator.TempJob)
////        };

////        JobHandle createDataHandle = createDataJob.Schedule();
////        createDataHandle.Complete();

////        // 填充 voxelMap
////        for (int y = 0; y < TerrainData.ChunkHeight; y++)
////        {
////            for (int x = 0; x < TerrainData.ChunkWidth; x++)
////            {
////                for (int z = 0; z < TerrainData.ChunkWidth; z++)
////                {
////                    voxelMap[x, y, z] = createDataJob.voxelMap[x + y * TerrainData.ChunkWidth + z * TerrainData.ChunkWidth * TerrainData.ChunkHeight];
////                }
////            }
////        }

////        // 生成网格
////        GenerateMeshJob generateMeshJob = new GenerateMeshJob
////        {
////            voxelMap = voxelMap
////        };

////        JobHandle generateMeshHandle = generateMeshJob.Schedule();
////        generateMeshHandle.Complete();

////        // 交给 world 渲染
////        world.RenderQueue.Enqueue(this);
////    }

////    struct CreateDataJob : IJob
////    {
////        public NativeArray<VoxelStruct> voxelMap;

////        public void Execute()
////        {
////            for (int y = 0; y < TerrainData.ChunkHeight; y++)
////            {
////                for (int x = 0; x < TerrainData.ChunkWidth; x++)
////                {
////                    for (int z = 0; z < TerrainData.ChunkWidth; z++)
////                    {
////                        VoxelStruct voxel = new VoxelStruct();
////                        if (y == 0)
////                        {
////                            voxel.voxelType = VoxelData.BedRock;
////                        }
////                        else if (y == 1 || y == 2)
////                        {
////                            voxel.voxelType = VoxelData.Soil;
////                        }
////                        else if (y == 3)
////                        {
////                            voxel.voxelType = VoxelData.Grass;
////                        }
////                        else
////                        {
////                            voxel.voxelType = VoxelData.Air;
////                        }

////                        voxelMap[x + y * TerrainData.ChunkWidth + z * TerrainData.ChunkWidth * TerrainData.ChunkHeight] = voxel;
////                    }
////                }
////            }
////        }
////    }

////    struct GenerateMeshJob : IJob
////    {
////        [ReadOnly]
////        public VoxelStruct[,,] voxelMap;

////        public void Execute()
////        {
////            // Mesh 生成逻辑
////            // 根据 voxelMap 生成网格数据
////            // ...
////        }
////    }
////}



//namespace NewGame
//{
//    public class NewChunk
//    {

//        #region 状态


//        #endregion



//        #region 周期函数

//        NewWorld world;
//        Vector3 myPosition;

//        public NewChunk(NewWorld _world, Vector3 _vec)
//        {

//            //data
//            world = _world;
//            myPosition = _vec;

//            //object
//            GameObject newChunktemp = new GameObject($"{_vec}");
//            newChunktemp.transform.SetParent(world.ChunksParent.transform);


//            //

//        }

//        #endregion


//        #region 创建数据

//        //创建数据
//        void CreateData()
//        {

//        }

//        #endregion


//        #region 生成Mesh与创建mesh


//        //生成网格
//        void GenerateMeshGrid()
//        {
//            //this

//            //添加
//            world.RenderQueue.Enqueue(this);
//        }


//        /// <summary>
//        /// 渲染网格
//        /// </summary>
//        public void CreateMesh()
//        {

//        }

//        #endregion



//    }

//}


