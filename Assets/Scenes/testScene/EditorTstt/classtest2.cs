//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//public enum Classclassfy
//{
//    A, B, C
//}

//public class classtest2 : MonoBehaviour
//{
//    //public ClassStruct[] test; // 这是一个数组

//    public GoooosTest[] test2;
//}

//[System.Serializable]
//public class ClassStruct
//{
//    public Classclassfy classclassfy; // 分类字段
//    public StructA structA;            // 结构体A
//    public StructB structB;            // 结构体B
//    public StructC structC;            // 结构体C
//}

//[System.Serializable]
//public class StructA
//{
//    public int a;
//}

//[System.Serializable]
//public class StructB
//{
//    public int a;
//}

//[System.Serializable]
//public class StructC
//{
//    public int a;
//}

//[CustomEditor(typeof(classtest2))]
//public class classtest2Editor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        // 更新序列化对象
//        serializedObject.Update();

//        // 获取 ClassStruct 数组的属性
//        SerializedProperty classStructArray = serializedObject.FindProperty("test");

//        // 遍历每个 ClassStruct 实例
//        for (int i = 0; i < classStructArray.arraySize; i++)
//        {
//            SerializedProperty classStruct = classStructArray.GetArrayElementAtIndex(i);

//            // 显示 classclassfy 属性
//            SerializedProperty classclassfyProperty = classStruct.FindPropertyRelative("classclassfy");
//            EditorGUILayout.PropertyField(classclassfyProperty);

//            // 根据 classclassfy 的值选择性显示结构体
//            switch ((Classclassfy)classclassfyProperty.enumValueIndex)
//            {
//                case Classclassfy.A:
//                    // 只显示 structA
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structA"), true);
//                    // 隐藏 structB 和 structC
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structB"), false);
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structC"), false);
//                    break;

//                case Classclassfy.B:
//                    // 只显示 structB
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structB"), true);
//                    // 隐藏 structA 和 structC
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structA"), false);
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structC"), false);
//                    break;

//                case Classclassfy.C:
//                    // 只显示 structC
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structC"), true);
//                    // 隐藏 structA 和 structB
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structA"), false);
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structB"), false);
//                    break;
//            }
//        }

//        // 添加按钮用于添加新元素
//        if (GUILayout.Button("Add ClassStruct"))
//        {
//            classStructArray.arraySize++;
//        }

//        // 应用更改
//        serializedObject.ApplyModifiedProperties();
//    }
//}
