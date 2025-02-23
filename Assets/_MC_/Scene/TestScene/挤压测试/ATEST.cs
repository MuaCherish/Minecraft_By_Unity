using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine.UIElements;
//using UnityEngine.ProBuilder.Shapes;

public class ATEST : MonoBehaviour
{

    #region ����ѹ�㷨


    [Header("��ʼ������")]
    private float thickness = 0.01f;
    public Sprite Sprite; // �� Unity �༭�������� sprite ��Դ
    public Material material;
    public Transform FATHER_PATH;
    public float SCALE = 0.01f;

    //�ҵ���Ե��
    private Texture2D spriteTexture;
    private List<Vector3> edgePoints = new List<Vector3>(); // �洢���еı�Ե��

    //Mesh
    List<Vector3> points = new List<Vector3>();
    List<int> sequence = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int Totalsequence = 0;


    private void Start()
    {
        ProcessSprite(Sprite, FATHER_PATH, SCALE);
    }


    //����3d��ѹ����
    void ProcessSprite(Sprite _sprite, Transform _partent, float _scale)
    {
        if (_sprite == null) return;


        // ������еı�Ե���б�
        edgePoints.Clear();

        // ��ȡ sprite �� texture �� Rect
        spriteTexture = _sprite.texture;
        Rect spriteRect = _sprite.textureRect;

        // ���¼���Ե������
        DetectEdges(spriteRect, _sprite);
        GenerateMeshData(_sprite, _scale); // ��������Mesh���߼�
    }



    
    void DetectEdges(Rect spriteRect, Sprite _sprite)
    {
        // ��ȡ sprite �Ŀ�Ⱥ͸߶�
        int width = (int)spriteRect.width;
        int height = (int)spriteRect.height;

        // ���� sprite ����������
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // ��ȡ��ǰ���ص�ʵ����������
                int pixelX = x + (int)spriteRect.x;
                int pixelY = y + (int)spriteRect.y;

                // ��ȡ������ɫ
                Color currentPixel = spriteTexture.GetPixel(pixelX, pixelY);

                // �����ǰ���ز���͸���ģ���������Ƿ���͸������
                if (currentPixel.a > 0.1f)
                {
                    // ����Ƿ�Ϊ��Ե����
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

        // ����ǰ���ص��������������� edgePoints �б�
        if (leftTransparent)
        {
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // ���½Ƕ���
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // ���ϽǶ���
        }

        // ����ǰ���ص��Ҳ������������ edgePoints �б�
        if (rightTransparent)
        {
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // ���½Ƕ���
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // ���ϽǶ���
        }

        // ����ǰ���ص��ϲ������������ edgePoints �б�
        if (topTransparent)
        {
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // ���ϽǶ���
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, (y + 1) / _sprite.pixelsPerUnit, 0)); // ���ϽǶ���
        }

