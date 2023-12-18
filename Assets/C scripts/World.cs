using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class World : MonoBehaviour
{


    public Material material;
    public BlockType[] blocktypes;

    public float noise2d_scale_smooth = 0.05f;
    public float noise2d_scale_steep = 0.02f;
    public float noise3d_scale = 0.05f;
    //hunk newChunk = new Chunk(new Chunk.ChunkCoord(i, j), this, noise2d_scale_smooth, noise2d_scale_steep, noise3d_scale);

    private void Start()
    {
        StartCoroutine(SpawnChunksWithDelay());
    }


    IEnumerator SpawnChunksWithDelay()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Chunk newChunk = new Chunk(new Chunk.ChunkCoord(i, j), this, noise2d_scale_smooth, noise2d_scale_steep, noise3d_scale);
                yield return new WaitForSeconds(0.8f);
            }


            // 等待1秒钟
            //yield return new WaitForSeconds(1f);
        }
    }


}

//结构体BlockType
//存储方块种类+面对应的UV
[System.Serializable]
public class BlockType
{

    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    //贴图中的面的坐标
    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }

}