using Homebrew;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
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
        Animation animationCoponent;
        World world;
        GameObject ParticleParent;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
            animationCoponent = GetComponent<Animation>();
            world = Collider_Component.managerhub.world;
            ParticleParent = GameObject.Find("Environment/Particles");
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
            }
        }

        #endregion


        #region ����ֵ����

        [Foldout("����ֵ����", true)]
        [Header("ʵ������ֵ")] public int EntityBlood = 20;
        [Header("��������")] public Material EntityMat;
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

            //��������
            if (animationCoponent != null && animationCoponent.GetClip("EntityDead") != null)
            {
                animationCoponent.Play("EntityDead");
            }
            else
            {
                print("�Ҳ���");
            }


            //��������
            // ����ʵ������������������Ϊ particleParent
            GameObject deadParticle = GameObject.Instantiate(
                Evaporation_Particle,
                transform.position,
                Quaternion.LookRotation(Vector3.up),
                ParticleParent.transform  // ���ø�����
            );


            //����������
            Vector3 randomPoint = Random.insideUnitSphere / 2f;
            Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position, new BlockItem(VoxelData.Slimeball, 1), false);
            Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position + randomPoint, new BlockItem(VoxelData.Apple, 2), false);

            Destroy(this.gameObject, 0.5f);

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

    }

}

