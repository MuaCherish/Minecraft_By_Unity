using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
//using static UnityEditor.PlayerSettings;

public class DevelopMode_NoiseDiagram : MonoBehaviour
{
    public int textureSize;        // 纹理的大小
    public DevelopModeWorld world;       // 引用到噪声生成的世界对象
    private Texture2D noiseTexture;      // 噪声纹理
    private Renderer Noiserenderer;           // 渲染器组件
    public GameObject thisobject;
    System.Random rand;

    public DevelopMode_NoiseDiagram(DevelopModeWorld _world)
    {
        //World
        world = _world;

        //计算像素
        textureSize = 16 * world.RenderWidth;
        rand = new System.Random(world.terrainLayerProbabilitySystem.Seed);

        // 创建一个新的Plane对象
        thisobject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        thisobject.transform.position = new Vector3(world.RenderWidth * 8f, 60f, world.RenderWidth * 8f); // 设置Plane的位置
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
                //float noisevalue = world.GetSimpleNoiseWithOffset(x,z,new Vector3(0,0,0), new Vector2(111f,222f), world.Noise_Scale);

                //noiseTexture.SetPixel(x, z, BiomeRenderColor(noisevalue));
                noiseTexture.SetPixel(x, z, GetBiome(x, z, new Vector3(0,0,0)));
            }
        } 

        // 应用更改
        noiseTexture.Apply();
    }


    //根据测试集染色
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

     
    //根据群系染色
    public Color GetBiome(int _x, int _z, Vector3 _myposition)
    {

        float _A = world.GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
        float _B = world.GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
        float _C = world.GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
        float _D = world.GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

        ////沙漠
        //if (_C >= world.干燥程度Aridity)
        //{

        //    return world.biomeclarify[2].color;

        //}

        //else
        //{

        //    //高原
        //    if (_B >= world.三维密度Density3d)
        //    {
        //        return world.biomeclarify[1].color;
        //    }

        //    //草原
        //    else if (_A >= world.氧气浓度OxygenDensity)
        //    {

        //        if (_D >= world.空气湿度MoistureLevel)
        //        {
        //            return world.biomeclarify[3].color;
        //        }
        //        else
        //        {
        //            return world.biomeclarify[0].color;
        //        }



        //    }
        //    else
        //    {
        //        return world.biomeclarify[4].color;
        //    }

        //}

        //高原
        if (_B >= world.三维密度Density3d)
        {

            return world.biomeclarify[1].color;

        }

        else
        {

            //沙漠
            if (_C >= world.干燥程度Aridity)
            {
                return world.biomeclarify[2].color;
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
                return world.biomeclarify[4].color;
            }

        }



    }




    #region tools

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

    #endregion

}
