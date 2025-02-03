using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCEntity;
using Homebrew;

[RequireComponent(typeof(MC_Collider_Component))]
public class Entity_TNT : MonoBehaviour, IEntityBrain
{


    #region ���ں���

    ManagerHub managerhub;
    MC_Velocity_Component Velocity_Component;
    MC_Collider_Component Collider_Component;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
        Collider_Component = GetComponent<MC_Collider_Component>();
    }

    public void OnStartEntity()
    {
        //throw new System.NotImplementedException();
    }

    // �����������ط���
    public void OnStartEntity(Vector3 _pos, bool _ActByTNT)
    {

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

    
    //��ʼ��
    void InitTNT()
    {
        ExploreWhite = crustRenderer.material;

        // ��ȡCrust��Renderer���
        originalMaterial = crustRenderer.material; // ����ԭʼ����
    }


    #endregion


    #region 1.��ȼ��Ծ

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


    #region 2.TNT��˸


    [Header("��˸����")] public int frequency = 9; // ��˸����
    [Header("��ɫ����ʱ��")] public float whiteDelay = 0.5f; // ��˸�ӳ�ʱ��
    [Header("��ͨ����ʱ��")] public float normalDelay = 0.5f; // ��˸�ӳ�ʱ��
    [Header("�������")] public float ColorA = 150f; // ��˸�ӳ�ʱ��
    [Header("����ʱ��")] public float FuseDuration = 4f;
    private Material ExploreWhite; // Ԥ�����˸����
    private Material originalMaterial; // ԭʼ����
    public Renderer crustRenderer; // crust��Renderer���

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


    #region 3.TNT����

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


    #region 3.TNT��ը

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
        BlocksFunction.GetAllTNTPositions(transform.position, out List<Vector3> TNTpositions);
        if (TNTpositions.Count != 0)
        {
            //print($"��������{TNTpositions.Count}��TNT");

            foreach (Vector3 item in TNTpositions)
            {
                Vector3 _direct = (item - transform.position).normalized;
                float value = _direct.magnitude / BlocksFunction.TNT_explore_Radius;
                managerhub.player.CreateTNT(item, true);
            }

        }


        //������Χ������ʵ��
        if (managerhub.world.GetOverlapSphereEntity(_center, BlocksFunction.TNT_explore_Radius + 2f, GetComponent<MC_Registration_Component>().EntityID, out List<EntityStruct> _entities))
        {
            foreach (var item in _entities)
            {
                Vector3 _forceDirect = (item._obj.transform.position - _center).normalized;
                _forceDirect.y = 0.8f;
                _forceDirect = _forceDirect.normalized;

                //ʩ������
                float _dis = (item._obj.transform.position - _center).magnitude;
                float _forceValue = 0f;

                // ���������0��4��֮�䣬��ֵ��400��160֮��仯
                if (_dis >= 0f && _dis <= BlocksFunction.TNT_explore_Radius)
                {
                    _forceValue = Mathf.Lerp(400f, 160f, _dis / BlocksFunction.TNT_explore_Radius);
                }
                // ���������4��6��֮�䣬��ֵ�̶�Ϊ50
                else if (_dis > BlocksFunction.TNT_explore_Radius && _dis <= BlocksFunction.TNT_explore_Radius + 2f)
                {
                    _forceValue = 50f;
                }
                // ������볬��6�ף���ֵΪ0������
                else
                {
                    _forceValue = 0f;  // ��������Ϊ����Ҫ��Ĭ��ֵ
                }

                item._obj.GetComponent<MC_Velocity_Component>().AddForce(_forceDirect, _forceValue);
            }
        }


        //Chunk
        if (!Collider_Component.IsInTheWater(Collider_Component.FootPoint + new Vector3(0f, 0.125f, 0f)))
        {
            BlocksFunction.Boom(_center);
        }
        

        GetComponent<MC_Registration_Component>().LogOffEntity();
        //Destroy(gameObject); // ���ٵ�ǰ����
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