        // ����ǰ���ص��²������������ edgePoints �б�
        if (bottomTransparent)
        {
            StoreEdgePoint(new Vector3(x / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // ���½Ƕ���
            StoreEdgePoint(new Vector3((x + 1) / _sprite.pixelsPerUnit, y / _sprite.pixelsPerUnit, 0)); // ���½Ƕ���
        }
    }

    void StoreEdgePoint(Vector3 point)
    {
        edgePoints.Add(point);
    }

    bool IsTransparent(int x, int y, int width, int height, Sprite _sprite)
    {
        // ����Ƿ񳬳�����Χ������������Ϊ͸��
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
        // �� edgePoints תΪ���飺edgePointsBack[] (����)
        Vector3[] edgePointsBack = edgePoints.ToArray();

        // ���� edgePointsFront[] (ǰ��)������Ԫ��Ϊ edgePointsBack ��� Vector3 �� z ���� thickness
        Vector3[] edgePointsFront = new Vector3[edgePointsBack.Length];
        for (int i = 0; i < edgePointsBack.Length; i++)
        {
            edgePointsFront[i] = edgePointsBack[i] + new Vector3(0, 0, thickness);
        }

        //// DrawBack
        //for (int i = 0; i < edgePoints.Count - 1; i += 2) // ÿ������������
        //{
        //    Vector3 pointA = edgePointsBack[i];
        //    Vector3 pointB = edgePointsBack[i + 1];
        //    //print($"{pointA}+{pointB}");
        //    Debug.DrawLine(pointA, pointB, Color.red, 100f); // ����Щ�����ӳ��߶�
        //}


        ////DrawFront
        //for (int i = 0; i < edgePoints.Count - 1; i += 2) // ÿ������������
        //{
        //    Vector3 pointA = edgePointsFront[i];
        //    Vector3 pointB = edgePointsFront[i + 1];
        //    //print($"{pointA}+{pointB}");
        //    Debug.DrawLine(pointA, pointB, Color.red, 100f); // ����Щ�����ӳ��߶�
        //}

        //DrawLateral
        //for (int i = 0; i < edgePoints.Count ; i ++) // ÿ������������
        //{
        //    Vector3 pointA = edgePointsBack[i];
        //    Vector3 pointB = edgePointsFront[i];
        //    //print($"{pointA}+{pointB}");
        //    Debug.DrawLine(pointA, pointB, Color.red, 100f); // ����Щ�����ӳ��߶� 
        //}


    }

    

    void GenerateMeshData(Sprite _sprite, float _scale)
    {
        
        ClearMeshData();

        //material.mainTexture = sprite.texture;

        // �� edgePoints תΪ���飺edgePointsBack[] (����)
        Vector3[] edgePointsBack = edgePoints.ToArray();

        // ���� edgePointsFront[] (ǰ��)������Ԫ��Ϊ edgePointsBack ��� Vector3 �� z ���� thickness
        Vector3[] edgePointsFront = new Vector3[edgePointsBack.Length];
        for (int i = 0; i < edgePointsBack.Length; i++)
        {
            edgePointsFront[i] = edgePointsBack[i] + new Vector3(0, 0, thickness);
        }

        for (int i = 0; i < edgePoints.Count - 1; i += 2) // ÿ������������
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

            // ���� X ֵ���� UV ���꣬�Ա��� X �������������
            //float uvXBack0 = edgePointsBack[i].x / thickness; // �� thickness Ϊ�ο�
            //float uvXBack1 = edgePointsBack[i + 1].x / thickness;
            //float uvXFront0 = edgePointsFront[i].x / thickness;
            //float uvXFront1 = edgePointsFront[i + 1].x / thickness;

            // ���� UV ����
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

            // ���� UV ����
            //uvs.Add(new Vector2(uvXBack0, 0.5f));
            //uvs.Add(new Vector2(uvXBack1, 0.5f));
            //uvs.Add(new Vector2(uvXFront0, 0.5f));
            //uvs.Add(new Vector2(uvXFront1, 0.5f));
            AddTexture(edgePointsBack[i], edgePointsBack[i + 1], edgePointsFront[i], edgePointsFront[i + 1]);
        }
         

        

        CreateMesh(_sprite, _scale);
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
        //��xyû��ϵ��
        float x = 1.0f / 16.0f;  // UV �������ʼ x ���꣨������
        float y = 11.0f / 16.0f; // UV �������ʼ y ���꣨������
        float height = 1.0f / 16.0f; // ������ÿ������ĸ߶�

        // ���� X ��������죬�����µ� UV ���
        float uvXBack0 = backPoint0.x / thickness;  // ʹ�� X �����ϵ�λ�������� x ֵ
        float uvXBack1 = backPoint1.x / thickness;
        float uvXFront0 = frontPoint0.x / thickness;
        float uvXFront1 = frontPoint1.x / thickness;

        // ���� X ֵ���� UV ���꣬ʹ�ò����� X ����������
        uvs.Add(new Vector2(x + uvXBack0 * height, y));      // ���½�
        uvs.Add(new Vector2(x + uvXBack1 * height, y + height)); // ���Ͻ�
        uvs.Add(new Vector2(x + uvXFront0 * height, y));     // ���½�
        uvs.Add(new Vector2(x + uvXFront1 * height, y + height)); // ���Ͻ�
    }

    void ClearMeshData()
    {
        // ���֮ǰ�ĵ㡢UV �������������б�
        points.Clear();
        uvs.Clear();
        sequence.Clear();
        Totalsequence = 0;

        foreach (Transform temp in FATHER_PATH.transform)
        {
            Destroy(temp.gameObject);
        }

    }

    void CreateMesh(Sprite _sprite, float _scale)
    {
        //����
        GameObject parent = new GameObject("��ѹ����");
        parent.transform.localScale = new Vector3(_scale, _scale, _scale);
        parent.transform.SetParent(FATHER_PATH, false);

        //���������
        GameObject front = new GameObject("����");
        front.AddComponent<SpriteRenderer>().sprite = _sprite;
        front.transform.position = new Vector3(0.08f, 0.08f,thickness);
        front.transform.SetParent(parent.transform, false);

        GameObject back = new GameObject("����");
        back.AddComponent<SpriteRenderer>().sprite = _sprite;
        back.transform.position = new Vector3(0.08f, 0.08f, 0);
        back.transform.SetParent(parent.transform, false);


        // ����һ���µĿ� GameObject
        GameObject meshObject = new GameObject("����");
        meshObject.transform.SetParent(parent.transform, false);


        // ����һ���µ� Mesh
        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.triangles = sequence.ToArray();
        mesh.uv = uvs.ToArray();

        // ���㷨�ߺͱ߽�
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Ϊ�� GameObject ��� MeshFilter �� MeshRenderer ���
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

        // �����ɵ� Mesh ��ֵ�� MeshFilter
        meshFilter.mesh = mesh;

        // ���ò���
        if (material != null)
        {
            meshRenderer.material = material;
        }

        // ��ȡSprite������
        Texture2D spriteTexture = _sprite.texture;

        // ��Sprite������ֵ��Material
        material.SetTexture("_MainTex", spriteTexture);

        // ���Sprite�������UV��Χ��������������һ���֣�����UVƫ�ƺ�����
        Vector4 textureScaleOffset = new Vector4(
            _sprite.rect.width / spriteTexture.width,  // UV��ȱ���
            _sprite.rect.height / spriteTexture.height,  // UV�߶ȱ���
            _sprite.rect.x / spriteTexture.width,  // UV Xƫ��
            _sprite.rect.y / spriteTexture.height   // UV Yƫ��
        );

        material.SetTextureScale("_MainTex", new Vector2(textureScaleOffset.x, textureScaleOffset.y));
        material.SetTextureOffset("_MainTex", new Vector2(textureScaleOffset.z, textureScaleOffset.w));

        // ��ѡ�������ҪΪ�� GameObject ��Ӷ��������������ײ��
        // meshObject.AddComponent<MeshCollider>();
        
    }


#endregion

}
