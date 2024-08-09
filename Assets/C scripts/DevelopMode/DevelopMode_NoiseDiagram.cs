using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class DevelopMode_NoiseDiagram : MonoBehaviour
{
    public int textureSize;        // 纹理的大小
    public DevelopModeWorld world;       // 引用到噪声生成的世界对象
    private Texture2D noiseTexture;      // 噪声纹理
    private Renderer Noiserenderer;           // 渲染器组件
    public GameObject thisobject;

    public DevelopMode_NoiseDiagram(DevelopModeWorld _world)
    {
        //World
        world = _world;

        //计算像素
        textureSize = 16 * world.RenderWidth;

        // 创建一个新的Plane对象
        thisobject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        thisobject.transform.position = new Vector3(world.RenderWidth * 8f, 36f, world.RenderWidth * 8f); // 设置Plane的位置
        thisobject.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        thisobject.transform.localScale = new Vector3(0.1f * 16f * world.RenderWidth, 0.1f * 16f * world.RenderWidth, 0.1f * 16f * world.RenderWidth); // 设置Plane的缩放
        thisobject.transform.SetParent(world.ChunkPATH.transform);
        thisobject.name = "NoiseDiagram--0,0";
        Noiserenderer = thisobject.GetComponent<Renderer>();

        // 创建并应用噪声纹理
        noiseTexture = new Texture2D(textureSize, textureSize);
        Noiserenderer.material.mainTexture = noiseTexture;

        // 生成纹理
        GenerateNoiseTexture();
    }


    void GenerateNoiseTexture()
    {
        for (int x = 0; x < textureSize; x++)
        {
            for (int z = 0; z < textureSize; z++)
            {

                // 温度
                float A = world.GetSimpleNoise((int)x, (int)z, new Vector3(0f, 0f, 0f));
                float B = world.GetSimpleNoise((int)x, (int)z, new Vector3(100f, 0f, 100f));
                float C = world.GetSimpleNoise((int)x, (int)z, new Vector3(300f, 0f, 420f));
                float D = world.GetSimpleNoise((int)x, (int)z, new Vector3(520f, 0f, 520f));

                //湿度
                //float N  = world.GetSimpleNoise((int)x, (int)z, new Vector3(100f, 0f, 100f));
                //print($"{T},{N}");
                // 设置纹理像素
                noiseTexture.SetPixel(x, z, simpleRenderColor(A,B,C,D));
            }
        }

        // 应用更改
        noiseTexture.Apply();
    }


    //群系染色
    public Color BiomeRenderColor(float _noisevalue)
    {

        foreach (var temp in world.biomeclarify)
        {
            //如果在区间内，则染色
            if (_noisevalue >= temp.Domain)
            {
                return temp.color;
            }
        }
        return Color.gray;

    }

    public Color simpleRenderColor(float _A, float _B, float _C, float _D)  
    {
        //沙漠
        if (_C >= world.干燥程度Aridity)
        {
           
            return world.biomeclarify[2].color;

        }

        else
        {

            //高原
            if (_B >= world.三维密度Density3d)
            {
                return world.biomeclarify[1].color;
            }

            //草原
            else if (_A >= world.氧气浓度OxygenDensity)
            {

                if (_D >= world.空气湿度MoistureLevel)
                {
                    return world.biomeclarify[3].color;
                }
                else
                {
                    return world.biomeclarify[0].color;
                }

                

            }
            else
            {
                return world.biomeclarify[0].color;
            }

        }


    }

    //温度和湿度测试
    //public Color NandTRenderColor(float _T, float _N)
    //{
    //    //*平原群系：高温 + 高湿： 淡绿色
    //    //    * 基础高度： 30～50s
    //    if (_T >= world.草原min && _N >= world.草原max)
    //    {
    //        return world.biomeclarify[0].color;
    //    }
    //    //* 高原群系：高温 + 低湿： 深棕色
    //    //    * 基础高度： 100～200
    //    else if (_T >= world.高原min && _N < world.高原max)
    //    {
    //        return world.biomeclarify[1].color;
    //    }
    //    //* 沙漠群系：低温 + 低湿： 亮黄色
    //    //    * 基础高度： 30～50
    //    else if (_T < world.沙漠min && _N < world.沙漠max)
    //    {
    //        return world.biomeclarify[2].color;
    //    }
    //    //* 沼泽群系：低温 + 高湿： 深绿色
    //    //    * 基础高度： 30～40
    //    else if (_T < world.沼泽min && _N >= world.沼泽max)
    //    {
    //        return world.biomeclarify[3].color;
    //    }
    //    else
    //    {
    //        return world.biomeclarify[0].color;
    //    }

    //}


    public bool isAlive()
    {
        if (thisobject == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void DestroySelf()
    {
        Destroy(thisobject);
    }
}
