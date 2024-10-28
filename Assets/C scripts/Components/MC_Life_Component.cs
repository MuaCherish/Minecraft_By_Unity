using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    public class MC_Life_Component : MonoBehaviour
    {


        #region ���ں���

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


        [Header("ʵ������ֵ")] public float Blood = 20f;


        /// <summary>
        /// ����ʵ������ֵ
        /// </summary>
        public void UpdateEntityLife(float _updateLifeValue)
        {

            Blood += _updateLifeValue;


        }


        //ʵ������
        void EntityDead()
        {

        }


        /// <summary>
        /// ����Ѫ��
        /// </summary>
        public void AddBlood()
        {

        }

        /// <summary>
        /// �趨Ѫ��
        /// </summary>
        public void SetBlood()
        {

        }


    }

}

