using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float sprintSpeed = 8.0f;
    public float maxSlopeAngle = 45f; // degrees
    public float jumpForce = 8.0f;
    public float rotationSpeed = 4.0f;
    public float maxLookUpAngle = 80.0f;
    public float maxLookDownAngle = 70.0f;  // Adjusted for full downward rotation
    private Camera playerCamera;

    Rigidbody rb;
    private MeshCollider mesh;
    public static bool isGrounded = true;
    private float currentCameraRotationX = 0.0f;
    private float currentCameraRotationY = 0.0f;
    private Vector3 groundNormal = Vector3.up;
    public bool isSprinting = false;

    private void Start()
    {
        playerCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        mesh = GetComponent<MeshCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        UpdateGroundNormal();
        //PLAYER MOVEMENT
        float _vertical = Input.GetAxis("Vertical");
        float _horizontal = Input.GetAxis("Horizontal");

        Vector3 forwardDirection = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized;
        Vector3 rightDirection = Vector3.ProjectOnPlane(playerCamera.transform.right, Vector3.up).normalized;
        Vector3 movingDirection = (forwardDirection * _vertical + rightDirection * _horizontal).normalized;

        if (CanMoveOnCurrentSlope(groundNormal))
        {
            float speed = isSprinting ? sprintSpeed : moveSpeed;
            Vector3 movement = movingDirection * speed * Time.deltaTime;
            Vector3 newPosition = rb.position + new Vector3(movement.x, 0f, movement.z);
            rb.MovePosition(newPosition);
        }

        //MOUSE MOVEMENT
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        float rotationX = -(mouseY * rotationSpeed);
        float rotationY = mouseX * rotationSpeed;

        currentCameraRotationX += rotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -maxLookUpAngle, maxLookDownAngle);
        currentCameraRotationY += rotationY;
        playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, currentCameraRotationY, 0.0f);

        rb.transform.localEulerAngles = new Vector3(0.0f, currentCameraRotationY, 0.0f);

        //SPEED CONTROLLER
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded ? true : false;
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce * rb.mass, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    bool CanMoveOnCurrentSlope(Vector3 normal)
    {
        float slopeAngle = Vector3.Angle(normal, Vector3.up);
        return slopeAngle <= maxSlopeAngle;
    }

    void UpdateGroundNormal()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            groundNormal = hit.normal;
        }
        else
        {
            groundNormal = Vector3.up; // default to flat if no ground detected
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
