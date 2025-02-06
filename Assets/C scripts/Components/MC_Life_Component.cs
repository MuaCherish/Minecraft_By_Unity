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
    public class MC_Life_Component : MonoBehaviour
    {

        #region 状态

        [Foldout("状态", true)]

        [ReadOnly] public bool isEntity_Hurt = false;
        [ReadOnly] public bool isEntity_Dead = false;
        [ReadOnly] public bool isEntity_Dive = false;

        #endregion


        #region 周期函数

        MC_Velocity_Component Velocity_Component;
        MC_Collider_Component Collider_Component;
        MC_AI_Component AI_Component;
        World world;
        MC_Registration_Component Registration_Component;
        ManagerHub managerhub;
        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
            AI_Component = GetComponent<MC_AI_Component>();
            world = Collider_Component.managerhub.world;
            Registration_Component = GetComponent<MC_Registration_Component>();
            _ReferAwake_CreateMaterialInstance();
            managerhub = Collider_Component.managerhub;
        }


        private void Start()
        {
            if (EntityMat != null)
            {
                originalColor = EntityMat.color; // 获取材质的原始颜色
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


        #region 材质实例

        //创建材质实例
        [Foldout("材质实例", true)]
        [Header("渲染器")] public Renderer[] Renderers;
        [Header("水中颜色")] public Color Color_UnderWater = new Color(0x5B / 255f, 0x5B / 255f, 0x5B / 255f, 1f); private Color save_Color;
        [Header("被挤压颜色")] public Color Color_UnderBlock = new Color(0x00 / 255f, 0x00 / 255f, 0x00 / 255f, 1f);
        private Material EntityMat;

        void _ReferAwake_CreateMaterialInstance()
        {
            EntityMat = new Material(Renderers[0].sharedMaterial);
            
            save_Color = EntityMat.color;

            foreach (var item in Renderers)
            {
                item.material = EntityMat;
            }

        }

        void _ReferUpdate_IntheWaterBeBlack()
        {
            // 提前返回 - 如果hurt则退出
            if (isEntity_Hurt)
                return;

            Color targetColor = save_Color;  // 默认颜色

            // 如果被挤压
            if (world.blocktypes[world.GetBlockType(Collider_Component.EyesPoint)].isSolid)
            {
                targetColor = Color_UnderBlock;
            }
            // 如果在水里
            else if (Collider_Component.IsInTheWater(Collider_Component.HeadPoint))
            {
                targetColor = Color_UnderWater;
            }

            // 如果当前目标颜色与之前不同，则更新材质颜色
            if (EntityMat.GetColor("_Color") != targetColor)
            {
                EntityMat.SetColor("_Color", targetColor);
            }
        }




        #endregion


        #region 生命值部分

        [Foldout("生命值设置", true)]
        [Header("实体生命值")] public int EntityBlood = 20;
        [Header("受伤持续时间")] public float Hurt_ElapseTime = 0.3f;
        [Header("受伤力度")] public float Hurt_Force = 35f;
        [Header("蒸汽粒子")] public GameObject Evaporation_Particle;
        private Color originalColor; // 保存材质的原始颜色

        /// <summary>
        /// 更新实体生命值，扣血就是-1,Vector3.zero的话则不会强制移动
        /// </summary>
        public void UpdateEntityLife(int _updateLifeValue, Vector3 _hutyDirect)
        {
            //提前返回-受伤或者死亡不触发
            if (isEntity_Hurt || isEntity_Dead)
                return;

            //提前返回-0不触发
            if (_updateLifeValue == 0)
                return;

            //触发受伤程序
            if (_updateLifeValue < 0)
                Handle_Hurt(_hutyDirect);


            //如果没有攻击性，则会开始逃跑
            if (AI_Component.isAggressive == false)
                AI_Component.EntityFlee();


            //修改血量并检查死亡程序
            if (CheckDead_EditBlood(_updateLifeValue))
                Handle_Dead();

            //print($"受击向量: {_hutyDirect}");
        }


        void Handle_Hurt(Vector3 _hurtDirect)
        {
            isEntity_Hurt = true;

            //材质变红
            if (EntityMat != null)
                StartCoroutine(ChangeColorCoroutine());
            else
                Debug.LogWarning("EntityMat is not assigned!");

            //强制位移
            if (_hurtDirect != Vector3.zero)
                Velocity_Component.AddForce(_hurtDirect, Hurt_Force);

        }

        void Handle_Dead()
        {
            isEntity_Dead = true;
            Registration_Component.LogOffEntity();
        }


        //材质变红
        IEnumerator ChangeColorCoroutine()
        {
            // 将材质颜色变为红色
            EntityMat.color = Color.red;

            // 等待1秒
            yield return new WaitForSeconds(Hurt_ElapseTime);

            // 将材质颜色还原
            EntityMat.color = originalColor;

            isEntity_Hurt = false;
        }


        bool CheckDead_EditBlood(int _updateLifeValue)
        {
            //提前返回-如果死亡
            if (EntityBlood + _updateLifeValue <= 0)
                return true;


            EntityBlood += _updateLifeValue;


            return false;
        }


        /// <summary>
        /// 设定实体血量
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
                print("life组件出现错误");
            }

        }


        #endregion


        #region 氧气部分

        [Header("实体氧气值")] public int EntityOxygen = 10;
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
                //提前返回-浮出水面
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


        #region 摔落检测

        [Foldout("摔落参数", true)]
        [Header("最大摔落高度")] public float maxFallDis = 4f;
        [Header("摔落音效ID")] public int Default_DropGround = 37;
        private float realMaxY = -Mathf.Infinity;  //当前触及的最大高度

        void _ReferUpdate_FallingCheck()
        {

            if (!Collider_Component.isGround || Collider_Component.IsInTheWater(Collider_Component.EyesPoint))
            {
                //实时更新
                if (Collider_Component.FootPoint.y > realMaxY)
                    realMaxY = Collider_Component.FootPoint.y;
            }
            else
            {
                //落地检测
                float _Drop = realMaxY - Collider_Component.FootPoint.y;
                if (_Drop > maxFallDis)
                {
                    //print($"扣除血量:{_Drop - maxFallDis}");
                    UpdateEntityLife(-(int)(_Drop - maxFallDis), Vector3.zero);
                    realMaxY = Collider_Component.FootPoint.y;

                    //播放落地音效
                    managerhub.NewmusicManager.Create3DSound(transform.position, Default_DropGround);
                }
            }


        } 

        #endregion

    }

}

