using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Chunk{

	//组件
	public ChunkCoord coord;
	GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

	//Mesh的绘制
	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
	List<Vector2> uvs = new List<Vector2> ();

	//Block的种类数组
    byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

	//World脚本
	World world;

	//噪声
    private float noise2d_scale_smooth;
    private float noise2d_scale_steep;
    private float noise3d_scale;

    public Chunk(ChunkCoord _coord ,World _world,float noise2d_smooth,float noise2d_steep,float noise3d)
	{
		world = _world;
		coord = _coord;


        noise2d_scale_smooth = noise2d_smooth;
		noise2d_scale_steep = noise2d_steep;
		noise3d_scale = noise3d;


		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent <MeshRenderer>();
		meshRenderer.sharedMaterial = world.material;
		chunkObject.transform.SetParent(world.transform);

        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = coord.x + ", " + coord.z;

        //先创建数据
        PopulateVoxelMap(coord);

        //开始遍历，生成数据
        CreateMeshData();

        //最够一次性构建所有面
        CreateMesh();
    }





    //Start
 //   private void Start()
 //   {
        
    


 //       //    for (int y = 0; y < 10; y++)
 //       //    {
 //       //        for (int z = 0; z < 10; z++)
 //       //        {
 //       //            for (int x = 0; x < 10; x++)
 //       //{
 //       //	float noise = Noise.Perlin3D((float)x * 0.1f, (float)y * 0.1f, (float)z * 0.1f); // 将100改为0.1

 //       //	if (noise < 0.4)
 //       //	{
 //       //		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
 //       //		cube.transform.position = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);// 乘以一个适当的放大倍数
 //       //	}


 //       //}

 //       //        }
 //       //    }





 //       ////获取World脚本
 //       //world = GameObject.Find("World").GetComponent<World>();

	//	//先创建数据
	//	PopulateVoxelMap();

	//	//开始遍历，生成数据
	//	CreateMeshData();

	//	//最够一次性构建所有面
	//	CreateMesh();

	//}

    //Block_Type序列化
    void PopulateVoxelMap (ChunkCoord crood) {



        for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {




                    //int randomInt2 = Random.Range(0, 4);

                    /*
					 ChunkHeight-8 ~ ChunkHeight：泥土
					 else：石头
					 0 ~ 2：基岩层
					 */

                    /*
					 0：基岩
					 1：石头
					 2：草地
					 3：泥土
					 4：空气
					*/

                    ////最顶层
                    //               if (y == VoxelData.ChunkHeight - 1)
                    //{
                    //	voxelMap[x, y, z] = 2;
                    //}//基岩层
                    //else if (y == 0)
                    //{
                    //	voxelMap[x, y, z] = 0;
                    //}//基岩缓冲层
                    //else if (y > 0 && y < 3 && randomInt == 1)
                    //{
                    //	voxelMap[x, y, z] = 0;
                    //}//泥土层
                    //else if (y > VoxelData.ChunkHeight - 6 && y < VoxelData.ChunkHeight)
                    //{
                    //	voxelMap[x, y, z] = 3;
                    //}//泥土缓冲层
                    //else if (y >= VoxelData.ChunkHeight - 8 && y <= VoxelData.ChunkHeight - 6 && randomInt == 1)
                    //{
                    //	voxelMap[x, y, z] = 3;
                    //}
                    //else//石头层
                    //{
                    //                   float noise = Noise.Perlin3D((float)x * 0.1f, (float)y * 0.1f, (float)z * 0.1f); // 将100改为0.1
                    //                   if (noise < 0.4)
                    //                   {
                    //                       voxelMap[x, y, z] = 4;
                    //                   }
                    //                   else
                    //                   {
                    //                       voxelMap[x, y, z] = 1;
                    //                   }
                    //               }



                    int randomInt = Random.Range(0, 2);

                    //判断基岩
                    if (y == 0)
					{
                        voxelMap[x, y, z] = 0;
                    }else if (y > 0 && y < 3 && randomInt == 1)
					{
                        voxelMap[x, y, z] = 0;
					}
					else
					{

						float noise2d_1 = Mathf.Lerp((float)(VoxelData.ChunkHeight/2 - 1), (float)VoxelData.ChunkHeight-1, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + chunkObject.transform.position.x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + chunkObject.transform.position.z * noise2d_scale_smooth));
                        float noise2d_2 = Mathf.Lerp((float)(VoxelData.ChunkHeight / 2 - 1), (float)VoxelData.ChunkHeight - 1, Mathf.PerlinNoise((float)x * noise2d_scale_steep + chunkObject.transform.position.x * noise2d_scale_steep, (float)z * noise2d_scale_steep + chunkObject.transform.position.z * noise2d_scale_steep));

						float noiseHigh = noise2d_1 * 0.7f + noise2d_2 * 0.3f;

                        //判断泥土
                        if (y > noiseHigh)
						{
                            voxelMap[x, y, z] = 4;
                        }else if ((y+1) > noiseHigh)
						{
                            voxelMap[x, y, z] = 2;
                        }else if (y > noiseHigh - 7)
						{
							voxelMap[x, y, z] = 3;
						}else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && randomInt == 1)
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





					//if ((x == 1 && y == 1 && z == 0) || (x == 1 && y == 1 && z == 1))
					//{
					//	voxelMap[x, y, z] = 4;
					//}

				}
			}
		}

	}

	//开始遍历
	void CreateMeshData () {

		for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {


					//if (x == 1 && y == 1 && z == 0)
					//{
					//	print("");
					//}

					AddVoxelDataToChunk (new Vector3(x, y, z));

				}
			}
		}

	}

	//面生成的判断
    //是方块吗----Y不绘制--N绘制
    //靠近边界----也返回N
    bool CheckVoxel (Vector3 pos,int _p) {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		//如果目标出界
		if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
		{
			//自己是不是空气
			if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4)
			{
				return true;
			}
			else
			{
                return false;
            }
		}//另判断自己和目标是不是空气
		else if((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && voxelMap[x,y,z] == 4)
		{
            return true;
        }

		//return voxelMap [x, y, z];
        return world.blocktypes[voxelMap [x, y, z]].isSolid;
    }

    //遍历中：：顺带判断面的生成方向
    void AddVoxelDataToChunk (Vector3 pos) {

		//判断六个面
		for (int p = 0; p < 6; p++) {


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




    public class ChunkCoord
    {

        public int x;
        public int z;

        public ChunkCoord(int _x, int _z)
        {

            x = _x;
            z = _z;

        }

        //public bool Equals(ChunkCoord other)
        //{

        //    if (other == null)
        //        return false;
        //    else if (other.x == x && other.z == z)
        //        return true;
        //    else
        //        return false;

        //}

    }

}
