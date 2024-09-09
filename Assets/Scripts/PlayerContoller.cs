using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
 
public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private PhotonView _photonView;
    [SerializeField] private Transform _camera;
    [SerializeField] private float _cameraSensitivity = 2f;
    [SerializeField] private float _movementSpeed = 4f;
    [SerializeField] private float _checkJumpRadius = 0.2f;
    [SerializeField] private float _jumpForce = 3f;
    
    
    private float _rotationX;
    
 
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _photonView = GetComponent<PhotonView>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
 
        if (!_photonView.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(_rigidbody);
        }
    }
 
    private void FixedUpdate()
    {
        if(_photonView.IsMine)
            PlayerMovement();
    }
    
    private void Update()
    {
        if(!_photonView.IsMine)
            return;
        
        RotatePlayerRightLeft();
        RotateCameraUpDown();
        if(Input.GetButtonDown("Jump"))
            TryJump();
    }
    
    private void TryJump()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position - Vector3.down * 0.5f, _checkJumpRadius);
        
        foreach(Collider collider in colliders)
        {
            if (collider.gameObject != gameObject)
                return;
        }
        
        _rigidbody.AddForce(Vector3.up * _jumpForce,ForceMode.Impulse);
    }
    
 
    private void PlayerMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
 
        Vector3 movementDir = transform.forward * h + transform.right * -v;
        movementDir = Vector3.ClampMagnitude(movementDir, 1f);
        
        _rigidbody.velocity = new Vector3(movementDir.x * _movementSpeed, _rigidbody.velocity.y,
            movementDir.z * _movementSpeed);
    }
 
    private void RotatePlayerRightLeft()
    {
        transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * _cameraSensitivity);
    }
 
    private void RotateCameraUpDown()
    {
        _rotationX -= _cameraSensitivity * Input.GetAxisRaw("Mouse Y");
        _rotationX = Mathf.Clamp(_rotationX, -75, 75);
        _camera.eulerAngles = new Vector3(_rotationX, _camera.eulerAngles.y, _camera.eulerAngles.z);
    }
}