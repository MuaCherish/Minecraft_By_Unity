//using System.Collections;
//using System.Collections.Generic;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif
//using UnityEngine;

//public class GoooosTest: MonoBehaviour
//{
//    public DataBase[] a;

//    [Header("�������")]
//    public string goodName;
//    public Sprite icon; //��Ʒ��ͼ��
//    public BlockClassfy classify = BlockClassfy.����;


//    [Header("classify = �������飬�����Է���")]
//    public float DestroyTime;
//    public bool isSolid;        //�Ƿ���赲���
//    public bool isTransparent;  //�ܱ߷����Ƿ����޳�
//    public bool canBeChoose;    //�Ƿ�ɱ��������鲶׽��
//    public bool candropBlock;   //�Ƿ���䷽��
//    public bool isDIYCollision;//������˵���Ƿ������ڼ�ѹ����ֵ
//    public CollosionRange CollosionRange; //����Y��˵��(0.5f,0,0f)������Y������������ڼ�ѹ0.5f��Y������������ڼ�ѹ0.0f����̨�׵���ײ����

//    [Header("classify = �����Է���")]
//    public bool isinteractable; //�Ƿ�ɱ��Ҽ����� 
//    public bool IsOriented;     //�Ƿ������ҳ��� 

//    [Header("classify = ���ߣ� ʳ��")]
//    public bool hasDiyRotation;//�Զ�����ת
//    public Vector3 DiyRotation;

//    [Header("classify = ����")]
//    public bool canBreakBlockWithMouse1;  // ������ƻ�����
//    public bool hasMouse2Action;          // �Ҽ�������������÷��飩
//    public bool hasMouse2HoldAction;      // �����Ҽ��Ĺ��ܣ������������÷��飩

//    [Header("classify = ʳ��")]
//    public int healthRecoveryAmount;      // �ɻָ���Ѫ��ֵ


//    [Header("classify = �������飬�����Է���")]
//    public bool is2d;           //����������ʾ
//    public Sprite front_sprite; //������
//    public Sprite surround_sprite; //������
//    public Sprite top_sprit; //������
//    public Sprite buttom_sprit; //������


//    [Header("classify = �������飬�����Է���")]
//    public AudioClip[] walk_clips = new AudioClip[2];
//    public AudioClip broking_clip;
//    public AudioClip broken_clip;


//    [Header("classify = �������飬�����Է���")]
//    public int backFaceTexture = 0;
//    public int frontFaceTexture = 0;
//    public int topFaceTexture = 0;
//    public int bottomFaceTexture = 0;
//    public int leftFaceTexture = 0;
//    public int rightFaceTexture = 0;
//    public DrawMode DrawMode = DrawMode.Block;

//    [Header("classify = �������飬�����Է���")]
//    public bool GenerateTwoFaceWithAir;    //��������������˫�����
//    public List<FaceCheckMode> OtherFaceCheck;

//    //��ͼ�е��������
//    public int GetTextureID(int faceIndex)
//    {

//        switch (faceIndex)
//        {

//            case 0:
//                return backFaceTexture;

//            case 1:
//                return frontFaceTexture;

//            case 2:
//                return topFaceTexture;

//            case 3:
//                return bottomFaceTexture;

//            case 4:
//                return leftFaceTexture;

//            case 5:
//                return rightFaceTexture;

//            default:
//                Debug.Log($"Error in GetTextureID; invalid face index {faceIndex}");
//                return 0;


//        }

//    }
//}

//namespace InspectEditor
//{
//    [CustomEditor(typeof(GoooosTest))]
//    public class GoooosTestEditor : Editor
//    {
//        #region ���л�����
//        private SerializedProperty classify;
//        private SerializedProperty goodName;
//        private SerializedProperty icon;

//        // ��������͹����Է�������
//        private SerializedProperty destroyTime;
//        private SerializedProperty isSolid;
//        private SerializedProperty isTransparent;
//        private SerializedProperty canBeChoose;
//        private SerializedProperty candropBlock;
//        private SerializedProperty isDIYCollision;
//        private SerializedProperty collosionRange;

//        // �����Է�������
//        private SerializedProperty isInteractable;
//        private SerializedProperty isOriented;

//        // ���ߺ�ʳ������
//        private SerializedProperty hasDiyRotation;
//        private SerializedProperty diyRotation;
//        private SerializedProperty canBreakBlockWithMouse1;
//        private SerializedProperty hasMouse2Action;
//        private SerializedProperty hasMouse2HoldAction;
//        private SerializedProperty healthRecoveryAmount;

