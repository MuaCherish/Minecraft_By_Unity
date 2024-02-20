using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{

    //组件
    //public ChunkCoord coord;
    GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    //Mesh的绘制
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //Block的种类数组
    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    //World脚本
    World world;
    int treecount;

    //噪声
    private float noise2d_scale_smooth;
    private float noise2d_scale_steep;
    private float noise3d_scale;

    //全部chunks
    private int x;
    private int y;
    private int z;

    


    //---------------------------------- 周期函数 ---------------------------------------

    //Start()
    public Chunk(Vector3 thisPosition, World _world)
    {
        //从world获取参数
        world = _world;
        UnityEngine.Random.InitState(world.Seed);
        noise2d_scale_smooth = world.noise2d_scale_smooth;
        noise2d_scale_steep = world.noise2d_scale_steep;
        noise3d_scale = world.noise3d_scale;
        treecount = world.TreeCount;

        //坐标
        x = 0;
        y = 0;
        z = 0;

        //ChunkObject
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + ", " + thisPosition.z;

        //先创建数据
        CreateData();

        //开始遍历，生成数据
        DataToChunk();

    }


    //-----------------------------------------------------------------------------------







    //---------------------------------- Data部分 ---------------------------------------

    //方块类型的初始化
    void CreateData()
    {



        //对一个chunk进行遍历
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {


                    /*
					 0：基岩
					 1：石头
					 2：草地
					 3：泥土
					 4：空气
                     5：沙子
                     6：木头
                     7：树叶
                     8：水
                     9：煤炭
					*/


                    int randomInt = UnityEngine.Random.Range(0, 2);

                    //判断基岩
                    if (y == 0)
                    {
                        voxelMap[x, y, z] = VoxelData.BedRock;
                    }
                    else if (y > 0 && y < 3 && randomInt == 1)
                    {
                        voxelMap[x, y, z] = VoxelData.BedRock;
                    }
                    else
                    {



                        //三个2d噪声
                        float noise2d_1 = Mathf.Lerp((float)world.soil_min, (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + chunkObject.transform.position.x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + chunkObject.transform.position.z * noise2d_scale_smooth));
                        float noise2d_2 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_steep + chunkObject.transform.position.x * noise2d_scale_steep, (float)z * noise2d_scale_steep + chunkObject.transform.position.z * noise2d_scale_steep));
                        float noise2d_3 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * 0.1f + chunkObject.transform.position.x * 0.1f, (float)z * 0.15f + chunkObject.transform.position.z * 0.15f));

                        //噪声叠加
                        float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f + noise2d_3 * 0.05f;


                        //判断泥土
                        if (y > noiseHigh)
                        {

                            //if (y - 1 < noiseHigh)
                            //{
                            //    if (UnityEngine.Random.Range(0, 100) > 90)
                            //    {
                            //        voxelMap[x, y, z] = 6;
                            //    }
                            //    else
                            //    {
                            //        voxelMap[x, y, z] = VoxelData.Air;
                            //    }
                            //}
                            //else
                            //{
                            //    voxelMap[x, y, z] = 4;
                            //}


                            //如果y小于海平面则为水，否则为空气
                            if (y - 1 < world.sea_level)
                            {
                                voxelMap[x, y, z] = VoxelData.Water;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Air;
                            }



                        }
                        else if ((y + 1) > noiseHigh)
                        {
                            if (y > world.sea_level)
                            {
                                voxelMap[x, y, z] = VoxelData.Grass;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Sand;
                            }

                        }
                        else if (y > noiseHigh - 7)
                        {
                            voxelMap[x, y, z] = VoxelData.Soil;
                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && randomInt == 1)
                        {
                            voxelMap[x, y, z] = VoxelData.Soil;
                        }
                        else
                        {



                            //判断空气
                            float noise3d = Perlin3D((float)x * noise3d_scale + chunkObject.transform.position.x * noise3d_scale, (float)y * noise3d_scale + y * noise3d_scale, (float)z * noise3d_scale + chunkObject.transform.position.z * noise3d_scale); // 将100改为0.1
                            int random_Coal = UnityEngine.Random.Range(0, 100);

                            if (noise3d < 0.4f)
                            {
                                voxelMap[x, y, z] = VoxelData.Air;
                            }
                            else
                            {
                                if (random_Coal < 2)
                                {
                                    voxelMap[x, y, z] = VoxelData.Coal;
                                }
                                else
                                {
                                    voxelMap[x, y, z] = VoxelData.Stone;
                                }

                            }



                        }



                    }

                }
            }
        }


        //补充树
        CreateTree();

        //voxelMap[0, 0, 0] = 6;
        //isVoxelMapPopulated = true;
    }

    //tree
    void CreateTree()
    {
        //[确定XZ]xoz上随便选择5个点
        while (treecount-- != 0)
        {
            int random_x = UnityEngine.Random.Range(2, VoxelData.ChunkWidth - 2);
            int random_z = UnityEngine.Random.Range(2, VoxelData.ChunkWidth - 2);
            int random_y = VoxelData.ChunkHeight;
            bool needTree = true;
            int random_Tree_High = UnityEngine.Random.Range(world.TreeHigh_min, world.TreeHigh_max + 1);

            //[确定Y]向下遍历直到地表上一层
            while (random_y-- != 0)
            {
                //如果是沙子或者树叶或者水面，不生成树
                if (voxelMap[random_x, random_y - 1, random_z] == VoxelData.Sand || voxelMap[random_x, random_y - 1, random_z] == VoxelData.Leaves || voxelMap[random_x, random_y - 1, random_z] == VoxelData.Water)
                {
                    needTree = false;
                    break;
                }


                //如果树顶超过最大高度，不生成
                //else if (random_y + random_Tree_High >= VoxelData.ChunkHeight)
                //{
                //    needTree = false;
                //    break;
                //}

                //如果碰到固体则生成
                else if (voxelMap[random_x, random_y - 1, random_z] != VoxelData.Air)
                {
                    needTree = true;
                    break;
                }
            }


            //定好树桩后，向上延伸5~7层树干
            if (needTree)
            {

                for (int i = 0; i <= random_Tree_High; i++)
                {
                    voxelMap[random_x, random_y + i, random_z] = VoxelData.Wood;
                }

                //生成树叶
                CreateLeaves(random_x, random_y + random_Tree_High, random_z);

            }


            //Debug.Log($"{random_x}, {random_y}, {random_z}");
        }
    }

    //leaves
    void CreateLeaves(int _x, int _y, int _z)
    {
        int randomInt = UnityEngine.Random.Range(0, 2);


        //第一层
        if (randomInt == 0)
        {
            SetLeaves(_x, _y + 1, _z);
        }
        else if (randomInt == 1)
        {
            SetLeaves(_x, _y + 1, _z + 1);
            SetLeaves(_x - 1, _y + 1, _z);
            SetLeaves(_x, _y + 1, _z);
            SetLeaves(_x + 1, _y + 1, _z);
            SetLeaves(_x, _y + 1, _z - 1);
        }

        //第二层
        SetLeaves(_x - 1, _y, _z);
        SetLeaves(_x + 1, _y, _z);
        SetLeaves(_x, _y, _z - 1);
        SetLeaves(_x, _y, _z + 1);

        //第三层
        SetLeaves(_x - 1, _y - 1, _z + 2);
        SetLeaves(_x, _y - 1, _z + 2);
        SetLeaves(_x + 1, _y - 1, _z + 2);

        SetLeaves(_x - 2, _y - 1, _z + 1);
        SetLeaves(_x - 1, _y - 1, _z + 1);
        SetLeaves(_x, _y - 1, _z + 1);
        SetLeaves(_x + 1, _y - 1, _z + 1);
        SetLeaves(_x + 2, _y - 1, _z + 1);

        SetLeaves(_x - 2, _y - 1, _z);
        SetLeaves(_x - 1, _y - 1, _z);
        SetLeaves(_x + 1, _y - 1, _z);
        SetLeaves(_x + 2, _y - 1, _z);

        SetLeaves(_x - 2, _y - 1, _z - 1);
        SetLeaves(_x - 1, _y - 1, _z - 1);
        SetLeaves(_x, _y - 1, _z - 1);
        SetLeaves(_x + 1, _y - 1, _z - 1);
        SetLeaves(_x + 2, _y - 1, _z - 1);

        SetLeaves(_x - 1, _y - 1, _z - 2);
        SetLeaves(_x, _y - 1, _z - 2);
        SetLeaves(_x + 1, _y - 1, _z - 2);

        //第四层
        SetLeaves(_x - 1, _y - 2, _z + 2);
        SetLeaves(_x, _y - 2, _z + 2);
        SetLeaves(_x + 1, _y - 2, _z + 2);

        SetLeaves(_x - 2, _y - 2, _z + 1);
        SetLeaves(_x - 1, _y - 2, _z + 1);
        SetLeaves(_x, _y - 2, _z + 1);
        SetLeaves(_x + 1, _y - 2, _z + 1);
        SetLeaves(_x + 2, _y - 2, _z + 1);

        SetLeaves(_x - 2, _y - 2, _z);
        SetLeaves(_x - 1, _y - 2, _z);
        SetLeaves(_x + 1, _y - 2, _z);
        SetLeaves(_x + 2, _y - 2, _z);

        SetLeaves(_x - 2, _y - 2, _z - 1);
        SetLeaves(_x - 1, _y - 2, _z - 1);
        SetLeaves(_x, _y - 2, _z - 1);
        SetLeaves(_x + 1, _y - 2, _z - 1);
        SetLeaves(_x + 2, _y - 2, _z - 1);

        SetLeaves(_x - 1, _y - 2, _z - 2);
        SetLeaves(_x, _y - 2, _z - 2);
        SetLeaves(_x + 1, _y - 2, _z - 2);

    }

    //leaves设定值，防止碰到树木
    void SetLeaves(int x, int y, int z)
    {
        //如果是固体，就不用生成树叶了
        if (voxelMap[x, y, z] != VoxelData.Air)
        {
            return;
        }
        else
        {
            voxelMap[x, y, z] = VoxelData.Leaves;
        }
    }


    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh部分 ----------------------------------------

    //开始遍历
    public void DataToChunk()
    {

        ClearMeshData();

        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //如果是小花，调用另一个updatemesh算法
                    //if(voxelMap[x, y - 1, z] == 9)
                    //{
                    //    updateMeshFlower(new Vector3(x, y, z));
                    //    continue;
                    //}


                    //空气就直接跳过，除非是水面上的空气
                    if (world.blocktypes[voxelMap[x, y, z]].isSolid || voxelMap[x, y, z] == VoxelData.Water || voxelMap[x, y - 1, z] == VoxelData.Water)
                        UpdateMeshData(new Vector3(x, y, z));
                    


                }
            }
        }

        //最够一次性构建所有面
        CreateMesh();

    }

    //清除网格
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    //编辑方块
    public void EditData(Vector3 pos, byte targetBlocktype)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        voxelMap[x, y, z] = targetBlocktype;

        DataToChunk();
    }

    //面生成的判断
    //是方块吗----Y不绘制----N绘制
    //靠近边界----也返回N
    bool CheckVoxel(Vector3 pos, int _p)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //print($"{x},{y},{z}");


        //如果目标出界
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
        {

            //if (ThisChunkLocation == new Vector3(100f,0f,99f))
            //{
            //	print("");
            //}




            //自己是不是空气
            if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air || voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water)
            {
                return true;
            }
            else
            {
                return false;
            }



            //目标处是否存在chunk
            //if (Copy_All_Chunks.TryGetValue(VoxelData.faceChecks[_p] + ThisChunkLocation, out Chunk cc))
            //{

            //    if (hasExec)
            //    {
            //        //print($"当前Chunk：{ThisChunkLocation}，目标Chunk：{ThisChunkLocation + VoxelData.faceChecks[_p]}");
            //        hasExec = false;
            //    }

            //    //判断一下方向
            //    //Z方向
            //    if (VoxelData.faceChecks[_p] == new Vector3(0, 0, -1))
            //    {
            //        //自己是空气，目标是Block，则返回false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[x, y, VoxelData.ChunkWidth - 1] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }
            //    else if (VoxelData.faceChecks[_p] == new Vector3(0, 0, 1))
            //    {
            //        //自己是空气，目标是Block，则返回false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[x, y, 0] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }
            //    //x方向
            //    else if (VoxelData.faceChecks[_p] == new Vector3(-1, 0, 0))
            //    {
            //        //自己是空气，目标是Block，则返回false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[VoxelData.ChunkWidth - 1, y, z] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }
            //    else if (VoxelData.faceChecks[_p] == new Vector3(1, 0, 0))
            //    {
            //        //自己是空气，目标是Block，则返回false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[0, y, z] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }


            //    //自己是空气，目标是Block，则返回false
            //    //if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[x & 16, y & 16, z & 16] != 4)
            //    //{
            //    //	return false;
            //    //}
            //    //else
            //    //{
            //    //	return true;
            //    //}

            //}
            //else
            //{
            //    //自己是不是空气
            //    if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}




        }
        else
        {

            //判断自己和目标是不是都是空气 || 自己和目标是不是都是水
            //不生成面的情况
            if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Air) || ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Water))
            {
                return true;
            }
            else // 生成面的情况
            {
                //(如果自己是水，目标是空气) || (自己是空气，目标是水)
                if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Air))
                {
                    return false;
                }else if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Water))
                {
                    return false;
                }
            }


            //如果自己是水
            //if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water)
            //{
            //    //上方时
            //    if (_p == 2 || voxelMap[x, y, z] == 4)
            //    {
            //        return false;
            //    }
            //    else // 前后左右一律不生成
            //    {
            //        return true;
            //    }
            //}



        }


        return world.blocktypes[voxelMap[x, y, z]].isSolid;

    }

    //遍历中：：顺带判断面的生成方向
    //创建mesh里的参数
    void UpdateMeshData(Vector3 pos)
    {

        //判断六个面
        for (int p = 0; p < 6; p++)
        {

            //if (x == 10 && y == 37 && z == 14)
            //{
            //    print("");
            //}

            //voxelMap[pos + VoxelData.faceChecks[p]]
            //Debug.Log(pos);

            if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
            {

                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                //uvs.Add (VoxelData.voxelUvs [0]);
                //uvs.Add (VoxelData.voxelUvs [1]);
                //uvs.Add (VoxelData.voxelUvs [2]);
                //uvs.Add (VoxelData.voxelUvs [3]);
                //AddTexture(1);

                //根据p生成对应的面，对应的UV
                AddTexture(world.blocktypes[blockID].GetTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;

            }
        }

    }

    //最后生成网格体
    //mesh实体化
    void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

    }

    //生成面——UV的处理
    void AddTexture(int textureID)
    {

        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));


    }

    //生成小花小草
    void updateMeshFlower()
    {
        
    }



    //------------------------------------------------------------------------------------






    //---------------------------------- 辅助部分 ----------------------------------------

    //销毁自己
    public void DestroyChunk()
    {
        Destroy(chunkObject);
    }

    //隐藏自己
    public void HideChunk()
    {
        chunkObject.SetActive(false);
    }

    //显示自己
    public void ShowChunk()
    {
        chunkObject.SetActive(true);
    }

    //3d噪声
    public static float Perlin3D(float x, float y, float z)
    {

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6f;

    }


    //----------------------------------------------------------------------------------

}
