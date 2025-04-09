using Homebrew;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Component_Velocity))]
    [RequireComponent(typeof(MC_Component_Physics))]
    [RequireComponent(typeof(MC_Component_Registration))]
    public class MC_Component_Life : MonoBehaviour
    {

        #region ״̬

        [Foldout("״̬", true)]

        [ReadOnly] public bool isEntity_Hurt = false;
        [ReadOnly] public bool isEntity_Dead = false;
        [ReadOnly] public bool isEntity_Dive = false;

        #endregion


        #region ���ں���

        MC_Component_Velocity Component_Velocity;
        MC_Component_Physics Component_Physics;
        MC_Component_AI Component_AI;
        MC_Component_Music Component_Music;
        MC_Service_World Service_World;
        MC_Component_Registration Component_Registration;
        ManagerHub managerhub;
        private void Awake()
        {
            Component_Velocity = GetComponent<MC_Component_Velocity>();
            Component_Physics = GetComponent<MC_Component_Physics>();
            Component_AI = GetComponent<MC_Component_AI>();
            Component_Music = GetComponent<MC_Component_Music>();
            Service_World = Component_Physics.managerhub.Service_World;
            Component_Registration = GetComponent<MC_Component_Registration>();
            managerhub = Component_Physics.managerhub;

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
            if (MC_Runtime_DynamicData.instance.GetGameState() == Game_State.Playing)
            {
                _ReferUpdate_CheckOxy();
                _ReferUpdate_FallingCheck();
                _ReferUpdate_DynamicEntityMatColor();
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
        [Header("��˸��ɫ(������)")] public Color Color_Blink;
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


        void _ReferUpdate_DynamicEntityMatColor()
        {
            //��ǰ����-�����Lock
            if (_DynamicEntityColorLock)
                return;

            // ��ǰ���� - ���hurt���˳�
            if (isEntity_Hurt)
                return;

            //��ǰ����-����255
            if (managerhub.Service_World.GetBlockType(Component_Physics.EyesPoint) == 255)
                return;

            // Ĭ����ɫ
            Color targetColor = save_Color;  

            // �������ѹ
            if (Service_World.blocktypes[managerhub.Service_World.GetBlockType(Component_Physics.EyesPoint)].isSolid)
                targetColor = Color_UnderBlock;
            // �����ˮ��
            else if (Component_Physics.IsInTheWater(Component_Physics.HeadPoint))
                targetColor = Color_UnderWater;

            // �����ǰĿ����ɫ��֮ǰ��ͬ������²�����ɫ
            if (EntityMat.GetColor("_Color") != targetColor)
            {
                EntityMat.SetColor("_Color", targetColor);
            }

        }

        /// <summary>
        /// ��ȡʵ�����ʵ��
        /// </summary>
        /// <returns></returns>
        public Material GetEntityMat()
        {
            return EntityMat;
        }

        /// <summary>
        /// ʵ����ɫ����
        /// </summary>
        /// <param name="_lock"></param>
        private bool _DynamicEntityColorLock;
        public void DynamicEntityColorLock(bool _lock)
        {
            _DynamicEntityColorLock = _lock;
        }

        /// <summary>
        /// �ı����ʵ����ɫ
        /// </summary>
        public void UpdateEntityColor(Color _TargetColor)
        {
            EntityMat.SetColor("_Color", _TargetColor);
        }

        /// <summary>
        /// �ָ�����ʵ����ɫ
        /// </summary>
        public void ResetEntityColor()
        {
            EntityMat.SetColor("_Color", save_Color);
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
            if(Component_AI != null)
            {
                if (!Component_AI.isAggressive && Component_AI.EntityCanFlee)
                    Component_AI.EntityFlee();
            }
           

            //�޸�Ѫ���������������
            if (CheckDead_EditBlood(_updateLifeValue))
                Handle_Dead();

            //print($"�ܻ�����: {_hutyDirect}");
        }


        void Handle_Hurt(Vector3 _hurtDirect)
        {
            isEntity_Hurt = true;

            //������Ч
            if(Component_Music != null)
                Component_Music.PlaySound(Component_Music.BehurtClip);
            //managerhub.Service_Music.Create3DSound(transform.position, Component_Music.BehurtClip);

            //���ʱ��
            if (EntityMat != null)
                StartCoroutine(ChangeColorCoroutine());
            else
                Debug.LogWarning("EntityMat is not assigned!");

            //ǿ��λ��
            if (_hurtDirect != Vector3.zero)
                Component_Velocity.AddForce(_hurtDirect, Component_Velocity.force_hurt);

        }

        void Handle_Dead()
        {
            isEntity_Dead = true;
            Component_Registration.LogOffEntity();
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
            if (managerhub.Service_World.GetBlockType(Component_Physics.EyesPoint) == VoxelData.Water && Coroutine_CheckOxy == null)
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
                if (managerhub.Service_World.GetBlockType(Component_Physics.EyesPoint) != VoxelData.Water)
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

            if (!Component_Physics.isGround || Component_Physics.IsInTheWater(Component_Physics.EyesPoint))
            {
                //ʵʱ����
                if (Component_Physics.FootPoint.y > realMaxY)
                    realMaxY = Component_Physics.FootPoint.y;
            }
            else
            {
                //��ؼ��
                float _Drop = realMaxY - Component_Physics.FootPoint.y;
                if (_Drop > maxFallDis)
                {
                    //print($"�۳�Ѫ��:{_Drop - maxFallDis}");
                    UpdateEntityLife(-(int)(_Drop - maxFallDis), Vector3.zero);
                    realMaxY = Component_Physics.FootPoint.y;

                    //���������Ч
                    managerhub.Service_Music.Create3DSound(transform.position, Default_DropGround);
                }
            }


        } 

        #endregion

    }

}

