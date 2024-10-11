using UnityEngine;


public enum DataType { Player, Enemy }

[System.Serializable]
public class DataBase : MonoBehaviour
{
    public GoooosTest[] a;

    public DataType dataType = DataType.Player;

    //ͨ������
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
        #region ���л�����

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
            //Ϊ������Ҫ��̬��ʾ�����Գ�ʼ��SerializedProperty
            dataType = serializedObject.FindProperty("dataType");
            attack = serializedObject.FindProperty("attack");
            defense = serializedObject.FindProperty("defense");
            health = serializedObject.FindProperty("health");

            //��ʾPlayer���
            speed = serializedObject.FindProperty("speed");
            jumpForce = serializedObject.FindProperty("jumpForce");

            //��ʾEnemy���
            idleTime = serializedObject.FindProperty("idleTime");
            patrolPoints = serializedObject.FindProperty("patrolPoints");
            chasePoints = serializedObject.FindProperty("chasePoints");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //��ʾ��������
            EditorGUILayout.PropertyField(speed);
            EditorGUILayout.PropertyField(attack);
            EditorGUILayout.PropertyField(defense);
            EditorGUILayout.PropertyField(health);

            // ��ʾDataTypeö��ѡ����  
            EditorGUILayout.PropertyField(dataType);

            // ����dataType��ֵ����ʾ����������  
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

            // �������  
            serializedObject.ApplyModifiedProperties();
        }
    }

}

