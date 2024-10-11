using UnityEngine;


public enum DataType { Player, Enemy }

[System.Serializable]
public class DataBase : MonoBehaviour
{
    public GoooosTest[] a;

    public DataType dataType = DataType.Player;

    //通用数据
    public float speed = 1.5f;
    public int attack = 1;
    public int defense;
    public int health;

    //Player
    public float jumpForce = 1;

    //Enemy
    public float idleTime;
    public Transform[] patrolPoints;
    public Transform[] chasePoints;
}

namespace InspectEditor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(DataBase))]
    public class DataBaseEditor : Editor
    {
        #region 序列化属性

        private SerializedProperty dataType;
        private SerializedProperty attack;
        private SerializedProperty defense;
        private SerializedProperty health;

        //player
        private SerializedProperty speed;
        private SerializedProperty jumpForce;

        //enemy
        private SerializedProperty idleTime;
        private SerializedProperty patrolPoints;
        private SerializedProperty chasePoints;

        #endregion


        private void OnEnable()
        {
            //为所有需要动态显示的属性初始化SerializedProperty
            dataType = serializedObject.FindProperty("dataType");
            attack = serializedObject.FindProperty("attack");
            defense = serializedObject.FindProperty("defense");
            health = serializedObject.FindProperty("health");

            //显示Player面板
            speed = serializedObject.FindProperty("speed");
            jumpForce = serializedObject.FindProperty("jumpForce");

            //显示Enemy面板
            idleTime = serializedObject.FindProperty("idleTime");
            patrolPoints = serializedObject.FindProperty("patrolPoints");
            chasePoints = serializedObject.FindProperty("chasePoints");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //显示公有属性
            EditorGUILayout.PropertyField(speed);
            EditorGUILayout.PropertyField(attack);
            EditorGUILayout.PropertyField(defense);
            EditorGUILayout.PropertyField(health);

            // 显示DataType枚举选择器  
            EditorGUILayout.PropertyField(dataType);

            // 根据dataType的值来显示或隐藏属性  
            switch ((DataType)dataType.enumValueIndex)
            {
                case DataType.Player:
                    EditorGUILayout.PropertyField(speed);
                    EditorGUILayout.PropertyField(jumpForce);
                    break;
                case DataType.Enemy:
                    EditorGUILayout.PropertyField(idleTime);
                    EditorGUILayout.PropertyField(patrolPoints);
                    EditorGUILayout.PropertyField(chasePoints);
                    break;
            }

            // 保存更改  
            serializedObject.ApplyModifiedProperties();
        }
    }

}

