using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Player : MonoBehaviour
{
    public static FPS_Player s;
    private Rigidbody _rb;

    #region Camera
    public Camera _cam;
    private Vector3 camFwd;
    #endregion


    #region Movement
    [Range(1.0f, 10.0f)]
    public float walk_speed;
    [Range(1.0f, 10.0f)]
    public float backwards_walk_speed;
    [Range(1.0f, 10.0f)]
    public float strafe_speed;

    [Range(2.0f, 10.0f)]
    public float jump_force;
    #endregion

    private GameObject _rightHand;
    private Animator _rhac;
    #region Attributes
    [Range(0.1f, 10.0f)]
    public float interactionDistance;

    #endregion

    #region Cell destruction
    private Cell _currentCell;
    #endregion

    void Awake()
    {
        s = this;

        Cursor.lockState = CursorLockMode.Locked;

        _rb = GetComponent<Rigidbody>();
        _rightHand = _cam.transform.Find("RightHand").gameObject;
        _rhac = _rightHand.GetComponent<Animator>();
    }

    private void Update()
    {
        bool jump = Input.GetButtonDown("Jump");

        // Jump 
        if (jump)
        {
            _rb.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
        }

        HandleLeftClick();
        HandleRightClick();


    }

    private void HandleRightClick() {
        if (Input.GetMouseButtonDown(1))
        {
            int layerMask = 1 << 8;
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, interactionDistance, layerMask))
            {
                Debug.DrawLine(_cam.transform.position, _cam.transform.position + _cam.transform.forward * interactionDistance, Color.blue);
                _currentCell = hit.transform.GetComponent<Cell>();
                _currentCell.PlaceOnSide(hit.point);

                _rhac.Play("PlaceCell_RightHand");
            }
        }
    }

    private void HandleLeftClick() {
        int layerMask = 1 << 8;
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, interactionDistance, layerMask))
            {
                Debug.DrawLine(_cam.transform.position, _cam.transform.position + _cam.transform.forward * interactionDistance, Color.blue);
                _currentCell = hit.transform.GetComponent<Cell>();
                _currentCell.StartBreaking();
                if (Input.GetMouseButtonDown(0))
                {
                    _rhac.Play("Pick_RightHand");
                    _rhac.SetBool("picking", true);
                }
            }
            else
            {
                Debug.DrawLine(_cam.transform.position, _cam.transform.position + _cam.transform.forward * interactionDistance, Color.red);
                _rhac.SetBool("picking", false);
                try { _currentCell.StopBreaking(); } catch (System.NullReferenceException e) { }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _rhac.SetBool("picking", false);
            try { _currentCell.StopBreaking(); } catch (System.NullReferenceException e) { }
        }
    }

    void FixedUpdate()
    {
        // Gets the input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Calculate camera relative directions to move:
        camFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 1, 1)).normalized;
        Vector3 camFlatFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 flatRight = new Vector3(_cam.transform.right.x, 0, _cam.transform.right.z);

        Vector3 m_CharForward = Vector3.Scale(camFlatFwd, new Vector3(1, 0, 1)).normalized;
        Vector3 m_CharRight = Vector3.Scale(flatRight, new Vector3(1, 0, 1)).normalized;


        // Draws a ray to show the direction the player is aiming at
        //Debug.DrawLine(transform.position, transform.position + camFwd * 5f, Color.red);

        float w_speed = (v > 0) ? walk_speed : backwards_walk_speed;
        Vector3 move = v * m_CharForward * w_speed + h * m_CharRight * strafe_speed;
        transform.position += move * Time.deltaTime;    // Move the actual player

        bool moving = (move != Vector3.zero);
        _rhac.SetBool("moving", moving);

    }

    public void MoveToPosition(Vector3 newPosition) {
        transform.position = newPosition;
    }

    public Vector3 GetWorldPosition() { return transform.position; }
}