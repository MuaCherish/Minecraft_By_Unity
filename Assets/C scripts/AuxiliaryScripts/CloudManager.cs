using Homebrew;
using System.Collections.Generic;
using UnityEngine;

namespace Cloud
{
    public class CloudManager : MonoBehaviour
    {
        [Foldout("云的引用")]
        [Header("云")] public Sprite CloudSprite; // 256x256 的Sprite
        [Header("位置")] public GameObject parent;

        [Foldout("Sprite处理")]
        [Header("裁切大小")] public Vector2Int cutSize = new Vector2Int(16, 16); // 裁切尺寸（16x16）

        [Foldout("云的设置")]
        [Header("云的渲染半径")] public float RenderSize = 4f;
        [Header("风向")] public Vector3 WindDirect;

        public Dictionary<Vector3, GameObject> AllClouds = new Dictionary<Vector3, GameObject>();

        private ManagerHub managerhub;
        bool hasExec_Update = true;
        private void Start()
        {


            managerhub = GlobalData.GetManagerhub();
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
        }

        private void Update()
        {
            if (managerhub.world.game_state == Game_State.Playing)
            {
                if (hasExec_Update)
                {
                    parent.transform.position = new Vector3(managerhub.player.transform.position.x, 128f, managerhub.player.transform.position.z) ;
                    hasExec_Update = false;
                }


            }
            else
            {
                if (hasExec_Update == false)
                {
                    hasExec_Update = true;
                }
            }
        }


        //由player调用
        public void UpdateClouds()
        {

            //Remove
            foreach (var item in AllClouds)
            {
                if ((item.Value.transform.position - managerhub.player.transform.position).magnitude > 160f)
                {
                    AllClouds.Remove(item.Key);
                }
            }
        }


        //设置颜色
        public void SetCloudColor(Color _color)
        {
            foreach (Transform item in parent.transform)
            {
                item.gameObject.GetComponent<SpriteRenderer>().color = _color;
            }
        }


        // 随机裁剪一部分云块并返回一个新的Sprite
        Sprite GetRandomCutCloudSprite()
        {
            Texture2D texture = CloudSprite.texture;

            // 计算最大可选位置，确保不越界
            int maxX = texture.width - cutSize.x;
            int maxY = texture.height - cutSize.y;

            // 随机选择裁剪的起始位置
            int startX = Random.Range(0, maxX);
            int startY = Random.Range(0, maxY);

            // 裁剪区域
            Rect rect = new Rect(startX, startY, cutSize.x, cutSize.y);

            // 创建一个新的Sprite，使用裁剪的纹理区域
            Sprite cutSprite = Sprite.Create(
                texture,
                rect,
                new Vector2(0.5f, 0.5f),  // 使Sprite居中
                100f                      // 像素单位比例
            );

            return cutSprite;
        }

        // 生成一个带有SpriteRenderer的GameObject
        void GenerateCloud(Vector3 _pos)
        {

            if (!AllClouds.ContainsKey(_pos))
            {
                // 创建新的GameObject
                GameObject cloudObject = new GameObject($"Cloud_{_pos}");
                cloudObject.transform.SetParent(parent.transform);

                // 给GameObject添加SpriteRenderer组件，并设置Sprite
                SpriteRenderer spriteRenderer = cloudObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = GetRandomCutCloudSprite();

                // 设置位置
                cloudObject.transform.position = new Vector3(_pos.x * 160f, 128f, _pos.z * 160f);
                cloudObject.transform.rotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
                cloudObject.transform.localScale = new Vector3(1000f, 1000f, 1000f);

                AllClouds.Add(_pos, cloudObject);
            }

           

        }
    }

}



