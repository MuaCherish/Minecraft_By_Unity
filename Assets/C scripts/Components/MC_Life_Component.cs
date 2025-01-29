using Homebrew;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
    [RequireComponent(typeof(MC_AI_Component))]
    [RequireComponent(typeof(MC_Registration_Component))]
    public class MC_Life_Component : MonoBehaviour
    {

        #region ״̬

        [Foldout("״̬", true)]

        [ReadOnly] public bool isEntity_Hurt = false;
        [ReadOnly] public bool isEntity_Dead = false;
        [ReadOnly] public bool isEntity_Dive = false;

        #endregion


        #region ���ں���

        MC_Velocity_Component Velocity_Component;
        MC_Collider_Component Collider_Component;
        MC_AI_Component AI_Component;
        World world;
        MC_Registration_Component Registration_Component;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
            AI_Component = GetComponent<MC_AI_Component>();
            world = Collider_Component.managerhub.world;
            Registration_Component = GetComponent<MC_Registration_Component>();
        }


        private void Start()
        {
            if (EntityMat != null)
            {
                originalColor = EntityMat.color; // ��ȡ���ʵ�ԭʼ��ɫ
            }
        }


        private void Update()
        {
            if (world.game_state == Game_State.Playing)
            {
                _ReferUpdate_CheckOxy();
                _ReferUpdate_FallingCheck();
            }
        }

        #endregion


        #region ����ֵ����

        [Foldout("Transform", true)]
        [Header("��������")] public Material EntityMat;

        [Foldout("����ֵ����", true)]
        [Header("ʵ������ֵ")] public int EntityBlood = 20;
        [Header("���˳���ʱ��")] public float Hurt_ElapseTime = 0.3f;
        [Header("��������")] public float Hurt_Force = 35f;
        [Header("��������")] public GameObject Evaporation_Particle;
        private Color originalColor; // ������ʵ�ԭʼ��ɫ

        /// <summary>
        /// ����ʵ������ֵ����Ѫ����-1,Vector3.zero�Ļ��򲻻�ǿ���ƶ�
        /// </summary>
        public void UpdateEntityLife(int _updateLifeValue, Vector3 _hutyDirect)
        {
            //��ǰ����-���˻�������������
            if (isEntity_Hurt || isEntity_Dead)
                return;

            //��ǰ����-0������
            if (_updateLifeValue == 0)
                return;

            //�������˳���
            if (_updateLifeValue < 0)
                Handle_Hurt(_hutyDirect);


            //���û�й����ԣ���Ὺʼ����
            if (AI_Component.isAggressive == false)
            {
                //AI_Component.myState = AIState.Flee;
            }


            //�޸�Ѫ���������������
            if (CheckDead_EditBlood(_updateLifeValue))
                Handle_Dead();

            //print($"�ܻ�����: {_hutyDirect}");
        }


        void Handle_Hurt(Vector3 _hurtDirect)
        {
            isEntity_Hurt = true;

            //���ʱ��
            if (EntityMat != null)
                StartCoroutine(ChangeColorCoroutine());
            else
                Debug.LogWarning("EntityMat is not assigned!");

            //ǿ��λ��
            if (_hurtDirect != Vector3.zero)
                Velocity_Component.AddForce(_hurtDirect, Hurt_Force);

        }

        void Handle_Dead()
        {
            isEntity_Dead = true;


            Registration_Component.LogOffEntity();
        }


        //���ʱ��
        IEnumerator ChangeColorCoroutine()
        {
            // ��������ɫ��Ϊ��ɫ
            EntityMat.color = Color.red;

            // �ȴ�1��
            yield return new WaitForSeconds(Hurt_ElapseTime);

            // ��������ɫ��ԭ
            EntityMat.color = originalColor;

            isEntity_Hurt = false;
        }


        bool CheckDead_EditBlood(int _updateLifeValue)
        {
            //��ǰ����-�������
            if (EntityBlood + _updateLifeValue <= 0)
                return true;


            EntityBlood += _updateLifeValue;


            return false;
        }


        /// <summary>
        /// �趨ʵ��Ѫ��
        /// </summary>
        public void SetEntityBlood(int _SetValue)
        {

            if (_SetValue == 0)
            {
                Handle_Dead();
            } else if (_SetValue > 0 && _SetValue <= 20)
            {
                EntityBlood = _SetValue;
            }
            else if (_SetValue > 20)
            {
                EntityBlood = 20;
            }
            else
            {
                print("life������ִ���");
            }

        }


        #endregion


        #region ��������

        [Header("ʵ������ֵ")] public int EntityOxygen = 10;
        Coroutine Coroutine_CheckOxy;


        void _ReferUpdate_CheckOxy()
        {
            if (world.GetBlockType(Collider_Component.EyesPoint) == VoxelData.Water && Coroutine_CheckOxy == null)
            {
                isEntity_Dive = true;
                Coroutine_CheckOxy = StartCoroutine(_CheckOxy());
            }
        }

        IEnumerator _CheckOxy()
        {
            

            while (true)
            {
                //��ǰ����-����ˮ��
                if (world.GetBlockType(Collider_Component.EyesPoint) != VoxelData.Water)
                {
                    EntityOxygen = 10;
                    Coroutine_CheckOxy = null;
                    isEntity_Dive = false;
                    yield break;
                }

                if (EntityOxygen < 0)
                {
                    if (EntityBlood > 0)
                    {
                        UpdateEntityLife(-1, Vector3.zero);
                    }
                    else
                    {
                        Coroutine_CheckOxy = null;
                        yield break;
                    }
                    
                }
                else
                {
                    EntityOxygen--;
                }

                yield return new WaitForSeconds(1f);


            }

            
        }

        #endregion


        #region ˤ����

        [Foldout("ˤ�����", true)]
        [Header("���ˤ��߶�")] public float maxFallDis = 4f;
        private float realMaxY = -Mathf.Infinity;  //��ǰ���������߶�

        void _ReferUpdate_FallingCheck()
        {

            //��ؼ��
            if (Collider_Component.isGround)
            {
                float _Drop = realMaxY - Collider_Component.FootPoint.y;
                if (_Drop > maxFallDis)
                {
                    print($"�۳�Ѫ��:{_Drop - maxFallDis}");
                    UpdateEntityLife(-(int)(_Drop - maxFallDis), Vector3.zero);
                    realMaxY = Collider_Component.FootPoint.y;
                }
            }
            else
            {
                //ʵʱ����
                if (Collider_Component.FootPoint.y > realMaxY)
                {
                    realMaxY = Collider_Component.FootPoint.y;
                }
            }

            

        }

        #endregion

    }

}

