using UnityEngine;
using NoiseMapTest;

namespace NoiseMapTest
{

    public static class GenNoise 
    {
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
        public static float[,] GenerateNoiseMap(Vector2 chunkCoord, int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
        {
            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float[,] noiseMap = new float[mapWidth, mapHeight];

            if (scale <= 0)
                scale = 0.0001f;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            // ������������Ļ�׼�㣨�������飩
            float worldStartX = chunkCoord.x * mapWidth;
            float worldStartY = chunkCoord.y * mapHeight;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    // �����������꣨ȷ�����룩
                    float worldX = (worldStartX + x) / scale;
                    float worldY = (worldStartY + y) / scale;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (worldX * frequency + octaveOffsets[i].x) / mapWidth;
                        float sampleY = (worldY * frequency + octaveOffsets[i].y) / mapHeight;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                    if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

            // ��һ��
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }

    }

}
