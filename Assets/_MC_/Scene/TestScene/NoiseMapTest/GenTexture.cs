using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoiseMapTest
{
    public static class GenTexture
    {
        private static Vector2Int _TextureSize = new Vector2Int(16, 16);

        public static Texture2D GetBlackWhiteTexture()
        {
            Texture2D texture = new Texture2D(_TextureSize.x, _TextureSize.y);
            texture.filterMode = FilterMode.Point;

            int halfWidth = _TextureSize.x / 2;
            int halfHeight = _TextureSize.y / 2;

            for (int y = 0; y < _TextureSize.y; y++)
            {
                for (int x = 0; x < _TextureSize.x; x++)
                {
                    // ���㵱ǰ���ص��ĸ��߽�Ĺ�һ�����루��Χ 0~1��
                    float distLeft = (float)x / halfWidth;
                    float distRight = (float)(_TextureSize.x - x) / halfWidth;
                    float distBottom = (float)y / halfHeight;
                    float distTop = (float)(_TextureSize.y - y) / halfHeight;

                    // ȡ�ĸ��������Сֵ���γɷ��ν���
                    float t = Mathf.Min(distLeft, distRight, distBottom, distTop);

                    // ������ɫ�����İף��߽�ڣ�
                    Color color = Color.Lerp(Color.black, Color.white, t);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        public static Texture2D GetPerlinNoiseTexture(float[,] heightMap)
        {
            //��ȡ���Ϳ�
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            //�����Ҷ�ͼ�����ݰ���������ֵ���ɶ�Ӧ����ɫ
            Color[] colourMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                }
            }

            return TextureFromColourMap(colourMap, width, height);
        }

        public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }
    }

}

