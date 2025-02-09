using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Debug_Navigate : MonoBehaviour
{


    #region ���ں���

    ManagerHub managerhub;
    World world;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        world = managerhub.world;
    }

    private void Update()
    {
        if (managerhub.world.game_state == Game_State.Playing)
        {
            if (Toggle_StartToFindDestination)
            {
                // ��ʼ��
                Toggle_StartToFindDestination = false;
                VisitPositions.Clear();  // ��ռ�¼��·����
                currentIndex = 0;

                // ����
                RandomWalkDestination(transform.position, out var result);

                // Gizmos
                VisitPositions = new List<Vector3>(result);
                StartCoroutine(DrawPathWithDelay());

            }
        }

        
    }


    #endregion


    #region ��������㷨

    [Header("ִ����������㷨")] public bool Toggle_StartToFindDestination;
    [Header("���������߲���")] public int WalkSteps = 10;  // �� N ��
    [Header("��ѭ�ϸ��������")] public float prevDirectionProbability = 0f;

    /// <summary>
    /// ��������㷨
    /// </summary>
    /// <param name="_StartPos"></param>
    /// <param name="VisitPositions"></param>
    void RandomWalkDestination(Vector3 _StartPos, out List<Vector3> VisitPositions)
    {
        // ��ʼ��
        Vector3 _thisPos = UsefulFunction.GetCenterVector3(_StartPos);
        Vector3 _thisDirect = EntityData.RandomWalkFace[Random.Range(0, 8)];
        VisitPositions = new List<Vector3>{ _StartPos };

        //��ǰ����-��ʼ�㱻��ס || ��ʼ������
        byte _StartPosBlockType = world.GetBlockType(_thisPos);
        byte _StartDownPosBlockType = world.GetBlockType(_thisPos + Vector3.down);
        if (_StartPosBlockType == 255 || world.blocktypes[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
            return;

        // ���� N ��
        for (int i = 0; i < WalkSteps; i++)
        {
            // ������һ��Ŀ��λ��
            //���������ɶҲû�ҵ�����ֱ�ӽ�������
            if (!GetOneDestination(VisitPositions, _thisPos, _thisDirect, out var _nextPos, out var _nextDirect))
                break;

            // ����λ�úͷ���
            _thisPos = _nextPos;
            _thisDirect = _nextDirect;

            // ������һ��λ��
            VisitPositions.Add(_nextPos);
        }
    }

    //Ѱ�ҵ����㷶Χ�ڵ���һ����
    bool GetOneDestination(List<Vector3> _VisitPositions, Vector3 _LastPos, Vector3 _LastDirect, out Vector3 _nextPos, out Vector3 _nextDirect)
    {
        _nextPos = Vector3.zero;
        _nextDirect = Vector3.zero;

        // �����ѭ��һ������ĸ��ʴ������ֵ�����������һ������
        if (Random.value < prevDirectionProbability)
        {
            _nextPos = _LastPos + _LastDirect;
            _nextDirect = _LastDirect;

            // ����õ���Ч����ǰ�˳�
            if (CheckVisit(_LastPos, _LastDirect, _nextPos, _nextDirect, _VisitPositions)) 
                return true; 
        }

        // ���ҷ���˳�򲢱���
        Vector3[] shuffledDirections = ShuffleArray(EntityData.RandomWalkFace);
        int errorNumber = 0;
        foreach (var _direct in shuffledDirections)
        {
            _nextPos = _LastPos + _direct;
            _nextDirect = _direct;

            // ����õ㲻���У�������һ������
            if (CheckVisit(_LastPos, _LastDirect, _nextPos, _nextDirect, _VisitPositions))
            {
                break;
            }
            else
            {
                errorNumber = errorNumber + 1;
                continue;
            }

            
        }

        //�ж��Ƿ���Ͷ��· 
        if (errorNumber >= 0 && errorNumber <= 7)
        {
            return true;
        }
        else
        {
            print("·����Ͷ��·");
            return false;
        }

    }

    //���ú����Լ��
    bool CheckVisit(Vector3 _LastPos,Vector3 _LastDirect, Vector3 _nextPos,Vector3 _nextDirect, List<Vector3> _VisitPositions)
    {
        byte _thisBlockType = world.GetBlockType(_nextPos);

        //[World] ��ǰ����-���û�з������� || ���Ŀ���ǹ���
        if (_thisBlockType == 255 || world.blocktypes[_thisBlockType].isSolid)
            return false;

        //[Path] ��ǰ����-�Ѿ��߹���·
        if (_VisitPositions.Contains(_nextPos))
            return false;

        //��ǰ����-���ֱ�Ǳ�ռ������Խ��߷�����Ч
        if (_nextDirect == new Vector3(1f, 0f, 1f))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, 1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }
        else if (_nextDirect == new Vector3(1f, 0f, -1f))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, -1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }
        else if (_nextDirect == new Vector3(-1f, 0f, -1))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(-1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, -1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }
        else if (_nextDirect == new Vector3(-1f, 0f, 1f))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(-1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, 1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }

        //ͨ�����м��
        return true;
    }

    #endregion


    #region Gizmos

    [Header("����Cube���")] public float CubeWidth = 0.8f;
    [Header("ÿ�����ӳ�ʱ��")] public float StepDelay = 0.5f; // �� Gizmos ���ӻ�ʱ�м��
    private List<int> DrawOrder = new List<int>(); // �����洢����˳��
    private int currentIndex = 0; // ��ǰ���ڻ��Ƶ�λ��
    public List<Vector3> VisitPositions = new List<Vector3>();

    // ����·��
    private IEnumerator DrawPathWithDelay()
    {
        DrawOrder.Clear(); // ��ջ���˳��

        // ��ÿ�����������ӵ�����˳����
        for (int i = 0; i < VisitPositions.Count; i++)
        {
            DrawOrder.Add(i);
        }

        // �ӳٻ���ÿһ������
        for (int i = 0; i < DrawOrder.Count; i++)
        {
            yield return new WaitForSeconds(StepDelay);
            currentIndex = i; // ���µ�ǰ���Ƶ�����
        }

    }

    private void OnDrawGizmos()
    {
        if (VisitPositions.Count > 0 && currentIndex < DrawOrder.Count)
        {
            // ��������·����
            for (int i = 0; i <= currentIndex; i++)
            {
                DrawCube(i);
            }
        }
    }

    // ���Ʒ���
    private void DrawCube(int index)
    {
        // �������Ϊ��ɫ���յ�Ϊ��ɫ��������Ϊ��ɫ
        Gizmos.color = (index == 0) ? Color.green : (index == VisitPositions.Count - 1) ? Color.red : Color.white;
        Gizmos.DrawWireCube(VisitPositions[index], new Vector3(CubeWidth, CubeWidth, CubeWidth));
    }

    #endregion


    #region Tool

    // ����Vector3���鲢����������
    public static Vector3[] ShuffleArray(Vector3[] array)
    {
        Vector3[] shuffledArray = (Vector3[])array.Clone(); // �������飬�����޸�ԭ����
        System.Random rng = new System.Random();

        for (int i = shuffledArray.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1); // ���� 0 �� i ֮����������
            (shuffledArray[i], shuffledArray[j]) = (shuffledArray[j], shuffledArray[i]); // ����Ԫ��
        }

        return shuffledArray;
    }

    #endregion

}
