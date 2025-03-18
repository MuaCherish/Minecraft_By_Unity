using UnityEngine;
using NoiseMapTest;
using UnityEngine.ProBuilder;

namespace NoiseMapTest
{

    public static class GenNoise 
    {

        public enum NormalizeMode
        {
            Local, Global
        };

        /// <summary>
        /// ��������ͼ
        /// </summary>
        /// <param name="chunkCoord">��������</param>
        /// <param name="mapWidth"></param>
        /// <param name="mapHeight"></param>
        /// <param name="scale"></param>
        /// <param name="octaves">�˶����׵�����</param>
        /// <param name="persistance">�־���</param>
        /// <param name="lacunarity">��϶��</param>
        /// <returns></returns>
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
        {
            //�趨����
            System.Random prng = new System.Random(seed);

            //�˶�����ƫ��
            Vector2[] octaveOffsets = new Vector2[octaves];

            float maxPossibleHeight = 0f;
            float amplitude = 1; //���
            float frequency = 1; //Ƶ��

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude += persistance;
            }

            //��������
            float[,] noiseMap = new float[mapWidth, mapHeight];

            //�������-scaleΪ0�����
            if (scale <= 0)
                scale = 0.0001f;

            //�ҵ������������ֵ
            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2;
            float halfHeight = mapHeight / 2;

            //������ά����,��������ֵ
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {

                    //���β���
                    amplitude = 1; //���
                    frequency = 1; //Ƶ��
                    float noiseHeight = 0; //���ٵ�ǰ�ĸ߶�ֵ

                    //�������а˶�����
                    for (int i = 0; i < octaves; i++)
                    {
                        //������                
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;  //Ƶ��Խ�ߣ�������֮��ľ���ԽԶ
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                        //��ȡPerlinNoise
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; //����Χ������[-1, 1]

                        //�����ܹ���ֵ
                        // noiseHeight = 0.6 * <1> + 0.4 * <0.5> + 0.2 * <0.25> = 0.85��ÿһ��Ĺ���ֵ���������
                        noiseHeight += perlinValue * amplitude;

                        //������һ�������������Ƶ��
                        amplitude *= persistance; //�𽥼�Сÿһ����������������ͼ������ƽ���̶Ⱥ�ϸ�ڱ�����
                        frequency *= lacunarity; //������ÿһ���Ƶ�ʣ���������ͼ��ϸ���ܶȣ����Ӹ�Ƶ������Ӱ�졣
                    }

                    //Ѱ�������С����ֵ
                    if (noiseHeight > maxLocalNoiseHeight)
                        maxLocalNoiseHeight = noiseHeight;
                    else if (noiseHeight < minLocalNoiseHeight)
                        minLocalNoiseHeight = noiseHeight;

                    //��ֵ
                    noiseMap[x, y] = noiseHeight;

                }
            }

            //��һ������
            //ֵ�ᳬ��01��Χ
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (normalizeMode == NormalizeMode.Local)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    }
                    else if (normalizeMode == NormalizeMode.Global)
                    {
                        float normalizeHeight = (noiseMap[x,y] + 1) / (2f * maxPossibleHeight / 16f);
                        noiseMap[x, y] = normalizeHeight;
                    }

                    
                }
            }

            //����
            return noiseMap;
        }

    }

}
