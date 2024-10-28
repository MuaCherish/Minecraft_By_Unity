using System.Collections;
using UnityEngine;
using MCEntity;
using Homebrew;

namespace MCEntity
{
    [RequireComponent(typeof(MC_Velocity_Component))]
    public class MC_Collider_Component : MonoBehaviour
    {

        #region ״̬
        [Foldout("״̬", true)]
        [ReadOnly] public bool isGround = false;

        void ReferUpdate()
        {
            isGround = collider_Down;
        }


        #endregion



        #region �������ں���

        MC_Velocity_Component Velocity_Component;
        ManagerHub managerhub;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();

            if (managerhub == null)
            {
                managerhub = GlobalData.GetManagerhub();
            }
        }

        private void Update()
        {

            ReferUpdate();
            ReferUpdateHitBox();
        }




        #endregion


        #region �ж���

        // �ж�������
        [Foldout("�ж���", true)]
        [Header("�����ж���")] public bool isDrawHitBox; private bool hasExec_isDrawHitBox = true;
        [Header("�ж����۾���Ը߶�")] public float hitBoxEyes = 0.8f;
        [Header("�ж�����")] public float hitBoxWidth = 1f;
        [Header("�ж���߶�")] public float hitBoxHeight = 1f;
        private GameObject lineObject;
        private Mesh mesh;

