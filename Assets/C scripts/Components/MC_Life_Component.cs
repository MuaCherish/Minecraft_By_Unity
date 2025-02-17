using Homebrew;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
    [RequireComponent(typeof(MC_AI_Component))]
    [RequireComponent(typeof(MC_Registration_Component))]
    [RequireComponent(typeof(MC_Music_Component))]
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
        MC_Music_Component Music_Component;
        World world;
        MC_Registration_Component Registration_Component;
        ManagerHub managerhub;
        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
            AI_Component = GetComponent<MC_AI_Component>();
            Music_Component = GetComponent<MC_Music_Component>(); 
            world = Collider_Component.managerhub.world;
            Registration_Component = GetComponent<MC_Registration_Component>();
            managerhub = Collider_Component.managerhub;

            _ReferAwake_GetAllRenderersAndCreateMaterialInstance();
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
                _ReferUpdate_IntheWaterBeBlack();
            }
        }





        #endregion


        #region ����ʵ��

        //��������ʵ��
        [Foldout("����ʵ��", true)]
        [Header("(ע����Ⱦ����ѡ���һ���������Ⱦ���Ĳ�����Ϊ����ʵ��\n�����͸��Ƥ����ʵ����Ҫע��)")]
        [Header("������Ⱦ��")] public Renderer[] hurtRenderers;
        [Header("������ɫ")] public Color Color_BeHurt = new Color(220 / 255f, 81 / 255f, 136 / 255f, 1f);
        [Header("ˮ����ɫ")] public Color Color_UnderWater = new Color(0x5B / 255f, 0x5B / 255f, 0x5B / 255f, 1f); private Color save_Color;
        [Header("����ѹ��ɫ")] public Color Color_UnderBlock = new Color(0x00 / 255f, 0x00 / 255f, 0x00 / 255f, 1f);
        private Material EntityMat;

        //��ȡ�������������Renderer
        //��Ϊ���Ǵ�������ʵ��
        void _ReferAwake_GetAllRenderersAndCreateMaterialInstance()
        {
            //�ҵ�������Ⱦ��
            if(hurtRenderers.Length == 0)
            {
                // ��һ�� List ���洢���е� Renderer�����㴦��
                List<Renderer> renderersList = new List<Renderer>();

                // �ݹ������ǰ����������Ӷ���
                Stack<Transform> stack = new Stack<Transform>();
                stack.Push(transform);

                while (stack.Count > 0)
                {
                    Transform current = stack.Pop();

                    // ��ȡ��ǰ����� Renderer ���
                    Renderer[] renderers = current.GetComponents<Renderer>();
                    renderersList.AddRange(renderers);

                    // �������Ӷ���ѹ��ջ�У��ݹ����
                    foreach (Transform child in current)
                    {
                        stack.Push(child);
                    }
                }

                // ����ȡ�� Renderer ת��Ϊ���鲢�洢�� Renderers
                hurtRenderers = renderersList.ToArray();
            }

            

            //��������ʵ��
            EntityMat = new Material(hurtRenderers[0].sharedMaterial);

            save_Color = EntityMat.color;

            foreach (var item in hurtRenderers)
            {
                item.material = EntityMat;
            }
        }


        void _ReferUpdate_IntheWaterBeBlack()
        {
            // ��ǰ���� - ���hurt���˳�
            if (isEntity_Hurt)
                return;

            //��ǰ����-����255
            if (world.GetBlockType(Collider_Component.EyesPoint) == 255)
                return;

            Color targetColor = save_Color;  // Ĭ����ɫ

            // �������ѹ
            if (world.blocktypes[world.GetBlockType(Collider_Component.EyesPoint)].isSolid)
            {
                targetColor = Color_UnderBlock;
            }
            // �����ˮ��
            else if (Collider_Component.IsInTheWater(Collider_Component.HeadPoint))
            {
                targetColor = Color_UnderWater;
            }

            // �����ǰĿ����ɫ��֮ǰ��ͬ������²�����ɫ
            if (EntityMat.GetColor("_Color") != targetColor)
            {
                EntityMat.SetColor("_Color", targetColor);
            }
        }




        #endregion


        #region ����ֵ����

        [Foldout("����ֵ����", true)]
        [Header("ʵ������ֵ")] public int EntityBlood = 20;
        [Header("���˳���ʱ��")] public float Hurt_ElapseTime = 0.3f;
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

            //����
            if (!AI_Component.isAggressive && AI_Component.EntityCanFlee)
                AI_Component.EntityFlee();

            //�޸�Ѫ���������������
            if (CheckDead_EditBlood(_updateLifeValue))
                Handle_Dead();

            //print($"�ܻ�����: {_hutyDirect}");
        }


        void Handle_Hurt(Vector3 _hurtDirect)
        {
            isEntity_Hurt = true;

            //������Ч
            //Music_Component.PlaySound(Music_Component.BehurtClip);
            managerhub.NewmusicManager.Create3DSound(transform.position, Music_Component.BehurtClip);

            //���ʱ��
            if (EntityMat != null)
                StartCoroutine(ChangeColorCoroutine());
            else
                Debug.LogWarning("EntityMat is not assigned!");

            //ǿ��λ��
            if (_hurtDirect != Vector3.zero)
                Velocity_Component.AddForce(_hurtDirect, Velocity_Component.force_hurt);

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
            EntityMat.color = Color_BeHurt;

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
        [Header("ˤ����ЧID")] public int Default_DropGround = 37;
        private float realMaxY = -Mathf.Infinity;  //��ǰ���������߶�

        void _ReferUpdate_FallingCheck()
        {

            if (!Collider_Component.isGround || Collider_Component.IsInTheWater(Collider_Component.EyesPoint))
            {
                //ʵʱ����
                if (Collider_Component.FootPoint.y > realMaxY)
                    realMaxY = Collider_Component.FootPoint.y;
            }
            else
            {
                //��ؼ��
                float _Drop = realMaxY - Collider_Component.FootPoint.y;
                if (_Drop > maxFallDis)
                {
                    //print($"�۳�Ѫ��:{_Drop - maxFallDis}");
                    UpdateEntityLife(-(int)(_Drop - maxFallDis), Vector3.zero);
                    realMaxY = Collider_Component.FootPoint.y;

                    //���������Ч
                    managerhub.NewmusicManager.Create3DSound(transform.position, Default_DropGround);
                }
            }


        } 

        #endregion

    }

}

