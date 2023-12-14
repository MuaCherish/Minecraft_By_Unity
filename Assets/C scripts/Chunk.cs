using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    //���
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

	//Mesh�Ļ���
	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
	List<Vector2> uvs = new List<Vector2> ();

	//Block����������
    byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

	//World�ű�
	World world;

    //��������
    public float noise2d_scale_smooth = 0.05f;
    public float noise2d_scale_steep = 0.1f;
    public float noise3d_scale = 0.1f;

    //Start
    void Start () {

		//    for (int y = 0; y < 10; y++)
		//    {
		//        for (int z = 0; z < 10; z++)
		//        {
		//            for (int x = 0; x < 10; x++)
		//{
		//	float noise = Noise.Perlin3D((float)x * 0.1f, (float)y * 0.1f, (float)z * 0.1f); // ��100��Ϊ0.1

		//	if (noise < 0.4)
		//	{
		//		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		//		cube.transform.position = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);// ����һ���ʵ��ķŴ���
		//	}


		//}

		//        }
		//    }





		//��ȡWorld�ű�
		world = GameObject.Find("World").GetComponent<World>();

		//�ȴ�������
		PopulateVoxelMap();

		//��ʼ��������������
		CreateMeshData();

		//�һ���Թ���������
		CreateMesh();

	}

    //Block_Type���л�
    void PopulateVoxelMap () {

        for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {




                    //int randomInt2 = Random.Range(0, 4);

                    /*
					 ChunkHeight-8 ~ ChunkHeight������
					 else��ʯͷ
					 0 ~ 2�����Ҳ�
					 */

                    /*
					 0������
					 1��ʯͷ
					 2���ݵ�
					 3������
					 4������
					*/

                    ////���
                    //               if (y == VoxelData.ChunkHeight - 1)
                    //{
                    //	voxelMap[x, y, z] = 2;
                    //}//���Ҳ�
                    //else if (y == 0)
                    //{
                    //	voxelMap[x, y, z] = 0;
                    //}//���һ����
                    //else if (y > 0 && y < 3 && randomInt == 1)
                    //{
                    //	voxelMap[x, y, z] = 0;
                    //}//������
                    //else if (y > VoxelData.ChunkHeight - 6 && y < VoxelData.ChunkHeight)
                    //{
                    //	voxelMap[x, y, z] = 3;
                    //}//���������
                    //else if (y >= VoxelData.ChunkHeight - 8 && y <= VoxelData.ChunkHeight - 6 && randomInt == 1)
                    //{
                    //	voxelMap[x, y, z] = 3;
                    //}
                    //else//ʯͷ��
                    //{
                    //                   float noise = Noise.Perlin3D((float)x * 0.1f, (float)y * 0.1f, (float)z * 0.1f); // ��100��Ϊ0.1
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

                    //�жϻ���
                    if (y == 0)
					{
                        voxelMap[x, y, z] = 0;
                    }else if (y > 0 && y < 3 && randomInt == 1)
					{
                        voxelMap[x, y, z] = 0;
					}
					else
					{

						float noise2d_1 = Mathf.Lerp((float)(VoxelData.ChunkHeight/2 - 1), (float)VoxelData.ChunkHeight-1, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + transform.position.x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + transform.position.z * noise2d_scale_smooth));
                        float noise2d_2 = Mathf.Lerp((float)(VoxelData.ChunkHeight / 2 - 1), (float)VoxelData.ChunkHeight - 1, Mathf.PerlinNoise((float)x * noise2d_scale_steep + transform.position.x * noise2d_scale_steep, (float)z * noise2d_scale_steep + transform.position.z * noise2d_scale_steep));

						float noiseHigh = noise2d_1 * 0.7f + noise2d_2 * 0.3f;

                        //�ж�����
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



                            //�жϿ���
                            float noise3d = Noise.Perlin3D((float)x * noise3d_scale + transform.position.x * noise3d_scale, (float)y * noise3d_scale + transform.position.y * noise3d_scale, (float)z * noise3d_scale + transform.position.z * noise3d_scale); // ��100��Ϊ0.1

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

	//��ʼ����
	void CreateMeshData () {

		for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {


					if (x == 1 && y == 1 && z == 0)
					{
						print("");
					}

					AddVoxelDataToChunk (new Vector3(x, y, z));

				}
			}
		}

	}

	//�����ɵ��ж�
    //�Ƿ�����----Y������--N����
    //�����߽�----Ҳ����N
    bool CheckVoxel (Vector3 pos,int _p) {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		//���Ŀ�����
		if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
		{
			//�Լ��ǲ��ǿ���
			if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4)
			{
				return true;
			}
			else
			{
                return false;
            }
		}//���ж��Լ���Ŀ���ǲ��ǿ���
		else if((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && voxelMap[x,y,z] == 4)
		{
            return true;
        }

		//return voxelMap [x, y, z];
        return world.blocktypes[voxelMap [x, y, z]].isSolid;
    }

    //�����У���˳���ж�������ɷ���
    void AddVoxelDataToChunk (Vector3 pos) {

		//�ж�������
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

				//����p���ɶ�Ӧ���棬��Ӧ��UV
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

	//�������������
	void CreateMesh () {

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.uv = uvs.ToArray ();
        mesh.Optimize();
        mesh.RecalculateNormals ();
		meshFilter.mesh = mesh;

	}

	//�����桪��UV�Ĵ���
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
}
