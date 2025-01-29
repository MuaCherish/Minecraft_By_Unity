using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCEntity;


public class Entity_TNT : MonoBehaviour, IEntityLifecycle
{


    #region ���ں���

    ManagerHub managerhub;
    MC_Velocity_Component Velocity_Component;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
    }


    public int MyEntityID;

    public void OnStartEntity()
    {
        //throw new System.NotImplementedException();
    }

    // �����������ط���
    public void OnStartEntity(int _id, Vector3 _pos, bool _ActByTNT)
    {
        MyEntityID = _id;

        InitTNT();
        transform.position = _pos;

        managerhub.NewmusicManager.PlayOneShot(MusicData.TNT_Fuse);

        if (_ActByTNT)
        {
            FuseDuration = Random.Range(1f, 3f);
        }

        TNTjump();
        StartCoroutine(TNTBlink());
    }

    public void OnEndEntity()
    {
        //��ֵ
        Vector3 _center = managerhub.player.GetCenterPoint(transform.position);

        //��ը����Ч��
        GameObject particle_explore = GameObject.Instantiate(managerhub.player.particle_explosion);
        particle_explore.transform.position = this.transform.position;
        GameObject particleInstance = Instantiate(managerhub.player.Particle_TNT_Prefeb);
        particleInstance.transform.parent = managerhub.player.particel_Broken_transform;
        particleInstance.transform.position = _center;
        particleInstance.GetComponent<ParticleCollision>().Particle_Play(VoxelData.TNT);

        // ���ը��
        Vector3 _Direction = managerhub.player.cam.transform.position - _center;  //ը�ɷ���
        float _value = _Direction.magnitude / 3;  //�������ĵ�̶�[0,1]
        _Direction.y = Mathf.Lerp(0, 1, _value);
        float Distance = Mathf.Lerp(3, 0, _value);
        managerhub.player.ForceMoving(_Direction, Distance, 0.1f);

        //��ҿ�Ѫ
        if (managerhub.world.game_mode == GameMode.Survival && _Direction.magnitude <= 4)
        {
            managerhub.lifeManager.UpdatePlayerBlood((int)Mathf.Lerp(30, 10, _value), true, true);
        }

        //Music
        managerhub.NewmusicManager.PlayOneShot(MusicData.explore);


        //���Χ�ڵ�����TNT
        BlocksFunction.GetAllTNTPositions(this.transform.position, out List<Vector3> TNTpositions);
        if (TNTpositions.Count != 0)
        {
            //print($"��������{TNTpositions.Count}��TNT");

            foreach (Vector3 item in TNTpositions)
            {
                Vector3 _direct = (item - transform.position).normalized;
                float value = _direct.magnitude / BlocksFunction.TNT_explore_Radius;

                //��ը�뾶Ϊ4m
                //���force
                float _force = Mathf.Lerp(500, 10, value);

                managerhub.player.CreateTNT(item, true);
            }

        }


        //������Χ������ʵ��
        List<EntityStruct> _entities = managerhub.world.GetOverlapSphereEntity(_center, BlocksFunction.TNT_explore_Radius);

        foreach (var item in _entities)
        {
            Vector3 _forceDirect = item._obj.transform.position - _center;
            _forceDirect.y = 0.8f;
            _forceDirect = _forceDirect.normalized;

            float _forceValue = 150f;

            item._obj.GetComponent<MC_Velocity_Component>().AddForce(_forceDirect, _forceValue);
        }

        //Chunk
        BlocksFunction.Boom(_center);


        //����ע��
        managerhub.world.RemoveEntity(MyEntityID);

        Destroy(gameObject); // ���ٵ�ǰ����
    }

    //��ʼ��
    void InitTNT()
    {
        ExploreWhite = transform.Find("Crust").gameObject.GetComponent<MeshRenderer>().material;

        // ��ȡCrust��Renderer���
        GameObject crust = transform.Find("Crust").gameObject;
        if (crust != null)
        {
            crustRenderer = crust.GetComponent<Renderer>();
            if (crustRenderer != null)
            {
                originalMaterial = crustRenderer.material; // ����ԭʼ����
            }
        }
        else
        {
            Debug.LogError("δ�ҵ�Crust����");
        }
    }


    #endregion


    #region TNTjump

    [Header("��Ծ��")] public float forcevalue = 45f; // ʩ�ӵ���
    [Header("��Ծ�߶�")] public float jumpheight = 0.9f; // ��Ծ�߶�
    void TNTjump(Vector3? _customDirection = null)
    {
        Vector3 direction;

        if (_customDirection.HasValue) // ����д����Զ��巽��
        {
            direction = _customDirection.Value; // ʹ���Զ��巽��
            Velocity_Component.AddForce(direction, 1); // ֱ��ʩ����������Ҫ��׼��
        }
        else
        {
            //direction = Random.onUnitSphere * 0.5f; // ʹ���������
            //direction.y = jumpheight; // ����Y��Ϊjumpheight
            //Vector3 direct = direction.normalized; // ��׼������
            //Velocity_Component.AddForce(direct, forcevalue); // ʩ����
            Velocity_Component.EntityRandomJump(forcevalue);
        }
    }




    #endregion


    #region TNTBlink


    [Header("��˸����")] public int frequency = 9; // ��˸����
    [Header("��ɫ����ʱ��")] public float whiteDelay = 0.5f; // ��˸�ӳ�ʱ��
    [Header("��ͨ����ʱ��")] public float normalDelay = 0.5f; // ��˸�ӳ�ʱ��
    [Header("�������")] public float ColorA = 150f; // ��˸�ӳ�ʱ��
    [Header("����ʱ��")] public float FuseDuration = 4f;
    private Material ExploreWhite; // Ԥ�����˸����
    private Material originalMaterial; // ԭʼ����
    private Renderer crustRenderer; // crust��Renderer���


    IEnumerator TNTBlink()
    {
        if (crustRenderer == null || ExploreWhite == null)
        {
            print("crustRenderer == null || ExploreWhite == null");
            yield break; // ���û��Renderer����ʣ��˳�Э��
        }

        // ����ExploreWhite�Ĳ���ʵ��
        Material blinkMaterial = new Material(ExploreWhite);
        float elapsetime = 0f;

        for (int i = 0; i < frequency; i++)
        {
            // ��crust����ʵ������
            crustRenderer.material = blinkMaterial;

            // ͸���ȵ���Ϊ1
            Color color = blinkMaterial.color;
            color.a = ColorA / 255f; // ����͸����Ϊ1
            blinkMaterial.color = color;

            //��ʼ����
            if (i == frequency - 1 || elapsetime > FuseDuration)
            {
                StartCoroutine(TNTSwell());
                yield break;
            }


            yield return new WaitForSeconds(whiteDelay);
            elapsetime += whiteDelay;
            // ͸���ȵ���Ϊ0
            color.a = 0f; // ����͸����Ϊ0
            blinkMaterial.color = color;

            yield return new WaitForSeconds(normalDelay);
            elapsetime += normalDelay;
        }

        // �ָ�ԭʼ����
        if (crustRenderer != null && originalMaterial != null)
        {
            crustRenderer.material = originalMaterial;
        }

        //OnEndEntity();
        

    }

    #endregion


    #region TNTswell

    [Header("����ʱ��")] public float SwellDuration = 0.1f;
    [Header("���ʹ�С")] public float SwellMaxScale = 1.2f;


    IEnumerator TNTSwell()
    {
        // ��ȡ��ǰ�����ԭʼ����
        Vector3 originalScale = transform.localScale;
        // �������ͺ������ֵ
        Vector3 targetScale = originalScale * SwellMaxScale;

        float elapsedTime = 0f; // ��¼������ʱ��

        while (elapsedTime < SwellDuration)
        {
            // ���㵱ǰ�ı���
            float t = elapsedTime / SwellDuration;

            // ʹ�� Lerp ��ƽ����������
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            elapsedTime += Time.deltaTime; // ���Ӿ�����ʱ��
            yield return null; // �ȴ���һ֡
        }

        // ȷ����������ֵΪĿ������ֵ
        transform.localScale = targetScale;

        OnEndEntity();
        
    }

    #endregion


    #region Debug

    //[Header("Debug")] public bool ��ȼһ��;
    //private void Update()
    //{
    //    if (��ȼһ��)
    //    {
    //        OnStartEntity(transform.position, false);
    //        ��ȼһ�� = false;
    //    }
    //}

    #endregion


}
