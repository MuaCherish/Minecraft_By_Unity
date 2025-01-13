using Homebrew;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cloud
{
    public class CloudManager : MonoBehaviour
    {


        #region 周期函数


        private ManagerHub managerhub;
        private bool isCloudInitialized = false; // 标记云的位置是否已初始化

        private void Start()
        {


            managerhub = GlobalData.GetManagerhub();

            // 在XOZ平面上随机生成一个单位向量作为初始方向
            WindDirect = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        
        
        }

        private void Update()
        {

            switch (managerhub.world.game_state)
            {
                case Game_State.Start:
                    if (!isCloudInitialized)
                    {
                        InitCloud_StartPos();
                        isCloudInitialized = true; // 确保只初始化一次
                    }
                    break;

                case Game_State.Playing:
                    Handle_GameState_Playing();
                    CloudMoving();
                    isCloudInitialized = false; // 切换到其他状态时重置标志
                    break;

                default:
                    isCloudInitialized = false; // 处理非Playing和Start的状态
                    break;
            }
        }


        void Handle_GameState_Start()
        {
            
        }


        void Handle_GameState_Playing()
        {

        }



        #endregion


        #region 大块云

        [Foldout("云的引用")]
        [Header("云")] public Sprite CloudSprite; // 256x256 的Sprite
        [Header("位置")] public GameObject parent;
        [Header("云的颜色")] public Material[] cloudMaterials = new Material[2];

        [Foldout("云的设置")]
        [Header("风向")][ReadOnly] public Vector3 WindDirect;
        [Header("风速")] public float windSpeed = 1.0f;


        


        void InitCloud_StartPos()
        {
            parent.transform.position = new Vector3(managerhub.player.transform.position.x, 126.5f, managerhub.player.transform.position.z);
        }


        void CloudMoving()
        {
            // 按照速度和方向移动parent
            parent.transform.position += WindDirect * windSpeed * Time.deltaTime;

        }


        


        //设置颜色
        public void SetCloudColor(Color _color)
        {
            foreach (var item in cloudMaterials)
            {
                item.color = _color;
            }
        }


        public void SetCloudSpeed()
        {

        }


        #endregion


        #region 区块云（弃案）

        // 随机获取裁剪的云块并生成对应的GameObject
        //for (int x = -3; x <= 3; x++)
        //{
        //    for (int z = -3; z <= 3; z++)
        //    {
        //        GenerateCloud(new Vector3(x, 0, z));
        //    }
        //}
        //GenerateCloud(new Vector3(0, 0, 0));
        //GenerateCloud(new Vector3(1, 0, 0));

        //[Header("云的渲染半径")] public float RenderSize = 4f;
        //[Foldout("Sprite处理")]
        //[Header("裁切大小")] public Vector2Int cutSize = new Vector2Int(16, 16); // 裁切尺寸（16x16）

        //public Dictionary<Vector3, GameObject> AllClouds = new Dictionary<Vector3, GameObject>();

        ////由player调用
        //public void UpdateClouds()
        //{

        //    //Remove
        //    foreach (var item in AllClouds)
        //    {
        //        if ((item.Value.transform.position - managerhub.player.transform.position).magnitude > 160f)
        //        {
        //            AllClouds.Remove(item.Key);
        //        }
        //    }
        //}

        //// 随机裁剪一部分云块并返回一个新的Sprite
        //Sprite GetRandomCutCloudSprite()
        //{
        //    Texture2D texture = CloudSprite.texture;

        //    // 计算最大可选位置，确保不越界
        //    int maxX = texture.width - cutSize.x;
        //    int maxY = texture.height - cutSize.y;

        //    // 随机选择裁剪的起始位置
        //    int startX = Random.Range(0, maxX);
        //    int startY = Random.Range(0, maxY);

        //    // 裁剪区域
        //    Rect rect = new Rect(startX, startY, cutSize.x, cutSize.y);

        //    // 创建一个新的Sprite，使用裁剪的纹理区域
        //    Sprite cutSprite = Sprite.Create(
        //        texture,
        //        rect,
        //        new Vector2(0.5f, 0.5f),  // 使Sprite居中
        //        100f                      // 像素单位比例
        //    );

        //    return cutSprite;
        //}

        //// 生成一个带有SpriteRenderer的GameObject
        //void GenerateCloud(Vector3 _pos)
        //{

        //    if (!AllClouds.ContainsKey(_pos))
        //    {
        //        // 创建新的GameObject
        //        GameObject cloudObject = new GameObject($"Cloud_{_pos}");
        //        cloudObject.transform.SetParent(parent.transform);

        //        // 给GameObject添加SpriteRenderer组件，并设置Sprite
        //        SpriteRenderer spriteRenderer = cloudObject.AddComponent<SpriteRenderer>();
        //        spriteRenderer.sprite = GetRandomCutCloudSprite();

        //        // 设置位置
        //        cloudObject.transform.position = new Vector3(_pos.x * 160f, 128f, _pos.z * 160f);
        //        cloudObject.transform.rotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
        //        cloudObject.transform.localScale = new Vector3(1000f, 1000f, 1000f);

        //        AllClouds.Add(_pos, cloudObject);
        //    }



        //}


        #endregion


    }

}



