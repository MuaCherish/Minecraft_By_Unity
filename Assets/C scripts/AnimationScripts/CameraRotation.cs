using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public Transform LookAtObject; // The object the camera will look at
    public float RotationSpeed = 1.0f; // Speed of rotation
    public float Height = 10.0f; // Height of the camera above the ground
    public float Radius = 20.0f; // Radius of the circular path

    void FixedUpdate()
    {
        // Calculate the desired position based on current angle, radius, and height
        float angle = Time.time * RotationSpeed;
        Vector3 offset = new Vector3(Mathf.Sin(angle) * Radius, Height, Mathf.Cos(angle) * Radius);
        Vector3 desiredPosition = LookAtObject.position + offset;

        // Update camera position
        transform.position = desiredPosition;

        // Ensure the camera looks at the target while rotating
        transform.LookAt(LookAtObject);
    }
}