        // ǰ����ײ���
        public bool collider_Front
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, 0f, 0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, 0f, 0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, 0f, 0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, 0f, 0.01f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(0f, 0f, 1f));
            }
        }

        // ����ײ���
        public bool collider_Back
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, 0f, -0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, 0f, -0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, 0f, -0.01f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, 0f, -0.01f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(0f, 0f, -1f));
            }
        }

        // ����ײ���
        public bool collider_Left
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(-0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(-0.01f, 0f, 0f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(-1f, 0f, 0f));
            }
        }

        // �ҷ���ײ���
        public bool collider_Right
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0.01f, 0f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0.01f, 0f, 0f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(1f, 0f, 0f));
            }
        }

        // �Ϸ���ײ���
        public bool collider_Up
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, 0.01f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, 0.01f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, 0.01f, 0f)) ||
                       managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, 0.01f, 0f)) ||
                       TryCheckMore_SubdivisionCollition(new Vector3(0f, 1f, 0f));
            }
        }

        // �·���ײ���
        public bool collider_Down
        {
            get
            {
                if (isCollisionLocked) return false;

                return managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, -0.01f, 0f)) ||
                    managerhub.world.CollisionCheckForVoxel(ǰ_���� + new Vector3(0f, -0.01f, 0f)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, -0.01f, 0f)) ||
                    managerhub.world.CollisionCheckForVoxel(��_���� + new Vector3(0f, -0.01f, 0f)) ||
                    TryCheckMore_SubdivisionCollition(new Vector3(0f, -1f, 0f));

            }
        }
        public bool Collider_Surround
        {
            get
            {
                return !isCollisionLocked && (collider_Front || collider_Back || collider_Left || collider_Right);
            }
        }




        /// <summary>
        /// �ж��õ��Ƿ����ж�����
        /// </summary>
        public bool CheckHitBox(Vector3 _targetPos)
        {
            Vector3 _selfPos = selfPos;
            float selfHalfWidth = hitBoxWidth / 2f;
            float selfHalfHeight = hitBoxHeight / 2f;

            // �ж�Ŀ����Ƿ��ڵ�ǰ�ж�����
            bool isInside =
                (_targetPos.x >= _selfPos.x - selfHalfWidth && _targetPos.x <= _selfPos.x + selfHalfWidth) &&
                (_targetPos.y >= _selfPos.y - selfHalfHeight && _targetPos.y <= _selfPos.y + selfHalfHeight) &&
                (_targetPos.z >= _selfPos.z - selfHalfWidth && _targetPos.z <= _selfPos.z + selfHalfWidth); // �����Ҫ����z��

            return isInside;
        }


        /// <summary>
        /// �ж����������Ƿ��ص�
        /// </summary>
        public Vector3 CheckHitBox(Vector3 _targetPos, float _targetWidth, float _targetHeight)
        {
            Vector3 _selfPos = selfPos;
            float _selfWidth = hitBoxWidth;
            float _selfHeight = hitBoxHeight;

            // ������Ͱ��
            float selfHalfWidth = _selfWidth / 2f;
            float selfHalfHeight = _selfHeight / 2f;
            float targetHalfWidth = _targetWidth / 2f;
            float targetHalfHeight = _targetHeight / 2f;

            // �ж��Ƿ��ص�
            bool isCollision =
                Mathf.Abs(_selfPos.x - _targetPos.x) < (selfHalfWidth + targetHalfWidth) &&
                Mathf.Abs(_selfPos.y - _targetPos.y) < (selfHalfHeight + targetHalfHeight) &&
                Mathf.Abs(_selfPos.z - _targetPos.z) < (selfHalfWidth + targetHalfWidth); // �����Ҫ����z��

            // ���������ײ��������ײ����
            if (isCollision)
            {
                return (_targetPos - _selfPos).normalized;
            }
            else
            {
                return Vector3.zero; // û����ײ
            }
        }


        //����Update
        private void ReferUpdateHitBox()
        {
            //λ������
            if (collider_Down)
            {
                // �����µ� y λ�ã����������������
                float newYPosition = (int)selfPos.y + hitBoxHeight / 2f;
                // ����һ���µ�λ��
                Vector3 newPosition = new Vector3(transform.position.x, newYPosition, transform.position.z);

                // ���������λ��
                transform.position = newPosition;
            }


            //�����ж���
            if (isDrawHitBox)
            {
                DrawHitBox();
                
            }
            else
            {
                if (lineObject != null)
                {
                    Destroy(lineObject);
                    lineObject = null;
                }

                if (hasExec_isDrawHitBox == false)
                {
                    hasExec_isDrawHitBox = true;
                }
            }

        }

        //�����ж���
        private void DrawHitBox()
        {

            if (hasExec_isDrawHitBox)
            {

                // ����һ���µ� GameObject ���ڻ����߿�
                lineObject = new GameObject("HitBoxLine");
                lineObject.transform.parent = transform; // �趨������Ϊ��ǰ����
                lineObject.transform.position = transform.position;

                // ���� Mesh ���
                mesh = new Mesh();
                MeshFilter meshFilter = lineObject.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                // ��� MeshRenderer ���
                MeshRenderer meshRenderer = lineObject.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Unlit/Color")) { color = Color.white }; // ʹ���޹��ղ���

                Vector3 center = lineObject.transform.localPosition;

                // �����ж���İ˸���
                Vector3[] positions = new Vector3[12];

                // �����ĸ���
                positions[0] = center + new Vector3(-hitBoxWidth / 2 - 0.01f, -hitBoxHeight / 2,  hitBoxWidth / 2 + 0.01f);  // ǰ��
                positions[1] = center + new Vector3( hitBoxWidth / 2 + 0.01f, -hitBoxHeight / 2,  hitBoxWidth / 2 + 0.01f);   // ǰ��
                positions[2] = center + new Vector3( hitBoxWidth / 2 + 0.01f, -hitBoxHeight / 2, -hitBoxWidth / 2 - 0.01f);  // ����
                positions[3] = center + new Vector3(-hitBoxWidth / 2 - 0.01f, -hitBoxHeight / 2, -hitBoxWidth / 2 - 0.01f); // ����

                // �����ĸ���
                positions[4] = positions[0] + Vector3.up * hitBoxHeight; // ǰ����
                positions[5] = positions[1] + Vector3.up * hitBoxHeight; // ǰ����
                positions[6] = positions[2] + Vector3.up * hitBoxHeight; // ������
                positions[7] = positions[3] + Vector3.up * hitBoxHeight; // ������

                // �۾�
                positions[8] = positions[0] + Vector3.up * hitBoxHeight * hitBoxEyes; // ǰ����
                positions[9] = positions[1] + Vector3.up * hitBoxHeight * hitBoxEyes; // ǰ����
                positions[10] = positions[2] + Vector3.up * hitBoxHeight * hitBoxEyes; // ������
                positions[11] = positions[3] + Vector3.up * hitBoxHeight * hitBoxEyes; // ������

                // �����߶�����
                int[] indices = new int[]
                {
                    // ����
                    0, 1,
                    1, 2,
                    2, 3,
                    3, 0,
                    // ����
                    4, 5,
                    5, 6,
                    6, 7,
                    7, 4,
                    // ����
                    0, 4,
                    1, 5,
                    2, 6,
                    3, 7,
                    // ����
                    8, 9,
                    9, 10,
                    10, 11,
                    11, 8
                };

                // ���� Mesh ����
                mesh.Clear();
                mesh.vertices = positions;
                mesh.SetIndices(indices, MeshTopology.Lines, 0);
                mesh.RecalculateBounds();

                hasExec_isDrawHitBox = false;
            }

        }


        #endregion


        #region ��ȡ��

        //��ײϸ��
        //����ָ������ĵ�ķ���ĳ���
        //�ж��������Ƿ��㹻
        //���򽫽�����ײϸ��
        bool TryCheckMore_SubdivisionCollition(Vector3 _Direct)
        {
            return false;
        }

        /// <summary>
        /// ��ȡ����Block��Ŀ�귽���
        /// </summary>
        /// <param name="_direct">��������</param>
        /// <returns>Ŀ���</returns>
        public Vector3 GetBlockDirectPoint(Vector3 _direct)
        {
            // ȷ����������Ϊ��λ����
            _direct.Normalize();

            // ���ݷ��򷵻���Ӧ��Ŀ���
            if (_direct == Vector3.forward) // Front
            {
                return selfPos + Vector3.forward;
            }
            else if (_direct == Vector3.back) // Back
            {
                return selfPos + Vector3.back;
            }
            else if (_direct == Vector3.left) // Left
            {
                return selfPos + Vector3.left;
            }
            else if (_direct == Vector3.right) // Right
            {
                return selfPos + Vector3.right;
            }
            else if (_direct == Vector3.up) // Up
            {
                return selfPos + Vector3.up;
            }
            else if (_direct == Vector3.down) // Down
            {
                return selfPos + Vector3.down;
            }
            else
            {
                Debug.LogError("GetDirectionPoint���벻��ȷ");
                return Vector3.zero; // ������������ʾ����
            }
        }


        //��������
        public Vector3 selfPos
        {
            get { return transform.position; }
        }

        public Vector3 EyesPoint
        {
            get
            {
                return transform.position + Vector3.up * hitBoxHeight * hitBoxEyes;
            }
        }

        //���µ�
        public Vector3 FootPoint
        {
            get
            {
                return selfPos - Vector3.down * hitBoxHeight - new Vector3(0f, 0.01f, 0f);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, hitBoxWidth / 2);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, hitBoxWidth / 2);
            }
        }

        // ǰ����
        public Vector3 ǰ_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, hitBoxWidth / 2);
            }
        }

        // ������ 
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
            }
        }

        // ������
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2, -hitBoxWidth / 2);
            }
        }

        // ������
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(-hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, -hitBoxWidth / 2);
            }
        }

        // ������
        public Vector3 ��_����
        {
            get
            {
                return selfPos + new Vector3(hitBoxWidth / 2, -hitBoxHeight / 2 + hitBoxHeight, -hitBoxWidth / 2);
            }
        }


        #endregion



        #region ���⹤��


        private bool isCollisionLocked = false;
        private float collisionLockTimer = 0f;

        // ��ʱ�ر���ײ���
        //isGround����
        public void CloseCollisionForAWhile(float _time)
        {
            isCollisionLocked = true;
            collisionLockTimer = _time;
            StartCoroutine(CollisionLockTimerCoroutine());
        }

        // ��ײ����Э�̣����ڽ���
        private IEnumerator CollisionLockTimerCoroutine()
        {
            while (collisionLockTimer > 0f)
            {
                collisionLockTimer -= Time.deltaTime;
                yield return null;
            }
            isCollisionLocked = false;
        }

        #endregion


    }
}