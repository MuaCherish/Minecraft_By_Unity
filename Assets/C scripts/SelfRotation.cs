using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    public float rotationSpeed = 30f; // ��ת�ٶ�

    void Update()
    {
        

        // ��X����ת
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
    }
}
