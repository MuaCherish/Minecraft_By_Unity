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
//    //public ClassStruct[] test; // ����һ������

//    public GoooosTest[] test2;
//}

//[System.Serializable]
//public class ClassStruct
//{
//    public Classclassfy classclassfy; // �����ֶ�
//    public StructA structA;            // �ṹ��A
//    public StructB structB;            // �ṹ��B
//    public StructC structC;            // �ṹ��C
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
//        // �������л�����
//        serializedObject.Update();

//        // ��ȡ ClassStruct ���������
//        SerializedProperty classStructArray = serializedObject.FindProperty("test");

//        // ����ÿ�� ClassStruct ʵ��
//        for (int i = 0; i < classStructArray.arraySize; i++)
//        {
//            SerializedProperty classStruct = classStructArray.GetArrayElementAtIndex(i);

//            // ��ʾ classclassfy ����
//            SerializedProperty classclassfyProperty = classStruct.FindPropertyRelative("classclassfy");
//            EditorGUILayout.PropertyField(classclassfyProperty);

//            // ���� classclassfy ��ֵѡ������ʾ�ṹ��
//            switch ((Classclassfy)classclassfyProperty.enumValueIndex)
//            {
//                case Classclassfy.A:
//                    // ֻ��ʾ structA
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structA"), true);
//                    // ���� structB �� structC
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structB"), false);
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structC"), false);
//                    break;

//                case Classclassfy.B:
//                    // ֻ��ʾ structB
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structB"), true);
//                    // ���� structA �� structC
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structA"), false);
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structC"), false);
//                    break;

//                case Classclassfy.C:
//                    // ֻ��ʾ structC
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structC"), true);
//                    // ���� structA �� structB
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structA"), false);
//                    EditorGUILayout.PropertyField(classStruct.FindPropertyRelative("structB"), false);
//                    break;
//            }
//        }

//        // ��Ӱ�ť���������Ԫ��
//        if (GUILayout.Button("Add ClassStruct"))
//        {
//            classStructArray.arraySize++;
//        }

//        // Ӧ�ø���
//        serializedObject.ApplyModifiedProperties();
//    }
//}
