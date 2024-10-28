using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    public class MC_Life_Component : MonoBehaviour
    {


        #region 周期函数

        MC_Velocity_Component Velocity_Component;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
        }


        private void Start()
        {
            
        }


        private void Update()
        {
            
        }

        #endregion


        [Header("实体生命值")] public float Blood = 20f;


        /// <summary>
        /// 更新实体生命值
        /// </summary>
        public void UpdateEntityLife(float _updateLifeValue)
        {

            Blood += _updateLifeValue;


        }


        //实体死亡
        void EntityDead()
        {

        }


        /// <summary>
        /// 更新血量
        /// </summary>
        public void AddBlood()
        {

        }

        /// <summary>
        /// 设定血量
        /// </summary>
        public void SetBlood()
        {

        }


    }

}

