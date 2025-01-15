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

        #region 状态

        [Foldout("状态", true)]

        [ReadOnly] public bool isEntity_Hurt = false;
        [ReadOnly] public bool isEntity_Dead = false;
        [ReadOnly] public bool isEntity_Dive = false;

        #endregion


        #region 周期函数

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
                originalColor = EntityMat.color; // 获取材质的原始颜色
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


        #region 生命值部分

        [Foldout("生命值设置", true)]
        [Header("实体生命值")] public int EntityBlood = 20;
        [Header("材质引用")] public Material EntityMat;
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

            //死亡动画
            if (animationCoponent != null && animationCoponent.GetClip("EntityDead") != null)
            {
                animationCoponent.Play("EntityDead");
            }
            else
            {
                print("找不到");
            }


            //蒸汽粒子
            // 创建实例，并将父对象设置为 particleParent
            GameObject deadParticle = GameObject.Instantiate(
                Evaporation_Particle,
                transform.position,
                Quaternion.LookRotation(Vector3.up),
                ParticleParent.transform  // 设置父对象
            );


            //创建掉落物
            Vector3 randomPoint = Random.insideUnitSphere / 2f;
            Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position, new BlockItem(VoxelData.Slimeball, 1), false);
            Collider_Component.managerhub.backpackManager.CreateDropBox(this.transform.position + randomPoint, new BlockItem(VoxelData.Apple, 2), false);

            Destroy(this.gameObject, 0.5f);

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

    }

}

