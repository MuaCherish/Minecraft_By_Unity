using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    private Vector3 Center = new Vector3(1f,40f,13f);
    private Vector3 LookAtTransform = new Vector3(8f,38f,6f);
    private float radius = 20f;
    private float rotationSpeed = 5f;

    private Vector3 offset;

    void Start()
    {
        // Calculate initial offset from center to camera
        offset = transform.position - Center;
    }

    void FixedUpdate()
    {
        // Calculate the desired position based on current angle and radius
        float angle = Time.time * rotationSpeed;
        Vector3 desiredPosition = Center + Quaternion.Euler(0, angle, 0) * new Vector3(0, 0, radius);

        // Update camera position
        transform.position = desiredPosition + offset;

        // Ensure the camera looks at the target while rotating
        transform.LookAt(LookAtTransform);
    }
}
