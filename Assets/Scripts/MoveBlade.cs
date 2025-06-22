using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBlade : MonoBehaviour
{
    public float speed = 5f; // Speed of movement
    private Vector3 worldZDirection = Vector3.forward; // Always (0,0,1) in world space

    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) // Left mouse button
        {
            transform.position += worldZDirection * speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Backspace)) // Right mouse button
        {
            transform.position -= worldZDirection * speed * Time.deltaTime;
        }
    }
}
