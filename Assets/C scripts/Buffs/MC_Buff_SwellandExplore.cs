using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MC_Static_BlocksFunction;

/// <summary>
/// Warning!!!
/// ��buffһ�����������ٸ�ʵ��
/// </summary>
public class MC_Buff_SwellandExplore : MC_Buff_Base
{

    #region ���ں���

    MC_Component_Physics Component_Physics;
    ManagerHub managerhub;

    private void Awake()
    {
        Component_Physics = GetComponent<MC_Component_Physics>();
        managerhub = Component_Physics.managerhub;
    }

    private void Update()
    {
        
    }

    #endregion

    /// <summary>
    /// ���͵�һ����С�����ը����ɷ����ʵ����˺�
    /// </summary>
    public override IEnumerator StartBuffEffect()
    {
        // �趨����ʱ���Ŀ�����
        float SwellTimer = 0;
        float _swellDuration = 0.3f;
        GameObject _Model = Component_Physics.Model;
        Vector3 originalScale = _Model.transform.localScale;  // ��ʼ����
        Vector3 targetScale = originalScale * 1.1f;  // Ŀ�����ţ����͵�1.1����

        // ���͹���
        while (SwellTimer < _swellDuration)
        {
            float lerpValue = SwellTimer / _swellDuration;  // ���㵱ǰ���ͽ���
            _Model.transform.localScale = Vector3.Lerp(originalScale, targetScale, lerpValue);  // ƽ����������

            SwellTimer += Time.deltaTime;
            yield return null;
        }

        // ��ը����
        Handle_Explore();
    }


    //��ը����
    void Handle_Explore()
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
        float BloodValue = _Direction.magnitude / 6f;
        float _value = _Direction.magnitude / 3;  //�������ĵ�̶�[0,1]
        _Direction.y = Mathf.Lerp(0, 1, _value);
        float Distance = Mathf.Lerp(3, 0, _value);
        managerhub.player.ForceMoving(_Direction, Distance, 0.1f);

        //��ҿ�Ѫ
        if (managerhub.Service_World.game_mode == GameMode.Survival && _Direction.magnitude <= 6)
            managerhub.lifeManager.UpdatePlayerBlood((int)Mathf.Lerp(52, 5, BloodValue), true, true);

        //Music
        managerhub.NewmusicManager.PlayOneShot(MusicData.explore);


        //���Χ�ڵ�����TNT
        GetAllTNTPositions(transform.position, out List<Vector3> TNTpositions);
        if (TNTpositions.Count != 0)
        {
            //print($"��������{TNTpositions.Count}��TNT");

            foreach (Vector3 item in TNTpositions)
            {
                Vector3 _direct = (item - transform.position).normalized;
                float value = _direct.magnitude / TNT_explore_Radius;
                managerhub.player.CreateTNT(item, true);
            }

        }


        //������Χ������ʵ��
        if (managerhub.Service_Entity.GetOverlapSphereEntity(_center, TNT_explore_Radius + 2f, GetComponent<MC_Component_Registration>().GetEntityId()._id, out List<EntityInfo> _entities))
        {
            foreach (var item in _entities)
            {
                Vector3 _forceDirect = (item._obj.transform.position - _center).normalized;
                _forceDirect.y = 0.8f;
                _forceDirect = _forceDirect.normalized;
                int updateBlood = 0;

                //ʩ������
                float _dis = (item._obj.transform.position - _center).magnitude;
                float _forceValue = 0f;

                // ���������0��4��֮�䣬��ֵ��400��160֮��仯
                if (_dis >= 0f && _dis <= TNT_explore_Radius)
                {
                    _forceValue = Mathf.Lerp(400f, 160f, _dis / TNT_explore_Radius);
                    updateBlood = (int)Mathf.Lerp(32, 10, _dis / TNT_explore_Radius);
                }
                // ���������4��6��֮�䣬��ֵ�̶�Ϊ50
                else if (_dis > TNT_explore_Radius && _dis <= TNT_explore_Radius + 2f)
                {
                    _forceValue = 50f;
                    updateBlood = (int)Mathf.Lerp(10, 0, (_dis - TNT_explore_Radius) / 2f);
                }
                // ������볬��6�ף���ֵΪ0������
                else
                {
                    updateBlood = 0;
                    _forceValue = 0f;  // ��������Ϊ����Ҫ��Ĭ��ֵ
                }

                if (item._obj.GetComponent<MC_Component_Life>() != null)
                {
                    item._obj.GetComponent<MC_Component_Life>().UpdateEntityLife(-updateBlood, _forceDirect * _forceValue);
                }
                else
                {
                    item._obj.GetComponent<MC_Component_Velocity>().AddForce(_forceDirect, _forceValue);
                }

            }
        }


        //Chunk
        if (!Component_Physics.IsInTheWater(Component_Physics.FootPoint + new Vector3(0f, 0.125f, 0f)))
            Boom(_center);


        GetComponent<MC_Component_Registration>().LogOffEntity(true);
    }



    /// <summary>
    /// ����Ҫ���buff�ˣ�ֱ�������Լ����ж���
    /// </summary>
    public override void EndBuffEffect()
    {
        //Pass
    }


    /// <summary>
    /// ������ʱ�亯��ʧЧ
    /// </summary>
    public override void ResetBuffDuration()
    {
        //pass
    }

}
