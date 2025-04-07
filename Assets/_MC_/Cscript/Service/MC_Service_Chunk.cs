using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using System.Collections;
using static MC_Static_Math;

public class MC_Service_Chunk : MonoBehaviour
{

    #region ���ں���

    ManagerHub managerhub;
    World world;
    Player player;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        world = managerhub.world;
        player = managerhub.player;
    }

    #endregion

    #region ��ʼ����ͼ

    public IEnumerator Init_Map_Thread(bool _isInitPlayerLocation)
    {
        //ȷ��Chunk���ĵ�
        GetChunkCenterNow();

        //��ʼ�����鲢��ӽ�����
        float temp = 0f;
        for (int x = -world.renderSize + (int)(world.Center_Now.x / TerrainData.ChunkWidth); x < world.renderSize + (int)(world.Center_Now.x / TerrainData.ChunkWidth); x++)
        {
            for (int z = -world.renderSize + (int)(world.Center_Now.z / TerrainData.ChunkWidth); z < world.renderSize + (int)(world.Center_Now.z / TerrainData.ChunkWidth); z++)
            {
                CreateBaseChunk(new Vector3(x, 0, z));
                temp++;
                float max = world.renderSize * world.renderSize * 4;
                managerhub.canvasManager.Initprogress = Mathf.Lerp(0f, 0.9f, temp / max);
                yield return new WaitForSeconds(world.InitCorountineDelay);
            }

        }

        //���³�ʼ�����λ�ã���ֹ��ģ
        if (_isInitPlayerLocation)
        {
            managerhub.player.InitPlayerLocation();
        }


        //��Ϸ��ʼ
        yield return new WaitForSeconds(0.5f);
        managerhub.canvasManager.Initprogress = 1f;

        //�������Ż�Э��
        StartCoroutine(Chunk_Optimization());
        StartCoroutine(FlashChunkCoroutine());

        world.Init_MapCoroutine = null;
    }

    //ȷ��Chunk���ĵ�
    void GetChunkCenterNow()
    {

        if (world.isLoadSaving)
        {
            world.Center_Now = new Vector3(GetRealChunkLocation(world.worldSetting.playerposition).x, 0, GetRealChunkLocation(world.worldSetting.playerposition).z);
            return;
        }


        if (world.hasExec_RandomPlayerLocation)
        {
            managerhub.player.RandomPlayerLocaiton();
            world.hasExec_RandomPlayerLocation = false;
        }
        //print(PlayerFoot.transform.position);
        world.Center_Now = new Vector3(GetRealChunkLocation(player.foot.transform.position).x, 0, GetRealChunkLocation(player.foot.transform.position).z);

    }

    #endregion

    #region ��̬���ص�ͼ

    public void Update_CenterWithNoInit()
    {
        if (world.Init_Map_Thread_NoInit_Coroutine == null)
        {
            //print("����ƶ�̫�죡Center_Now�Ѹ���");
            world.Init_Map_Thread_NoInit_Coroutine = StartCoroutine(Init_Map_Thread_NoInit());

            //managerhub.timeManager.UpdateDayFogDistance();
            HideFarChunks();
        }
    }

    IEnumerator Init_Map_Thread_NoInit()
    {

        world.Center_Now = new Vector3(GetRealChunkLocation(player.foot.transform.position).x, 0, GetRealChunkLocation(player.foot.transform.position).z);

        for (int x = -world.renderSize + (int)(world.Center_Now.x / TerrainData.ChunkWidth); x < world.renderSize + (int)(world.Center_Now.x / TerrainData.ChunkWidth); x++)
        {

            for (int z = -world.renderSize + (int)(world.Center_Now.z / TerrainData.ChunkWidth); z < world.renderSize + (int)(world.Center_Now.z / TerrainData.ChunkWidth); z++)
            {

                //Create
                CreateBaseChunk(new Vector3(x, 0, z));

                yield return new WaitForSeconds(world.InitCorountineDelay);
            }

        }

        world.Init_Map_Thread_NoInit_Coroutine = null;

    }


    //������������
    public void Update_CenterChunks(bool _isInitPlayerLocation)
    {
        //print("������������");
        //update������������
        if (world.Init_MapCoroutine == null)
        {
            world.Init_MapCoroutine = StartCoroutine(Init_Map_Thread(_isInitPlayerLocation));
        }


    }

    //�����Զ����
    public void HideFarChunks()
    {
        foreach (var temp in world.Allchunks)
        {
            if (Get2DLengthforVector3(player.foot.transform.position - temp.Value.myposition) > (world.StartToRender * 16f))
            {
                temp.Value.HideChunk();
            }
        }
    }

    //�Ż�Chunk����Э��
    //�������ǰ�BaseChunkȫ����������һ��

    public IEnumerator Chunk_Optimization()
    {

        foreach (var Chunk in world.Allchunks)
        {

            world.WaitToCreateMesh.Enqueue(Chunk.Value);

        }



        yield return new WaitForSeconds(1f);

    }


    //����ѽ��յĵ���Chunk�ö��߳�ˢ��
    public IEnumerator FlashChunkCoroutine()
    {

        while (true)
        {

            if (world.WaitToFlashChunkQueue.TryDequeue(out Chunk chunktemp))
            {
                Thread myThread = new Thread(new ThreadStart(chunktemp.UpdateChunkMesh_WithSurround));
                myThread.Start();
            }


            yield return null;
        }



    }


    //��ӵ��ȴ���Ӷ���
    public void AddtoCreateChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) + world.Center_direction * (world.renderSize - 1);

            //����Chunk
            for (int i = -world.renderSize; i < world.renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));

                world.WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));


            }

            //�������Chunk����
            for (int i = -world.renderSize; i < world.renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));

                lock (world.Allchunks_Lock)
                {

                    if (world.Allchunks.TryGetValue(add_vec + new Vector3((float)i, 0, -1), out Chunk chunktemp))
                        world.WaitToCreateMesh.Enqueue(chunktemp);

                }





            }

        }

        //Back
        if (add_vec == new Vector3(0, 0, -1))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) + world.Center_direction * (world.renderSize);

            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                world.WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

            //�������Chunk����
            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                lock (world.Allchunks_Lock)
                {

                    if (world.Allchunks.TryGetValue(add_vec + new Vector3((float)i, 0, 1), out Chunk chunktemp))
                        world.WaitToCreateMesh.Enqueue(chunktemp);

                }




            }

        }

        //Left
        if (add_vec == new Vector3(-1, 0, 0))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) + world.Center_direction * (world.renderSize);

            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                //CreateChunk(add_vec + new Vector3(0, 0, (float)i));
                world.WatingToCreate_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }

            //�������Chunk����
            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                lock (world.Allchunks_Lock)
                {

                    //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                    if (world.Allchunks.TryGetValue(add_vec + new Vector3(1, 0, (float)i), out Chunk chunktemp))
                        world.WaitToCreateMesh.Enqueue(chunktemp);

                }



            }

        }



        //Right
        if (add_vec == new Vector3(1, 0, 0))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) + world.Center_direction * (world.renderSize - 1);

            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                //CreateChunk(add_vec + new Vector3(0, 0, (float)i));
                world.WatingToCreate_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }


            //�������Chunk����
            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                lock (world.Allchunks_Lock)
                {

                    //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                    if (world.Allchunks.TryGetValue(add_vec + new Vector3(-1, 0, (float)i), out Chunk chunktemp))
                        world.WaitToCreateMesh.Enqueue(chunktemp);

                }



            }

        }

        //Debug.Log("�Ѿ��������");


        //�ж��Ƿ�����Э��
        //���������������Э�̣��������ݶ���һ������Э��
        if (world.WatingToCreate_Chunks.Count > 0 && world.CreateCoroutine == null)
        {

            world.CreateCoroutine = StartCoroutine(CreateChunksQueue());

        }


    }

    //Э�̣�����Chunk
    private IEnumerator CreateChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(world.CreateCoroutineDelay);


            //��������������ݣ��Ͷ�ȡ
            //���������û�����ݣ��͹ر�Э��
            if (world.WatingToCreate_Chunks.Count > 0)
            {

                //����鵽��chunk�Ѿ����ڣ�����
                //������������
                if (world.Allchunks.TryGetValue(world.WatingToCreate_Chunks[0], out world.obj))
                {

                    if (world.obj.isShow == false)
                    {

                        world.obj.ShowChunk();

                    }

                }
                else
                {

                    CreateChunk(world.WatingToCreate_Chunks[0]);

                }

                world.WatingToCreate_Chunks.RemoveAt(0);

            }
            else
            {

                world.CreateCoroutine = null;
                break;

            }



        }

    }

    //����Chunk
    //BaseChunk������������޳�
    public void CreateBaseChunk(Vector3 pos)
    {

        //���ж�һ����û��
        if (world.Allchunks.ContainsKey(pos))
        {
            world.Allchunks[pos].ShowChunk();
            return;
        }

        //����Chunk
        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;


        //if (_ChunkLocation == new Vector3(195f,0,89f))
        //{
        //    print("");
        //}

        //����Chunk
        if (world.ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false, world.TheSaving[world.GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
        }
        else
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false);
        }

        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //��ӵ��ֵ�
        world.Allchunks.Add(_ChunkLocation, _chunk_temp);

    }

    //��BaseChunk�����Chunk���޳�
    void CreateChunk(Vector3 pos)
    {

        //���ж�һ����û��
        if (world.Allchunks.ContainsKey(pos))
        {

            world.Allchunks[pos].ShowChunk();
            return;

        }


        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;

        //����Chunk
        if (world.ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false, world.TheSaving[world.GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
        }
        else
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false);
        }


        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //��ӵ��ֵ�
        world.Allchunks.Add(pos, _chunk_temp);

    }

    //��ӵ��ȴ�ɾ������
    public void AddtoRemoveChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) - world.Center_direction * (world.renderSize + 1);

            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                world.WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

        }



        //Back
        if (add_vec == new Vector3(0, 0, -1))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) - world.Center_direction * (world.renderSize);

            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                world.WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

        }



        //Left
        if (add_vec == new Vector3(-1, 0, 0))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) - world.Center_direction * (world.renderSize);

            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                world.WatingToRemove_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }

        }

        //Right
        if (add_vec == new Vector3(1, 0, 0))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) - world.Center_direction * (world.renderSize + 1);

            for (int i = -world.renderSize; i < world.renderSize; i++)
            {

                world.WatingToRemove_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }

        }

        //�ж��Ƿ�����Э��
        //���������������Э�̣��������ݶ���һ������Э��
        if (world.WatingToRemove_Chunks.Count > 0 && world.RemoveCoroutine == null)
        {

            world.RemoveCoroutine = StartCoroutine(RemoveChunksQueue());

        }

    }

    //Э�̣�ɾ��ChunK
    private IEnumerator RemoveChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(world.RemoveCoroutineDelay);


            //��������������ݣ��Ͷ�ȡ
            //���������û�����ݣ��͹ر�Э��
            if (world.WatingToRemove_Chunks.Count > 0)
            {

                if (world.Allchunks.TryGetValue(world.WatingToRemove_Chunks[0], out world.obj))
                {

                    Chunk_HideOrRemove(world.WatingToRemove_Chunks[0]);

                    world.WatingToRemove_Chunks.RemoveAt(0);

                }
                else
                {

                    world.WatingToRemove_Chunks.RemoveAt(0);
                    world.WatingToRemove_Chunks.Add(world.WatingToRemove_Chunks[0]);

                }


            }
            else
            {

                world.RemoveCoroutine = null;
                break;

            }



        }

    }


    #endregion

    #region ��������(ʲô��ʺ����)

    void Chunk_HideOrRemove(Vector3 chunklocation)
    {
        world.obj.HideChunk();
    }

    #endregion

    #region ��Ⱦ����

    //��ȾЭ�̳�
    public void RenderCoroutineManager()
    {
        // ����ȴ���Ⱦ�Ķ��в�Ϊ�գ�����û���������е���ȾЭ��
        if (world.WaitToRender.Count != 0 && world.Render_Coroutine == null)
        {
            //print($"������ȾЭ��");
            world.Render_Coroutine = StartCoroutine(Render_0());
        }
    }

    // һ����ȾЭ��
    IEnumerator Render_0()
    {
        bool hasError = false;  // ����Ƿ����쳣

        while (true)
        {
            try
            {
                // ���ԴӶ�����ȡ��Ҫ��Ⱦ��Chunk
                if (world.WaitToRender.TryDequeue(out Chunk chunktemp))
                {
                    //print($"{GetRelaChunkLocation(chunktemp.myposition)}��ʼ��Ⱦ");

                    // ���Chunk�Ѿ�׼������Ⱦ������CreateMesh
                    if (chunktemp.isReadyToRender)
                    {
                        chunktemp.CreateMesh();
                    }
                }

                // �������Ϊ�գ�ֹͣЭ��
                if (world.WaitToRender.Count == 0)
                {
                    //print($"����Ϊ�գ�ֹͣЭ��");
                    world.Render_Coroutine = null;
                    world.RenderLock = false;
                    break;
                }
            }
            catch (Exception ex)
            {
                // �����쳣����ֹЭ�����쳣��ֹ
                Debug.LogError($"��ȾЭ�̳���: {ex.Message}\n{ex.StackTrace}");

                hasError = true;  // ��Ƿ�������
                break;  // �˳���ǰѭ�����ȴ�����������
            }

            // ��������ȴ�һ��ʱ���Կ�����ȾƵ��
            yield return new WaitForSeconds(world.RenderDelay);
        }

        // ����������쳣���ȴ�������Э��
        if (hasError)
        {
            world.Render_Coroutine = null;  // ����Э��״̬
            yield return new WaitForSeconds(1f);  // �ȴ�һ��ʱ��
            RenderCoroutineManager();  // ����������ȾЭ��
        }
    }

    //MeshЭ��
    public void CreateMeshCoroutineManager()
    {

        if (world.WaitToCreateMesh.Count != 0 && world.Mesh_Coroutine == null)
        {

            world.Mesh_Coroutine = StartCoroutine(Mesh_0());

        }

    }

    bool hasExec_CaculateInitTime = true;
    public float OneChunkRenderTime;
    IEnumerator Mesh_0()
    {

        while (true)
        {

            if (world.MeshLock == false)
            {

                world.MeshLock = true;

                world.WaitToCreateMesh.TryDequeue(out Chunk chunktemp);

                //print($"{GetRelaChunkLocation(chunktemp.myposition)}��ӵ�meshQueue");

                //Mesh�߳�
                Thread myThread = new Thread(new ThreadStart(chunktemp.UpdateChunkMesh_WithSurround));
                myThread.Start();

                if (world.WaitToCreateMesh.Count == 0)
                {
                    if (hasExec_CaculateInitTime)
                    {
                        //print("��Ⱦ����");
                        world.InitEndTime = Time.time;

                        //renderSize * renderSize * 4������������2����Ϊ���޳���Ⱦ������
                        OneChunkRenderTime = (world.InitEndTime - world.InitStartTime) / (world.renderSize * world.renderSize * 4 * 2);

                        hasExec_CaculateInitTime = false;
                    }

                    world.Mesh_Coroutine = null;
                    break;

                }




            }

            world.Mesh_0_TaskCount = world.WaitToCreateMesh.Count;
            //print("WaitToCreateMesh.Count");
            yield return new WaitForSeconds(world.RenderDelay);


        }

    }

    #endregion

    #region ����Chunk����

    //Vector3 --> ���������
    public Chunk GetChunkObject(Vector3 pos)
    {

        world.Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk chunktemp);

        return chunktemp;

    }

    //New-��ȡ�������
    public bool TryGetChunkObject(Vector3 pos, out Chunk chunktemp)
    {

        if (world.Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk _chunktemp))
        {
            chunktemp = _chunktemp;
            return true;
        }

        chunktemp = null;
        return false;
    }

    #endregion

    #region ��ȡ����

    /// <summary>
    /// ���ط�������,������Ǿ�������
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public byte GetBlockType(Vector3 pos)
    {
        //��ǰ����-�Ҳ�������
        if (!world.Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk chunktemp))
            return 0;

        //��ǰ����-������������߽�
        if (MC_Static_Chunk.isOutOfChunkRange(pos))
            return 0;

        //��ȡ�������
        Vector3 _vec = GetRelaPos(pos);

        //��ȡBlock����
        byte block_type = chunktemp.GetBlock((int)_vec.x, (int)_vec.y, (int)_vec.z).voxelType;

        //Return
        return block_type;

    }

    #endregion

    #region �ṹ����(δӦ��)

    //���ڼ�¼����
    //recordData.pos����Ǿ�������
    private List<EditStruct> recordData = new List<EditStruct>();

    //��¼����
    public void RecordBuilding(Vector3 _Start, Vector3 _End)
    {
        recordData.Clear();


        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {

            }
        }
    }

    //�ͷŽ���
    //����0�±��end�±꣬��������������飬Ȼ������ݶ������鼴��
    public void ReleaseBuilding(Vector3 place, List<EditStruct> _recordData)
    {

        Vector3 start = _recordData[0].editPos;
        Vector3 end = _recordData[_recordData.Count - 1].editPos;

        for (float x = start.x; x < end.x; x += 16f)
        {
            for (float z = start.z; z < end.z; z += 16f)
            {

            }
        }
    }


    #endregion

    #region �޸ķ���

    //����޸ķ���
    public void EditBlock(Vector3 _pos, byte _target)
    {
        world.Allchunks[GetRelaChunkLocation(_pos)].EditData(_pos, _target);
    }

    public void EditBlock(List<EditStruct> _editStructs)
    {
        List<Vector3> _ChunkLocations = new List<Vector3>();

        // ����_editStructs���洢ChunkLocations
        foreach (var item in _editStructs)
        {

            // ���allchunks��û��pos.��_ChunkLocations���
            if (world.Allchunks.ContainsKey(GetRelaChunkLocation(item.editPos)))
            {
                if (!_ChunkLocations.Contains(GetRelaChunkLocation(item.editPos)))
                {
                    _ChunkLocations.Add(GetRelaChunkLocation(item.editPos));
                }


            }
            else
            {
                print($"���鲻����:{GetRelaChunkLocation(item.editPos)}");

            }


        }

        // ����_ChunkLocations����allchunk���_ChunkLocationsִ��EditData
        foreach (var chunkLocation in _ChunkLocations)
        {
            world.Allchunks[chunkLocation].EditData(_editStructs);
        }

        // ��ӡ�ҵ�����������
        //print($"�ҵ�{_ChunkLocations.Count}��");
    }


    Coroutine editBlockCoroutine;
    public void EditBlock(List<EditStruct> _editStructs, float _time)
    {
        if (editBlockCoroutine == null)
        {
            editBlockCoroutine = StartCoroutine(Coroutine_editBlock(_editStructs, _time));
        }


    }

    IEnumerator Coroutine_editBlock(List<EditStruct> _editStructs, float _time)
    {
        foreach (var item in _editStructs)
        {
            //print("ִ��EditBlocks");
            world.Allchunks[GetRelaChunkLocation(item.editPos)].EditData(item.editPos, item.targetType);

            yield return new WaitForSeconds(_time);
        }
        editBlockCoroutine = null;
    }

    #endregion

    #region ������

    //�������ײ�еķ����ж�
    //true������ײ
    public bool CollisionCheckForVoxel(Vector3 pos)
    {

        Vector3 realLocation = pos; //��������
        Vector3 relaLocation = GetRelaPos(realLocation);
        byte targetBlock = GetBlockType(realLocation);

        //�����ж�(Chunk)
        if (!world.Allchunks.ContainsKey(GetRelaChunkLocation(realLocation)))
            return true;

        //�����ж�(Y)
        if (realLocation.y >= TerrainData.ChunkHeight || realLocation.y < 0)
            return false;

        //������Զ�����ײ
        if (world.blocktypes[targetBlock].isSolid && world.blocktypes[targetBlock].isDIYCollision)
        {
            realLocation = CollisionOffset(realLocation, targetBlock);
            Vector3 OffsetrelaLocation = GetRelaPos(realLocation);

            if (OffsetrelaLocation != relaLocation)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //���ع��廹�ǿ���
        return world.blocktypes[world.Allchunks[GetRelaChunkLocation(realLocation)].voxelMap[(int)relaLocation.x, (int)relaLocation.y, (int)relaLocation.z].voxelType].isSolid;

    }

    //public float XOFFSET;
    //public float YOFFSET;
    //public float ZOFFSET;

    public Vector3 CollisionOffset(Vector3 _realPos, byte _targetType)
    {
        Vector3 _input = new Vector3(managerhub.player.horizontalInput, managerhub.player.Facing.y, managerhub.player.verticalInput);
        float _x = _realPos.x; Vector2 _xRange = world.blocktypes[_targetType].CollosionRange.xRange; float _xOffset = _x - (int)_x;
        float _y = _realPos.y; Vector2 _yRange = world.blocktypes[_targetType].CollosionRange.yRange; float _yOffset = _y - (int)_y;
        float _z = _realPos.z; Vector2 _zRange = world.blocktypes[_targetType].CollosionRange.zRange; float _zOffset = _z - (int)_z;


        //X
        if (_input.x >= 0 || _xOffset < _xRange.x)
            _x -= _xRange.x;
        else if (_input.x < 0 || _xOffset > _xRange.y)
            _x += (1 - _xRange.y);


        //Y
        if (_input.y >= 0 || _yOffset < _yRange.x)
            _y -= _yRange.x;
        else if (_input.y < 0 || _yOffset > _yRange.y)
            _y += (1 - _yRange.y);

        //Z
        if (_input.z >= 0 || _zOffset < _zRange.x)
            _z -= _zRange.x;
        else if (_input.z < 0 || _zOffset > _zRange.y)
            _z += (1 - _zRange.y);

        return new Vector3(_x, _y, _z);
    }

    //���ø��������
    //�����۾����ߵļ��
    public bool RayCheckForVoxel(Vector3 pos)
    {

        Vector3 realLocation = pos; //��������
        Vector3 relaLocation = GetRelaPos(realLocation);
        byte targetBlock = GetBlockType(realLocation);

        if (!world.Allchunks.ContainsKey(GetRelaChunkLocation(pos))) { return false; }

        //�����������
        Vector3 vec = GetRelaPos(new Vector3(pos.x, pos.y, pos.z));

        //�ж�XOZ����û�г���
        if (!world.Allchunks.ContainsKey(GetRelaChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (realLocation.y >= TerrainData.ChunkHeight) { return false; }


        //������Զ�����ײ
        if (world.blocktypes[targetBlock].isDIYCollision)
        {
            realLocation = CollisionOffset(realLocation, targetBlock);
            Vector3 OffsetrelaLocation = GetRelaPos(realLocation);

            if (OffsetrelaLocation != relaLocation)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //���ع��廹�ǿ���
        return world.blocktypes[world.Allchunks[GetRelaChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z].voxelType].canBeChoose;

    }


    #endregion

    #region ָ������Ѱַ

    //ָ������Ѱַ
    public Vector3 AddressingBlock(Vector3 _start, int _direct)
    {
        Vector3 _address = _start;
        //print($"start: {_address}");

        for (int i = 0; i < TerrainData.ChunkHeight; i++)
        {
            byte _byte = GetBlockType(_address);
            if (_byte != VoxelData.Air)
            {
                //print($"���꣺{_address} , ����{_byte}");
                //���һ�������ڽ�
                if (_byte == VoxelData.Water)
                {
                    EditBlock(_address, VoxelData.Grass);
                }

                //Offset
                return _address + new Vector3(0.5f, 2f, 0.5f);
            }

            _address += VoxelData.faceChecks[_direct];
        }

        print("Ѱַʧ��");
        return _start;

    }

    //����һ����ʼ����ͳ�ʼ���򣬳�������������ChunkHeight������һ���ǿ�������
    public Vector3 LoopAndFindABestLocation(Vector3 _start, Vector3 _direct)
    {
        _direct.x = _direct.x > 0 ? 1 : 0;
        _direct.y = _direct.y > 0 ? 1 : 0;
        _direct.z = _direct.z > 0 ? 1 : 0;

        Vector3 _next = _start;

        //Loop
        for (int i = 0; i < TerrainData.ChunkHeight; i++)
        {
            // Check�������ǰλ�õķ������Ͳ��ǿ��������ظ�����
            if (GetBlockType(_next) != VoxelData.Air)
            {
                return _next;
            }

            // �ۻ��ƶ�λ��
            _next += _direct; // ʹ�ù�һ���ķ����������ƶ�
        }

        return _start;
    }

    #endregion

    #region ���ؿ��ó�����

    public void GetSpawnPos(Vector3 _pos, out List<Vector3> _Spawns)
    {
        _Spawns = new List<Vector3>();
        Vector3 _ChunkLocation = GetRelaChunkLocation(_pos);

        //��ǰ����-û������
        if (!world.Allchunks.TryGetValue(_ChunkLocation, out Chunk _chunktemp))
            return;

        _chunktemp.GetSpawnPos(GetRelaPos(_pos), out List<Vector3> __Spawns);
        _Spawns = new List<Vector3>(__Spawns);
    }

    #endregion

}
