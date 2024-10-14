//using System.Collections;
//using System.Collections.Generic;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif
//using UnityEngine;

//public class GoooosTest: MonoBehaviour
//{
//    public DataBase[] a;

//    [Header("必填参数")]
//    public string goodName;
//    public Sprite icon; //物品栏图标
//    public BlockClassfy classify = BlockClassfy.禁用;


//    [Header("classify = 建筑方块，功能性方块")]
//    public float DestroyTime;
//    public bool isSolid;        //是否会阻挡玩家
//    public bool isTransparent;  //周边方块是否面剔除
//    public bool canBeChoose;    //是否可被高亮方块捕捉到
//    public bool candropBlock;   //是否掉落方块
//    public bool isDIYCollision;//抽象来说就是方块向内挤压的数值
//    public CollosionRange CollosionRange; //对于Y来说，(0.5f,0,0f)，就是Y正方向的面向内挤压0.5f，Y负方向的面向内挤压0.0f，即台阶的碰撞参数

//    [Header("classify = 功能性方块")]
//    public bool isinteractable; //是否可被右键触发 
//    public bool IsOriented;     //是否跟随玩家朝向 

//    [Header("classify = 工具， 食物")]
//    public bool hasDiyRotation;//自定义旋转
//    public Vector3 DiyRotation;

//    [Header("classify = 工具")]
//    public bool canBreakBlockWithMouse1;  // 左键可破坏方块
//    public bool hasMouse2Action;          // 右键操作（比如放置方块）
//    public bool hasMouse2HoldAction;      // 长按右键的功能（比如连续放置方块）

//    [Header("classify = 食物")]
//    public int healthRecoveryAmount;      // 可恢复的血量值


//    [Header("classify = 建筑方块，功能性方块")]
//    public bool is2d;           //用来区分显示
//    public Sprite front_sprite; //掉落物
//    public Sprite surround_sprite; //掉落物
//    public Sprite top_sprit; //掉落物
//    public Sprite buttom_sprit; //掉落物


//    [Header("classify = 建筑方块，功能性方块")]
//    public AudioClip[] walk_clips = new AudioClip[2];
//    public AudioClip broking_clip;
//    public AudioClip broken_clip;


//    [Header("classify = 建筑方块，功能性方块")]
//    public int backFaceTexture = 0;
//    public int frontFaceTexture = 0;
//    public int topFaceTexture = 0;
//    public int bottomFaceTexture = 0;
//    public int leftFaceTexture = 0;
//    public int rightFaceTexture = 0;
//    public DrawMode DrawMode = DrawMode.Block;

//    [Header("classify = 建筑方块，功能性方块")]
//    public bool GenerateTwoFaceWithAir;    //如果朝向空气，则双面绘制
//    public List<FaceCheckMode> OtherFaceCheck;

//    //贴图中的面的坐标
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
//        #region 序列化属性
//        private SerializedProperty classify;
//        private SerializedProperty goodName;
//        private SerializedProperty icon;

//        // 建筑方块和功能性方块属性
//        private SerializedProperty destroyTime;
//        private SerializedProperty isSolid;
//        private SerializedProperty isTransparent;
//        private SerializedProperty canBeChoose;
//        private SerializedProperty candropBlock;
//        private SerializedProperty isDIYCollision;
//        private SerializedProperty collosionRange;

//        // 功能性方块属性
//        private SerializedProperty isInteractable;
//        private SerializedProperty isOriented;

//        // 工具和食物属性
//        private SerializedProperty hasDiyRotation;
//        private SerializedProperty diyRotation;
//        private SerializedProperty canBreakBlockWithMouse1;
//        private SerializedProperty hasMouse2Action;
//        private SerializedProperty hasMouse2HoldAction;
//        private SerializedProperty healthRecoveryAmount;

//        // 建筑方块属性
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
//            // 初始化所有需要动态显示的属性
//            classify = serializedObject.FindProperty("classify");
//            goodName = serializedObject.FindProperty("goodName");
//            icon = serializedObject.FindProperty("icon");

//            // 建筑方块和功能性方块的属性
//            destroyTime = serializedObject.FindProperty("DestroyTime");
//            isSolid = serializedObject.FindProperty("isSolid");
//            isTransparent = serializedObject.FindProperty("isTransparent");
//            canBeChoose = serializedObject.FindProperty("canBeChoose");
//            candropBlock = serializedObject.FindProperty("candropBlock");
//            isDIYCollision = serializedObject.FindProperty("isDIYCollision");
//            collosionRange = serializedObject.FindProperty("collosionRange");

//            // 功能性方块的属性
//            isInteractable = serializedObject.FindProperty("isInteractable");
//            isOriented = serializedObject.FindProperty("isOriented");

//            // 工具和食物的属性
//            hasDiyRotation = serializedObject.FindProperty("hasDiyRotation");
//            diyRotation = serializedObject.FindProperty("diyRotation");
//            canBreakBlockWithMouse1 = serializedObject.FindProperty("canBreakBlockWithMouse1");
//            hasMouse2Action = serializedObject.FindProperty("hasMouse2Action");
//            hasMouse2HoldAction = serializedObject.FindProperty("hasMouse2HoldAction");
//            healthRecoveryAmount = serializedObject.FindProperty("healthRecoveryAmount");

//            // 建筑方块的属性
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

//            // 输出调试信息，检查序列化属性是否正确
//            //Debug.Log($"Good Name: {goodName != null}");
//            //Debug.Log($"Icon: {icon != null}");
//            //Debug.Log($"Classify: {classify != null}");

//            // 检查所有其他属性
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

//            // 显示公有属性
//            if (goodName != null) EditorGUILayout.PropertyField(goodName);
//            if (icon != null) EditorGUILayout.PropertyField(icon);
//            if (classify != null) EditorGUILayout.PropertyField(classify);

//            // 根据 classify 的值来显示或隐藏属性  
//            if (classify != null)
//            {
//                switch ((BlockClassfy)classify.enumValueIndex)
//                {
//                    case BlockClassfy.建筑方块:
//                        ShowBuildingBlockProperties();
//                        break;

//                    case BlockClassfy.功能性方块:
//                        ShowFunctionalBlockProperties();
//                        break;

//                    case BlockClassfy.工具:
//                        ShowToolProperties();
//                        break;

//                    case BlockClassfy.食物:
//                        ShowFoodProperties();
//                        break;
//                }

//                // 显示建筑方块的额外属性
//                if (classify.enumValueIndex == (int)BlockClassfy.建筑方块 || classify.enumValueIndex == (int)BlockClassfy.功能性方块)
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
//            ShowBuildingBlockProperties(); // 显示建筑方块的属性
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