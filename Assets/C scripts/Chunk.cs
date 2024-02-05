using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Chunk : MonoBehaviour {

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

    //噪声
    private float noise2d_scale_smooth;
    private float noise2d_scale_steep;
    private float noise3d_scale;

    //全部chunks
    private Dictionary<Vector3, Chunk> Copy_All_Chunks;
    private Vector3 ThisChunkLocation;
    private bool hasExec = true;
    //private Vector3 localVec;
    private int x;
    private int y;
    private int z;

    //初始化
    public Chunk(Vector3 thisPosition, World _world)
    {
        world = _world;

        Copy_All_Chunks = world.Allchunks;

        noise2d_scale_smooth = world.noise2d_scale_smooth;
        noise2d_scale_steep = world.noise2d_scale_steep;
        noise3d_scale = world.noise3d_scale;

        x = 0;
        y = 0;
        z = 0;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.transform);

        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + ", " + thisPosition.z;

        //noise2d_plain = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise(chunkObject.transform.position.x * 0.12f,chunkObject.transform.position.z * 0.12f));
        //print($"当前坐标：{chunkObject.name}，查询的allchunk：{Copy_All_Chunks.Count}");
        ThisChunkLocation = thisPosition;

        //先创建数据
        PopulateVoxelMap();

        //开始遍历，生成数据
        UpdateChunk();

     
    }

    //Block_Type序列化
    void PopulateVoxelMap() {



        //对一个chunk进行遍历
        for (int y = 0; y < VoxelData.ChunkHeight; y++) {
            for (int x = 0; x < VoxelData.ChunkWidth; x++) {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) {


                    /*
					 0：基岩
					 1：石头
					 2：草地
					 3：泥土
					 4：空气
					*/


                    int randomInt = UnityEngine.Random.Range(0, 2);

                    //判断基岩
                    if (y == 0)
                    {
                        voxelMap[x, y, z] = 0;
                    } else if (y > 0 && y < 3 && randomInt == 1)
                    {
                        voxelMap[x, y, z] = 0;
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
                            voxelMap[x, y, z] = 4;
                        } else if ((y + 1) > noiseHigh)
                        {
                            if (y > world.sea_level)
                            {
                                voxelMap[x, y, z] = 2;
                            }
                            else
                            {
                                voxelMap[x, y, z] = 5;
                            }

                        } else if (y > noiseHigh - 7)
                        {
                            voxelMap[x, y, z] = 3;
                        } else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && randomInt == 1)
                        {
                            voxelMap[x, y, z] = 3;
                        }
                        else
                        {



                            //判断空气
                            float noise3d = Noise.Perlin3D((float)x * noise3d_scale + chunkObject.transform.position.x * noise3d_scale, (float)y * noise3d_scale + y * noise3d_scale, (float)z * noise3d_scale + chunkObject.transform.position.z * noise3d_scale); // 将100改为0.1

                            if (noise3d < 0.4f)
                            {
                                voxelMap[x, y, z] = 4;
                            }
                            else
                            {
                                voxelMap[x, y, z] = 1;
                            }



                        }



                    }

                }
            }
        }

    }

    //开始遍历
    public void UpdateChunk() {

        ClearMeshData();

        for (y = 0; y < VoxelData.ChunkHeight; y++) {
            for (x = 0; x < VoxelData.ChunkWidth; x++) {
                for (z = 0; z < VoxelData.ChunkWidth; z++) {

                    if (world.blocktypes[voxelMap[x, y, z]].isSolid)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }


                }
            }
        }

        //最够一次性构建所有面
        CreateMesh();

    }

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

        UpdateChunk();
    }

	//面生成的判断
    //是方块吗----Y不绘制----N绘制
    //靠近边界----也返回N
    bool CheckVoxel (Vector3 pos,int _p) {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		//print($"{x},{y},{z}");


		//如果目标出界
		if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
		{

            //if (ThisChunkLocation == new Vector3(100f,0f,99f))
            //{
            //	print("");
            //}


            //自己是不是空气
            if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4)
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




        }//另判断自己和目标是不是空气
        else if((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && voxelMap[x,y,z] == 4)
		{
            return true;
        }


        return world.blocktypes[voxelMap [x, y, z]].isSolid;

    }

    //遍历中：：顺带判断面的生成方向
    void UpdateMeshData (Vector3 pos) {

		//判断六个面
		for (int p = 0; p < 6; p++) {

            //if (pos == new Vector3(16,32,16))
            //{
            //    print("");
            //}


            if (!CheckVoxel(pos + VoxelData.faceChecks[p],p)) {

                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 0]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 1]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 2]]);
				vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 3]]);

				//uvs.Add (VoxelData.voxelUvs [0]);
				//uvs.Add (VoxelData.voxelUvs [1]);
				//uvs.Add (VoxelData.voxelUvs [2]);
				//uvs.Add (VoxelData.voxelUvs [3]);
				//AddTexture(1);

				//根据p生成对应的面，对应的UV
                AddTexture(world.blocktypes[blockID].GetTextureID(p));

                triangles.Add (vertexIndex);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 3);
				vertexIndex += 4;

			}
		}

	}



	//最后生成网格体
	void CreateMesh () {

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.uv = uvs.ToArray ();
        mesh.Optimize();
        mesh.RecalculateNormals ();
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

	//销毁自己
    public void DestroyChunk()
    {
		Destroy(chunkObject);
    }

}
