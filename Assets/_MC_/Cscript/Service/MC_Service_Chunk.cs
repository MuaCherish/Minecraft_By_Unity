using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using System.Collections;
using static MC_Static_Math;

public class MC_Service_Chunk : MonoBehaviour
{

    #region 周期函数

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

    #region 初始化地图

    public IEnumerator Init_Map_Thread(bool _isInitPlayerLocation)
    {
        //确定Chunk中心点
        GetChunkCenterNow();

        //初始化区块并添加进度条
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

        //重新初始化玩家位置，防止穿模
        if (_isInitPlayerLocation)
        {
            managerhub.player.InitPlayerLocation();
        }


        //游戏开始
        yield return new WaitForSeconds(0.5f);
        managerhub.canvasManager.Initprogress = 1f;

        //开启面优化协程
        StartCoroutine(Chunk_Optimization());
        StartCoroutine(FlashChunkCoroutine());

        world.Init_MapCoroutine = null;
    }

    //确定Chunk中心点
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

    #region 动态加载地图

    public void Update_CenterWithNoInit()
    {
        if (world.Init_Map_Thread_NoInit_Coroutine == null)
        {
            //print("玩家移动太快！Center_Now已更新");
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


    //更新中心区块
    public void Update_CenterChunks(bool _isInitPlayerLocation)
    {
        //print("更新中心区块");
        //update加载中心区块
        if (world.Init_MapCoroutine == null)
        {
            world.Init_MapCoroutine = StartCoroutine(Init_Map_Thread(_isInitPlayerLocation));
        }


    }

    //清除过远区块
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

    //优化Chunk面数协程
    //本质上是把BaseChunk全部重新生成一遍

    public IEnumerator Chunk_Optimization()
    {

        foreach (var Chunk in world.Allchunks)
        {

            world.WaitToCreateMesh.Enqueue(Chunk.Value);

        }



        yield return new WaitForSeconds(1f);

    }


    //负责把接收的到的Chunk用多线程刷新
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


    //添加到等待添加队列
    public void AddtoCreateChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (world.Center_Now / TerrainData.ChunkWidth) + world.Center_direction * (world.renderSize - 1);

            //新增Chunk
            for (int i = -world.renderSize; i < world.renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));

                world.WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));


            }

            //呼叫里侧Chunk更新
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

            //呼叫里侧Chunk更新
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

            //呼叫里侧Chunk更新
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


            //呼叫里侧Chunk更新
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

        //Debug.Log("已经添加坐标");


        //判断是否启动协程
        //先两次添加再启动协程，后面数据多了一次启动协程
        if (world.WatingToCreate_Chunks.Count > 0 && world.CreateCoroutine == null)
        {

            world.CreateCoroutine = StartCoroutine(CreateChunksQueue());

        }


    }

    //协程：创造Chunk
    private IEnumerator CreateChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(world.CreateCoroutineDelay);


            //如果队列中有数据，就读取
            //如果队列中没有数据，就关闭协程
            if (world.WatingToCreate_Chunks.Count > 0)
            {

                //如果查到的chunk已经存在，则唤醒
                //不存在则生成
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

    //生成Chunk
    //BaseChunk不会进行自身剔除
    public void CreateBaseChunk(Vector3 pos)
    {

        //先判断一下有没有
        if (world.Allchunks.ContainsKey(pos))
        {
            world.Allchunks[pos].ShowChunk();
            return;
        }

        //调用Chunk
        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;


        //if (_ChunkLocation == new Vector3(195f,0,89f))
        //{
        //    print("");
        //}

        //调用Chunk
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

        //添加到字典
        world.Allchunks.Add(_ChunkLocation, _chunk_temp);

    }

    //非BaseChunk会进行Chunk面剔除
    void CreateChunk(Vector3 pos)
    {

        //先判断一下有没有
        if (world.Allchunks.ContainsKey(pos))
        {

            world.Allchunks[pos].ShowChunk();
            return;

        }


        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;

        //调用Chunk
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

        //添加到字典
        world.Allchunks.Add(pos, _chunk_temp);

    }

    //添加到等待删除队列
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

        //判断是否启动协程
        //先两次添加再启动协程，后面数据多了一次启动协程
        if (world.WatingToRemove_Chunks.Count > 0 && world.RemoveCoroutine == null)
        {

            world.RemoveCoroutine = StartCoroutine(RemoveChunksQueue());

        }

    }

    //协程：删除ChunK
    private IEnumerator RemoveChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(world.RemoveCoroutineDelay);


            //如果队列中有数据，就读取
            //如果队列中没有数据，就关闭协程
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

    #region 隐藏区块(什么狗屎代码)

    void Chunk_HideOrRemove(Vector3 chunklocation)
    {
        world.obj.HideChunk();
    }

    #endregion

    #region 渲染部分

    //渲染协程池
    public void RenderCoroutineManager()
    {
        // 如果等待渲染的队列不为空，并且没有正在运行的渲染协程
        if (world.WaitToRender.Count != 0 && world.Render_Coroutine == null)
        {
            //print($"启动渲染协程");
            world.Render_Coroutine = StartCoroutine(Render_0());
        }
    }

    // 一条渲染协程
    IEnumerator Render_0()
    {
        bool hasError = false;  // 标记是否发生异常

        while (true)
        {
            try
            {
                // 尝试从队列中取出要渲染的Chunk
                if (world.WaitToRender.TryDequeue(out Chunk chunktemp))
                {
                    //print($"{GetRelaChunkLocation(chunktemp.myposition)}开始渲染");

                    // 如果Chunk已经准备好渲染，调用CreateMesh
                    if (chunktemp.isReadyToRender)
                    {
                        chunktemp.CreateMesh();
                    }
                }

                // 如果队列为空，停止协程
                if (world.WaitToRender.Count == 0)
                {
                    //print($"队列为空，停止协程");
                    world.Render_Coroutine = null;
                    world.RenderLock = false;
                    break;
                }
            }
            catch (Exception ex)
            {
                // 捕获异常，防止协程因异常终止
                Debug.LogError($"渲染协程出错: {ex.Message}\n{ex.StackTrace}");

                hasError = true;  // 标记发生错误
                break;  // 退出当前循环，等待后重新启动
            }

            // 正常情况等待一段时间以控制渲染频率
            yield return new WaitForSeconds(world.RenderDelay);
        }

        // 如果发生了异常，等待并重启协程
        if (hasError)
        {
            world.Render_Coroutine = null;  // 重置协程状态
            yield return new WaitForSeconds(1f);  // 等待一段时间
            RenderCoroutineManager();  // 重新启动渲染协程
        }
    }

    //Mesh协程
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

                //print($"{GetRelaChunkLocation(chunktemp.myposition)}添加到meshQueue");

                //Mesh线程
                Thread myThread = new Thread(new ThreadStart(chunktemp.UpdateChunkMesh_WithSurround));
                myThread.Start();

                if (world.WaitToCreateMesh.Count == 0)
                {
                    if (hasExec_CaculateInitTime)
                    {
                        //print("渲染完了");
                        world.InitEndTime = Time.time;

                        //renderSize * renderSize * 4是总区块数，2是因为面剔除渲染了两次
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

    #region 返回Chunk对象

    //Vector3 --> 大区块对象
    public Chunk GetChunkObject(Vector3 pos)
    {

        world.Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk chunktemp);

        return chunktemp;

    }

    //New-获取区块对象
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

    #region 获取方块

    /// <summary>
    /// 返回方块类型,输入的是绝对坐标
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public byte GetBlockType(Vector3 pos)
    {
        //提前返回-找不到区块
        if (!world.Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk chunktemp))
            return 0;

        //提前返回-超过单个区块边界
        if (MC_Static_Chunk.isOutOfChunkRange(pos))
            return 0;

        //获取相对坐标
        Vector3 _vec = GetRelaPos(pos);

        //获取Block类型
        byte block_type = chunktemp.GetBlock((int)_vec.x, (int)_vec.y, (int)_vec.z).voxelType;

        //Return
        return block_type;

    }

    #endregion

    #region 结构方块(未应用)

    //用于记录建筑
    //recordData.pos存的是绝对坐标
    private List<EditStruct> recordData = new List<EditStruct>();

    //记录建筑
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

    //释放建筑
    //根据0下标和end下标，计算其包含的区块，然后把数据丢给区块即可
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

    #region 修改方块

    //外界修改方块
    public void EditBlock(Vector3 _pos, byte _target)
    {
        world.Allchunks[GetRelaChunkLocation(_pos)].EditData(_pos, _target);
    }

    public void EditBlock(List<EditStruct> _editStructs)
    {
        List<Vector3> _ChunkLocations = new List<Vector3>();

        // 遍历_editStructs并存储ChunkLocations
        foreach (var item in _editStructs)
        {

            // 如果allchunks里没有pos.则_ChunkLocations添加
            if (world.Allchunks.ContainsKey(GetRelaChunkLocation(item.editPos)))
            {
                if (!_ChunkLocations.Contains(GetRelaChunkLocation(item.editPos)))
                {
                    _ChunkLocations.Add(GetRelaChunkLocation(item.editPos));
                }


            }
            else
            {
                print($"区块不存在:{GetRelaChunkLocation(item.editPos)}");

            }


        }

        // 遍历_ChunkLocations，将allchunk里的_ChunkLocations执行EditData
        foreach (var chunkLocation in _ChunkLocations)
        {
            world.Allchunks[chunkLocation].EditData(_editStructs);
        }

        // 打印找到的区块数量
        //print($"找到{_ChunkLocations.Count}个");
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
            //print("执行EditBlocks");
            world.Allchunks[GetRelaChunkLocation(item.editPos)].EditData(item.editPos, item.targetType);

            yield return new WaitForSeconds(_time);
        }
        editBlockCoroutine = null;
    }

    #endregion

    #region 方块检测

    //对玩家碰撞盒的方块判断
    //true：有碰撞
    public bool CollisionCheckForVoxel(Vector3 pos)
    {

        Vector3 realLocation = pos; //绝对坐标
        Vector3 relaLocation = GetRelaPos(realLocation);
        byte targetBlock = GetBlockType(realLocation);

        //出界判断(Chunk)
        if (!world.Allchunks.ContainsKey(GetRelaChunkLocation(realLocation)))
            return true;

        //出界判断(Y)
        if (realLocation.y >= TerrainData.ChunkHeight || realLocation.y < 0)
            return false;

        //如果是自定义碰撞
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

        //返回固体还是空气
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

    //放置高亮方块的
    //用于眼睛射线的检测
    public bool RayCheckForVoxel(Vector3 pos)
    {

        Vector3 realLocation = pos; //绝对坐标
        Vector3 relaLocation = GetRelaPos(realLocation);
        byte targetBlock = GetBlockType(realLocation);

        if (!world.Allchunks.ContainsKey(GetRelaChunkLocation(pos))) { return false; }

        //计算相对坐标
        Vector3 vec = GetRelaPos(new Vector3(pos.x, pos.y, pos.z));

        //判断XOZ上有没有出界
        if (!world.Allchunks.ContainsKey(GetRelaChunkLocation(pos))) { return true; }

        //判断Y上有没有出界
        if (realLocation.y >= TerrainData.ChunkHeight) { return false; }


        //如果是自定义碰撞
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

        //返回固体还是空气
        return world.blocktypes[world.Allchunks[GetRelaChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z].voxelType].canBeChoose;

    }


    #endregion

    #region 指定方向寻址

    //指定方向寻址
    public Vector3 AddressingBlock(Vector3 _start, int _direct)
    {
        Vector3 _address = _start;
        //print($"start: {_address}");

        for (int i = 0; i < TerrainData.ChunkHeight; i++)
        {
            byte _byte = GetBlockType(_address);
            if (_byte != VoxelData.Air)
            {
                //print($"坐标：{_address} , 碰到{_byte}");
                //添加一个方块踮脚
                if (_byte == VoxelData.Water)
                {
                    EditBlock(_address, VoxelData.Grass);
                }

                //Offset
                return _address + new Vector3(0.5f, 2f, 0.5f);
            }

            _address += VoxelData.faceChecks[_direct];
        }

        print("寻址失败");
        return _start;

    }

    //给定一个初始坐标和初始方向，朝着这个方向遍历ChunkHeight，返回一个非空气坐标
    public Vector3 LoopAndFindABestLocation(Vector3 _start, Vector3 _direct)
    {
        _direct.x = _direct.x > 0 ? 1 : 0;
        _direct.y = _direct.y > 0 ? 1 : 0;
        _direct.z = _direct.z > 0 ? 1 : 0;

        Vector3 _next = _start;

        //Loop
        for (int i = 0; i < TerrainData.ChunkHeight; i++)
        {
            // Check，如果当前位置的方块类型不是空气，返回该坐标
            if (GetBlockType(_next) != VoxelData.Air)
            {
                return _next;
            }

            // 累积移动位置
            _next += _direct; // 使用归一化的方向向量逐步移动
        }

        return _start;
    }

    #endregion

    #region 返回可用出生点

    public void GetSpawnPos(Vector3 _pos, out List<Vector3> _Spawns)
    {
        _Spawns = new List<Vector3>();
        Vector3 _ChunkLocation = GetRelaChunkLocation(_pos);

        //提前返回-没有区块
        if (!world.Allchunks.TryGetValue(_ChunkLocation, out Chunk _chunktemp))
            return;

        _chunktemp.GetSpawnPos(GetRelaPos(_pos), out List<Vector3> __Spawns);
        _Spawns = new List<Vector3>(__Spawns);
    }

    #endregion

}