//        // ������������
//        private SerializedProperty is2D;
//        private SerializedProperty frontSprite;
//        private SerializedProperty surroundSprite;
//        private SerializedProperty topSprite;
//        private SerializedProperty bottomSprite;
//        private SerializedProperty walkClips;
//        private SerializedProperty brokingClip;
//        private SerializedProperty brokenClip;
//        private SerializedProperty backFaceTexture;
//        private SerializedProperty frontFaceTexture;
//        private SerializedProperty topFaceTexture;
//        private SerializedProperty bottomFaceTexture;
//        private SerializedProperty leftFaceTexture;
//        private SerializedProperty rightFaceTexture;
//        private SerializedProperty drawMode;
//        private SerializedProperty generateTwoFaceWithAir;
//        private SerializedProperty otherFaceCheck;
//        #endregion

//        private void OnEnable()
//        {
//            // ��ʼ��������Ҫ��̬��ʾ������
//            classify = serializedObject.FindProperty("classify");
//            goodName = serializedObject.FindProperty("goodName");
//            icon = serializedObject.FindProperty("icon");

//            // ��������͹����Է��������
//            destroyTime = serializedObject.FindProperty("DestroyTime");
//            isSolid = serializedObject.FindProperty("isSolid");
//            isTransparent = serializedObject.FindProperty("isTransparent");
//            canBeChoose = serializedObject.FindProperty("canBeChoose");
//            candropBlock = serializedObject.FindProperty("candropBlock");
//            isDIYCollision = serializedObject.FindProperty("isDIYCollision");
//            collosionRange = serializedObject.FindProperty("collosionRange");

//            // �����Է��������
//            isInteractable = serializedObject.FindProperty("isInteractable");
//            isOriented = serializedObject.FindProperty("isOriented");

//            // ���ߺ�ʳ�������
//            hasDiyRotation = serializedObject.FindProperty("hasDiyRotation");
//            diyRotation = serializedObject.FindProperty("diyRotation");
//            canBreakBlockWithMouse1 = serializedObject.FindProperty("canBreakBlockWithMouse1");
//            hasMouse2Action = serializedObject.FindProperty("hasMouse2Action");
//            hasMouse2HoldAction = serializedObject.FindProperty("hasMouse2HoldAction");
//            healthRecoveryAmount = serializedObject.FindProperty("healthRecoveryAmount");

//            // �������������
//            is2D = serializedObject.FindProperty("is2D");
//            frontSprite = serializedObject.FindProperty("front_sprite");
//            surroundSprite = serializedObject.FindProperty("surround_sprite");
//            topSprite = serializedObject.FindProperty("top_sprit");
//            bottomSprite = serializedObject.FindProperty("buttom_sprit");
//            walkClips = serializedObject.FindProperty("walk_clips");
//            brokingClip = serializedObject.FindProperty("broking_clip");
//            brokenClip = serializedObject.FindProperty("broken_clip");
//            backFaceTexture = serializedObject.FindProperty("backFaceTexture");
//            frontFaceTexture = serializedObject.FindProperty("frontFaceTexture");
//            topFaceTexture = serializedObject.FindProperty("topFaceTexture");
//            bottomFaceTexture = serializedObject.FindProperty("bottomFaceTexture");
//            leftFaceTexture = serializedObject.FindProperty("leftFaceTexture");
//            rightFaceTexture = serializedObject.FindProperty("rightFaceTexture");
//            drawMode = serializedObject.FindProperty("DrawMode");
//            generateTwoFaceWithAir = serializedObject.FindProperty("GenerateTwoFaceWithAir");
//            otherFaceCheck = serializedObject.FindProperty("OtherFaceCheck");

//            // ���������Ϣ��������л������Ƿ���ȷ
//            //Debug.Log($"Good Name: {goodName != null}");
//            //Debug.Log($"Icon: {icon != null}");
//            //Debug.Log($"Classify: {classify != null}");

//            // ���������������
//            CheckSerializedProperties();
//        }

//        private void CheckSerializedProperties()
//        {
//            var properties = new[]
//            {
//                destroyTime, isSolid, isTransparent, canBeChoose,
//                candropBlock, isDIYCollision, collosionRange,
//                isInteractable, isOriented,
//                hasDiyRotation, diyRotation, canBreakBlockWithMouse1,
//                hasMouse2Action, hasMouse2HoldAction, healthRecoveryAmount,
//                is2D, frontSprite, surroundSprite, topSprite,
//                bottomSprite, walkClips, brokingClip, brokenClip,
//                backFaceTexture, frontFaceTexture, topFaceTexture,
//                bottomFaceTexture, leftFaceTexture, rightFaceTexture,
//                drawMode, generateTwoFaceWithAir, otherFaceCheck
//            };

//            //foreach (var property in properties)
//            //{
//            //    Debug.Log($"Property {property?.name}: {(property != null ? "Initialized" : "Not Initialized")}");
//            //}
//        }

