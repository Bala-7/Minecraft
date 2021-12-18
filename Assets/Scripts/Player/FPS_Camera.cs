using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Camera : MonoBehaviour
{
    public Camera _cam;

    [Range(0.1f, 2.0f)]
    public float sensitivity;
    public bool invertXAxis;
    public bool invertYAxis;

    void FixedUpdate()
    {
        // Read input
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        // Settings
        h = (invertXAxis) ? (-h) : h;
        v = (invertYAxis) ? (v) : -v;

        // Orbit the camera around the character
        if (h != 0)
        {   // Horizontal movement 
            transform.Rotate(Vector3.up, h * 90 * sensitivity * Time.deltaTime);

        }
        if (v != 0)
        {   // Vertical movement
            //_cam.transform.RotateAround(transform.position, transform.right, v * 90 * sensitivity * Time.deltaTime);
            _cam.transform.Rotate(Vector3.right, v * 90 * sensitivity * Time.deltaTime);
        }


        // Fix Z-rotation issues
        Vector3 ea = _cam.transform.rotation.eulerAngles;
        _cam.transform.rotation = Quaternion.Euler(new Vector3(ea.x, ea.y, 0));
    }

    public Vector3 GetForwardDirection()
    {
        return _cam.transform.forward;
    }

}