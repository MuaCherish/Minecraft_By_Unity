using Homebrew;
using System.Collections.Generic;
using UnityEngine;

namespace Cloud
{
    public class CloudManager : MonoBehaviour
    {
        [Foldout("�Ƶ�����")]
        [Header("��")] public Sprite CloudSprite; // 256x256 ��Sprite
        [Header("λ��")] public GameObject parent;

        [Foldout("Sprite����")]
        [Header("���д�С")] public Vector2Int cutSize = new Vector2Int(16, 16); // ���гߴ磨16x16��

        [Foldout("�Ƶ�����")]
        [Header("�Ƶ���Ⱦ�뾶")] public float RenderSize = 4f;
        [Header("����")] public Vector3 WindDirect;

        public Dictionary<Vector3, GameObject> AllClouds = new Dictionary<Vector3, GameObject>();

        private ManagerHub managerhub;
        bool hasExec_Update = true;
        private void Start()
        {


            managerhub = GlobalData.GetManagerhub();
            // �����ȡ�ü����ƿ鲢���ɶ�Ӧ��GameObject
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


        //��player����
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


        //������ɫ
        public void SetCloudColor(Color _color)
        {
            foreach (Transform item in parent.transform)
            {
                item.gameObject.GetComponent<SpriteRenderer>().color = _color;
            }
        }


        // ����ü�һ�����ƿ鲢����һ���µ�Sprite
        Sprite GetRandomCutCloudSprite()
        {
            Texture2D texture = CloudSprite.texture;

            // ��������ѡλ�ã�ȷ����Խ��
            int maxX = texture.width - cutSize.x;
            int maxY = texture.height - cutSize.y;

            // ���ѡ��ü�����ʼλ��
            int startX = Random.Range(0, maxX);
            int startY = Random.Range(0, maxY);

            // �ü�����
            Rect rect = new Rect(startX, startY, cutSize.x, cutSize.y);

            // ����һ���µ�Sprite��ʹ�òü�����������
            Sprite cutSprite = Sprite.Create(
                texture,
                rect,
                new Vector2(0.5f, 0.5f),  // ʹSprite����
                100f                      // ���ص�λ����
            );

            return cutSprite;
        }

        // ����һ������SpriteRenderer��GameObject
        void GenerateCloud(Vector3 _pos)
        {

            if (!AllClouds.ContainsKey(_pos))
            {
                // �����µ�GameObject
                GameObject cloudObject = new GameObject($"Cloud_{_pos}");
                cloudObject.transform.SetParent(parent.transform);

                // ��GameObject���SpriteRenderer�����������Sprite
                SpriteRenderer spriteRenderer = cloudObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = GetRandomCutCloudSprite();

                // ����λ��
                cloudObject.transform.position = new Vector3(_pos.x * 160f, 128f, _pos.z * 160f);
                cloudObject.transform.rotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
                cloudObject.transform.localScale = new Vector3(1000f, 1000f, 1000f);

                AllClouds.Add(_pos, cloudObject);
            }

           

        }
    }

}