//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();

//            // ��ʾ��������
//            if (goodName != null) EditorGUILayout.PropertyField(goodName);
//            if (icon != null) EditorGUILayout.PropertyField(icon);
//            if (classify != null) EditorGUILayout.PropertyField(classify);

//            // ���� classify ��ֵ����ʾ����������  
//            if (classify != null)
//            {
//                switch ((BlockClassfy)classify.enumValueIndex)
//                {
//                    case BlockClassfy.��������:
//                        ShowBuildingBlockProperties();
//                        break;

//                    case BlockClassfy.�����Է���:
//                        ShowFunctionalBlockProperties();
//                        break;

//                    case BlockClassfy.����:
//                        ShowToolProperties();
//                        break;

//                    case BlockClassfy.ʳ��:
//                        ShowFoodProperties();
//                        break;
//                }

//                // ��ʾ��������Ķ�������
//                if (classify.enumValueIndex == (int)BlockClassfy.�������� || classify.enumValueIndex == (int)BlockClassfy.�����Է���)
//                {
//                    ShowBuildingExtraProperties();
//                }
//            }

//            serializedObject.ApplyModifiedProperties();
//        }

//        private void ShowBuildingBlockProperties()
//        {
//            if (destroyTime != null) EditorGUILayout.PropertyField(destroyTime);
//            if (isSolid != null) EditorGUILayout.PropertyField(isSolid);
//            if (isTransparent != null) EditorGUILayout.PropertyField(isTransparent);
//            if (canBeChoose != null) EditorGUILayout.PropertyField(canBeChoose);
//            if (candropBlock != null) EditorGUILayout.PropertyField(candropBlock);
//            if (isDIYCollision != null) EditorGUILayout.PropertyField(isDIYCollision);
//            if (collosionRange != null) EditorGUILayout.PropertyField(collosionRange);
//        }

//        private void ShowFunctionalBlockProperties()
//        {
//            ShowBuildingBlockProperties(); // ��ʾ�������������
//            if (isInteractable != null) EditorGUILayout.PropertyField(isInteractable);
//            if (isOriented != null) EditorGUILayout.PropertyField(isOriented);
//        }

//        private void ShowToolProperties()
//        {
//            if (hasDiyRotation != null) EditorGUILayout.PropertyField(hasDiyRotation);
//            if (hasDiyRotation.boolValue && diyRotation != null)
//            {
//                EditorGUILayout.PropertyField(diyRotation);
//            }
//            if (canBreakBlockWithMouse1 != null) EditorGUILayout.PropertyField(canBreakBlockWithMouse1);
//            if (hasMouse2Action != null) EditorGUILayout.PropertyField(hasMouse2Action);
//            if (hasMouse2HoldAction != null) EditorGUILayout.PropertyField(hasMouse2HoldAction);
//        }

//        private void ShowFoodProperties()
//        {
//            if (healthRecoveryAmount != null) EditorGUILayout.PropertyField(healthRecoveryAmount);
//        }

//        private void ShowBuildingExtraProperties()
//        {
//            if (is2D != null) EditorGUILayout.PropertyField(is2D);
//            if (frontSprite != null) EditorGUILayout.PropertyField(frontSprite);
//            if (surroundSprite != null) EditorGUILayout.PropertyField(surroundSprite);
//            if (topSprite != null) EditorGUILayout.PropertyField(topSprite);
//            if (bottomSprite != null) EditorGUILayout.PropertyField(bottomSprite);
//            if (walkClips != null) EditorGUILayout.PropertyField(walkClips);
//            if (brokingClip != null) EditorGUILayout.PropertyField(brokingClip);
//            if (brokenClip != null) EditorGUILayout.PropertyField(brokenClip);
//            if (backFaceTexture != null) EditorGUILayout.PropertyField(backFaceTexture);
//            if (frontFaceTexture != null) EditorGUILayout.PropertyField(frontFaceTexture);
//            if (topFaceTexture != null) EditorGUILayout.PropertyField(topFaceTexture);
//            if (bottomFaceTexture != null) EditorGUILayout.PropertyField(bottomFaceTexture);
//            if (leftFaceTexture != null) EditorGUILayout.PropertyField(leftFaceTexture);
//            if (rightFaceTexture != null) EditorGUILayout.PropertyField(rightFaceTexture);
//            if (drawMode != null) EditorGUILayout.PropertyField(drawMode);
//            if (generateTwoFaceWithAir != null) EditorGUILayout.PropertyField(generateTwoFaceWithAir);
//            if (otherFaceCheck != null) EditorGUILayout.PropertyField(otherFaceCheck);
//        }
//    }
//}