using UnityEngine;
using Random = System.Random;

namespace NoiseMapTest
{
    public static class GenVoronoi
    {
        private static int DistanceSqr(Vector2Int a, Vector2Int b)
        {
            return (a - b).sqrMagnitude;
        }

        // ��ȾVoronoiͼ��������Texture2D
        public static Texture2D RenderVoronoiGraph(int width, int height, int seed, int[] units)
        {
            var tex = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point
            };

            for (var x = 0; x < width; ++x)
            {
                for (var y = 0; y < height; ++y)
                {
                    tex.SetPixel(x, y, GetTextureColorAt(seed, units, 0, x, y));
                }
            }
            tex.Apply();
            return tex;
        }

        // ��ȡĳ�������ɫ
        private static Color GetTextureColorAt(int seed, int[] units, int level, int x, int y)
        {
            if (level == units.Length - 1)
            {
                return GetClosestRootColorFrom(seed, units, level, x, y);
            }

            var next = GetClosestRootPositionFrom(seed, units, level, x, y);
            return GetTextureColorAt(seed, units, level + 1, next.x, next.y);
        }

        // ��ȡĳ��Cell�ĸ�����ɫ
        private static Color GetColorOfCellRoot(int seed, int level, int cell_x, int cell_y)
        {
            var rand = new Random(cell_x ^ cell_y * 0x123456 + level + (seed << 2) + 66666);
            return new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
        }

        // ��ȡCell��λ��
        private static Vector2Int GetCellRootPosition(int seed, int level, int cell_x, int cell_y, int[] units)
        {
            var rand = new Random((((level ^ 0xe12345) * cell_x + cell_y) ^ 0xabcdef) + level * level + seed + 23333);
            return new Vector2Int(
                (cell_x << units[level]) + rand.Next(1 << units[level]),
                (cell_y << units[level]) + rand.Next(1 << units[level]));
        }

        // ��ȡ��������ĸ�λ��
        private static Vector2Int GetClosestRootPositionFrom(int seed, int[] units, int level, int x, int y)
        {
            int cx = x >> units[level], cy = y >> units[level];
            var min_dist = int.MaxValue;
            var ret = Vector2Int.zero;

            for (var i = -1; i <= 1; ++i)
            {
                for (var j = -1; j <= 1; ++j)
                {
                    int ax = cx + i, ay = cy + j;
                    var pos = GetCellRootPosition(seed, level, ax, ay, units);
                    var dist = DistanceSqr(pos, new Vector2Int(x, y));
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        ret = pos;
                    }
                }
            }
            return ret;
        }

        // ��ȡ���������ɫ
        private static Color GetClosestRootColorFrom(int seed, int[] units, int level, int x, int y)
        {
            int cx = x >> units[level], cy = y >> units[level];
            var min_dist = int.MaxValue;
            var ret = Color.red;

            for (var i = -1; i <= 1; ++i)
            {
                for (var j = -1; j <= 1; ++j)
                {
                    int ax = cx + i, ay = cy + j;
                    var pos = GetCellRootPosition(seed, level, ax, ay, units);
                    var color = GetColorOfCellRoot(seed, level, ax, ay);
                    var dist = DistanceSqr(pos, new Vector2Int(x, y));
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        ret = color;
                    }
                }
            }
            return ret;
        }
    }
}
