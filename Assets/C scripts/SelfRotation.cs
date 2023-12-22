using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    public float rotationSpeed = 30f; // 旋转速度

    void Update()
    {
        

        // 绕X轴旋转
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
    }
}
