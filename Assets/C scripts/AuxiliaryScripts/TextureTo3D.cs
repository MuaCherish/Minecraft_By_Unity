using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureTo3D : MonoBehaviour
{
    #region 纹理挤压算法


    [Header("初始化参数")]
    private float thickness = 0.01f;
    public Material material;
    public bool HandLayer;

    //找到边缘点
    private Texture2D spriteTexture;
    private List<Vector3> edgePoints = new List<Vector3>(); // 存储所有的边缘点

    //Mesh
    List<Vector3> points = new List<Vector3>();
    List<int> sequence = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int Totalsequence = 0;


    //public Sprite cloud;

    //private void Start()
    //{
    //    ProcessSprite(cloud, this.transform, 1, false);
    //}



    //创造3d挤压物体
    public void ProcessSprite(Sprite _sprite, Transform _partent, float _scale, bool _HandLayer)
    {
        if (_sprite == null) return;
        HandLayer = _HandLayer;

        // 清空已有的边缘点列表
        edgePoints.Clear();

        // 获取 sprite 的 texture 和 Rect
        spriteTexture = _sprite.texture;
        Rect spriteRect = _sprite.textureRect;

        // 重新检测边缘并绘制
        DetectEdges(spriteRect, _sprite);
        GenerateMeshData(_sprite, _scale, _partent); // 更新生成Mesh的逻辑
    }




    void DetectEdges(Rect spriteRect, Sprite _sprite)
    {
        // 获取 sprite 的宽度和高度
        int width = (int)spriteRect.width;
        int height = (int)spriteRect.height;

        // 遍历 sprite 的所有像素
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 获取当前像素的实际纹理坐标
                int pixelX = x + (int)spriteRect.x;
                int pixelY = y + (int)spriteRect.y;

                // 获取像素颜色
                Color currentPixel = spriteTexture.GetPixel(pixelX, pixelY);

                // 如果当前像素不是透明的，检查四周是否有透明像素
                if (currentPixel.a > 0.1f)
                {
                    // 检查是否为边缘像素
                    CheckAndStoreEdgePoints(x, y, width, height, _sprite);
                }
            }
        }
    }

    void CheckAndStoreEdgePoints(int x, int y, int width, int height, Sprite _sprite)
    {
        bool leftTransparent = IsTransparent(x - 1, y, width, height, _sprite);
        bool rightTransparent = IsTransparent(x + 1, y, width, height, _sprite);
        bool topTransparent = IsTransparent(x, y + 1, width, height, _sprite);
        bool bottomTransparent = IsTransparent(x, y - 1, width, height, _sprite);

        // 将当前像素的左侧两个顶点加入 edgePoints 列表
        if (leftTransparent)
        {
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // 左下角顶点
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // 左上角顶点
        }

        // 将当前像素的右侧两个顶点加入 edgePoints 列表
        if (rightTransparent)
        {
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // 右下角顶点
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // 右上角顶点
        }

        // 将当前像素的上侧两个顶点加入 edgePoints 列表
        if (topTransparent)
        {
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // 左上角顶点
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // 右上角顶点
        }

        // 将当前像素的下侧两个顶点加入 edgePoints 列表
        if (bottomTransparent)
        {
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // 左下角顶点
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // 右下角顶点
        }
    }

    void StoreEdgePoint(Vector3 point)
    {
        edgePoints.Add(point);
    }

    bool IsTransparent(int x, int y, int width, int height, Sprite _sprite)
    {
        // 检查是否超出纹理范围，若超出则视为透明
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return true;
        }

        int pixelX = x + (int)_sprite.textureRect.x;
        int pixelY = y + (int)_sprite.textureRect.y;

        Color pixelColor = spriteTexture.GetPixel(pixelX, pixelY);
        return pixelColor.a <= 0.1f;
    }

    void DrawEdgeLines()
    {
        // 将 edgePoints 转为数组：edgePointsBack[] (背面)
        Vector3[] edgePointsBack = edgePoints.ToArray();

        // 创建 edgePointsFront[] (前面)，它的元素为 edgePointsBack 里的 Vector3 的 z 加上 thickness
        Vector3[] edgePointsFront = new Vector3[edgePointsBack.Length];
        for (int i = 0; i < edgePointsBack.Length; i++)
        {
            edgePointsFront[i] = edgePointsBack[i] + new Vector3(0, 0, thickness);
        }

        //// DrawBack
        //for (int i = 0; i < edgePoints.Count - 1; i += 2) // 每次连接两个点
        //{
        //    Vector3 pointA = edgePointsBack[i];
        //    Vector3 pointB = edgePointsBack[i + 1];
        //    //print($"{pointA}+{pointB}");
        //    Debug.DrawLine(pointA, pointB, Color.red, 100f); // 将这些点连接成线段
        //}


        ////DrawFront
        //for (int i = 0; i < edgePoints.Count - 1; i += 2) // 每次连接两个点
        //{
        //    Vector3 pointA = edgePointsFront[i];
        //    Vector3 pointB = edgePointsFront[i + 1];
        //    //print($"{pointA}+{pointB}");
        //    Debug.DrawLine(pointA, pointB, Color.red, 100f); // 将这些点连接成线段
        //}

        //DrawLateral
        //for (int i = 0; i < edgePoints.Count ; i ++) // 每次连接两个点
        //{
        //    Vector3 pointA = edgePointsBack[i];
        //    Vector3 pointB = edgePointsFront[i];
        //    //print($"{pointA}+{pointB}");
        //    Debug.DrawLine(pointA, pointB, Color.red, 100f); // 将这些点连接成线段 
        //}


    }



    void GenerateMeshData(Sprite _sprite, float _scale, Transform _parent)
    {

        ClearMeshData(_parent);

        //material.mainTexture = sprite.texture;

        // 将 edgePoints 转为数组：edgePointsBack[] (背面)
        Vector3[] edgePointsBack = edgePoints.ToArray();

        // 创建 edgePointsFront[] (前面)，它的元素为 edgePointsBack 里的 Vector3 的 z 加上 thickness
        Vector3[] edgePointsFront = new Vector3[edgePointsBack.Length];
        for (int i = 0; i < edgePointsBack.Length; i++)
        {
            edgePointsFront[i] = edgePointsBack[i] + new Vector3(0, 0, thickness);
        }

        for (int i = 0; i < edgePoints.Count - 1; i += 2) // 每次连接两个点
        {
            points.Add(edgePointsBack[i]);
            points.Add(edgePointsBack[i + 1]);
            points.Add(edgePointsFront[i]);
            points.Add(edgePointsFront[i + 1]);

            sequence.Add(Totalsequence);        //0 
            sequence.Add(Totalsequence + 1);    //1
            sequence.Add(Totalsequence + 3);    //3
            sequence.Add(Totalsequence + 3);    //3
            sequence.Add(Totalsequence + 2);    //2
            sequence.Add(Totalsequence);        //0
            Totalsequence += 4;

            // 根据 X 值调整 UV 坐标，以便在 X 方向上拉伸材质
            //float uvXBack0 = edgePointsBack[i].x / thickness; // 以 thickness 为参考
            //float uvXBack1 = edgePointsBack[i + 1].x / thickness;
            //float uvXFront0 = edgePointsFront[i].x / thickness;
            //float uvXFront1 = edgePointsFront[i + 1].x / thickness;

            // 更新 UV 坐标
            //uvs.Add(new Vector2(uvXBack0, 0.5f));
            //uvs.Add(new Vector2(uvXBack1, 0.5f));
            //uvs.Add(new Vector2(uvXFront0, 0.5f));
            //uvs.Add(new Vector2(uvXFront1, 0.5f));
            AddTexture(edgePointsBack[i], edgePointsBack[i + 1], edgePointsFront[i], edgePointsFront[i + 1]);


            points.Add(edgePointsBack[i]);
            points.Add(edgePointsBack[i + 1]);
            points.Add(edgePointsFront[i]);
            points.Add(edgePointsFront[i + 1]);

            sequence.Add(Totalsequence);        //0 
            sequence.Add(Totalsequence + 2);    //2
            sequence.Add(Totalsequence + 3);    //3
            sequence.Add(Totalsequence + 3);    //3
            sequence.Add(Totalsequence + 1);    //1
            sequence.Add(Totalsequence);        //0
            Totalsequence += 4;

            // 更新 UV 坐标
            //uvs.Add(new Vector2(uvXBack0, 0.5f));
            //uvs.Add(new Vector2(uvXBack1, 0.5f));
            //uvs.Add(new Vector2(uvXFront0, 0.5f));
            //uvs.Add(new Vector2(uvXFront1, 0.5f));
            AddTexture(edgePointsBack[i], edgePointsBack[i + 1], edgePointsFront[i], edgePointsFront[i + 1]);
        }




        CreateMesh(_sprite, _scale, _parent);
    }

    //void AddTexture()
    //{
    //    float x = 1.0f / 16.0f;
    //    float y = 11.0f / 16.0f;
    //    float height = 1.0f / 16.0f;

    //    uvs.Add(new Vector2(x, y));
    //    uvs.Add(new Vector2(x, y + height));
    //    uvs.Add(new Vector2(x + height, y));
    //    uvs.Add(new Vector2(x + height, y + height));

    //}

    void AddTexture(Vector3 backPoint0, Vector3 backPoint1, Vector3 frontPoint0, Vector3 frontPoint1)
    {
        //跟xy没关系了
        float x = 1.0f / 16.0f;  // UV 坐标的起始 x 坐标（基础）
        float y = 11.0f / 16.0f; // UV 坐标的起始 y 坐标（基础）
        float height = 1.0f / 16.0f; // 纹理集中每个纹理的高度

        // 根据 X 方向的拉伸，计算新的 UV 宽度
        float uvXBack0 = backPoint0.x / thickness;  // 使用 X 方向上的位置来调整 x 值
        float uvXBack1 = backPoint1.x / thickness;
        float uvXFront0 = frontPoint0.x / thickness;
        float uvXFront1 = frontPoint1.x / thickness;

        // 根据 X 值更新 UV 坐标，使得材质在 X 方向上拉伸
        uvs.Add(new Vector2(x + uvXBack0 * height, y));      // 左下角
        uvs.Add(new Vector2(x + uvXBack1 * height, y + height)); // 左上角
        uvs.Add(new Vector2(x + uvXFront0 * height, y));     // 右下角
        uvs.Add(new Vector2(x + uvXFront1 * height, y + height)); // 右上角
    }

    void ClearMeshData(Transform _parent)
    {
        // 清空之前的点、UV 和三角形索引列表
        points.Clear();
        uvs.Clear();
        sequence.Clear();
        Totalsequence = 0;

        foreach (Transform temp in _parent.transform)
        {
            Destroy(temp.gameObject);
        }

    }

    void CreateMesh(Sprite _sprite, float _scale, Transform _parent)
    {
        float spriteWidth = _sprite.rect.width;
        //print($"像素大小：{spriteWidth} , 偏移大小：{spriteWidth / 200f}");

        // 父类
        GameObject parent = new GameObject("挤压物体");
        parent.transform.localScale = new Vector3(_scale, _scale, _scale);
        parent.transform.SetParent(_parent, false);

        // 添加正反面
        GameObject front = new GameObject("正面");
        front.AddComponent<SpriteRenderer>().sprite = _sprite;
        front.transform.position = new Vector3(spriteWidth / 200f, spriteWidth / 200f, thickness);
        front.transform.SetParent(parent.transform, false);
         
        GameObject back = new GameObject("反面");
        back.AddComponent<SpriteRenderer>().sprite = _sprite;
        back.transform.position = new Vector3(spriteWidth / 200f, spriteWidth / 200f, 0);
        back.transform.SetParent(parent.transform, false);

        // 创建一个新的空 GameObject
        GameObject meshObject = new GameObject("侧面");
        meshObject.transform.SetParent(parent.transform, false);

        if (HandLayer)
        {
            parent.layer = LayerMask.NameToLayer("Hand");
            front.layer = LayerMask.NameToLayer("Hand");
            back.layer = LayerMask.NameToLayer("Hand");
            meshObject.layer = LayerMask.NameToLayer("Hand");
        }

        // 创建一个新的 Mesh
        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.triangles = sequence.ToArray();
        mesh.uv = uvs.ToArray();

        // 计算法线和边界
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // 为新 GameObject 添加 MeshFilter 和 MeshRenderer 组件
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

        // 将生成的 Mesh 赋值到 MeshFilter
        meshFilter.mesh = mesh;

        // 为每个 meshObject 创建一个独立的材质实例
        Material newMaterial = Instantiate(material);

        // 设置新材质到 MeshRenderer
        meshRenderer.material = newMaterial;

        // 获取Sprite的纹理
        Texture2D spriteTexture = _sprite.texture;

        // 将Sprite的纹理赋值给新材质
        newMaterial.SetTexture("_MainTex", spriteTexture);

        // 如果Sprite有特殊的UV范围，比如是纹理集的一部分，设置UV偏移和缩放
        Vector4 textureScaleOffset = new Vector4(
            _sprite.rect.width / spriteTexture.width,  // UV宽度比例
            _sprite.rect.height / spriteTexture.height,  // UV高度比例
            _sprite.rect.x / spriteTexture.width,  // UV X偏移
            _sprite.rect.y / spriteTexture.height   // UV Y偏移
        );

        newMaterial.SetTextureScale("_MainTex", new Vector2(textureScaleOffset.x, textureScaleOffset.y));
        newMaterial.SetTextureOffset("_MainTex", new Vector2(textureScaleOffset.z, textureScaleOffset.w));

        // 可选：如果需要为新 GameObject 添加额外组件，比如碰撞体
        // meshObject.AddComponent<MeshCollider>();
    }



    #endregion

}
