using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public Transform cameraTransform;
    
    public float walkSpeed = 15f;
    public float sprintSpeed = 30f;

    private Transform _tf;
    private bool _isInSprint;
    private float _speed;
    
    public float mouseSensitivity = 150f;

    private float _xRotation = 0f;

    void Start ()
    {
        _tf = transform;
        _speed = walkSpeed; //Start with walk speed
        _isInSprint = false; //Set sprint toggle to walk
        
        Cursor.lockState = CursorLockMode.Locked;
    }
	
    // Update is called once per frame
    void Update () {
        // Deal with player movement
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            _isInSprint = !_isInSprint;
            _speed = (_isInSprint) ? sprintSpeed : walkSpeed;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = _tf.right * x + _tf.forward * z;

        characterController.Move(_speed * Time.deltaTime * move);
        
        // Deal with camera movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        //Rotate camera for vertical camera movement to ensure player movement stays on xz-plane
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _tf.Rotate(Vector3.up * mouseX); 
    }
}
