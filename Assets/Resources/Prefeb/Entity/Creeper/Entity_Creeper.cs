using MCEntity;
using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class Entity_Creeper : MonoBehaviour
{

    #region ���ں���

    ManagerHub managerhub;
    MC_Service_World Service_world;
    Player player;
    MC_Component_AI Component_AI;
    MC_Component_Buff Buff_Component;

    private void Awake()
    {
        Service_world = SceneData.GetManagerhub().Service_World;
        Component_AI = GetComponent<MC_Component_AI>();
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
        Buff_Component = GetComponent<MC_Component_Buff>();
    }

    private void Update()
    {
        if (MC_Runtime_DynamicData.instance.GetGameState() == Game_State.Playing)
        {
            Handle_GameState_Playing();
        }
    }

    void Handle_GameState_Playing()
    {
        _ReferUpdate_Handle_StartFuse();
    }

    #endregion


    #region Creeper

    // ������Ҵ�������
    [Header("�Ƿ񴥷�������")] public bool startFuse;
    [Header("�������ž���")] public float DistanceToFuse = 3f;
    [Header("�ر����ž���")] public float DistanceToStopFuse = 5f;
    [Header("����ʱ��")] public float FuseTime = 3f;

    Coroutine _Coroutine_FuseToExplore;

    void _ReferUpdate_Handle_StartFuse()
    {
        //��ǰ����-ֻ������ģʽ��������
        if (MC_Runtime_DynamicData.instance.GetGameMode() != GameMode.Survival)
            return;

        float distanceToPlayer = (transform.position - player.transform.position).magnitude;

        // �������δ����������ҽ��봥����Χ
        if (!startFuse && Component_AI.isSeePlayer && distanceToPlayer < DistanceToFuse)
        {
            startFuse = true;
            Component_AI.Debug_PauseAI = true;
            Buff_Component.AddBuff(BuffData.Blink, 3f); // ���� Blink Buff

            // ȷ�� Coroutine ֻ������һ��
            if (_Coroutine_FuseToExplore == null)
                _Coroutine_FuseToExplore = StartCoroutine(StartFuseToExplore());
        }

        // �������뿪������Χ��������ֹͣЭ�̣����ǵȴ���ǰ�������
        if (startFuse && distanceToPlayer >= DistanceToStopFuse)
        {
            startFuse = false;
            Component_AI.Debug_PauseAI = false;
            if (_Coroutine_FuseToExplore != null)
            {
                StopCoroutine(_Coroutine_FuseToExplore);
                _Coroutine_FuseToExplore = null;
            }
            
        }
    }

    // ���Ŵﵽһ��ʱ�䴥����ը
    IEnumerator StartFuseToExplore()
    {
        float _ExploreTimer = 0;

        // �ȴ�һ��ʱ��
        while (_ExploreTimer < FuseTime)
        {
            // ��ǰ����-û�д������Ų���ʱ
            if (!startFuse)
            {
                // ����ȡ����Э���˳�
                _Coroutine_FuseToExplore = null;
                yield break;
            }

            _ExploreTimer += Time.deltaTime;
            yield return null; // �ȴ���һ֡
        }

        // ��ը����
        Buff_Component.AddBuff(BuffData.SwellandExplore, 2f); // ������ը Buff
        _Coroutine_FuseToExplore = null;
    }

    #endregion

}
