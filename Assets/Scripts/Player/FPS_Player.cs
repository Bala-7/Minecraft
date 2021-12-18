using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        PlayerInput();
    }

    #region Input
    private void PlayerInput()
    {
        if (Input.GetMouseButtonDown(1))
            OnRightClick();
        if (Input.GetMouseButtonDown(0))
            OnLeftClick();
        float mouseScroll = Input.mouseScrollDelta.y;
        if (mouseScroll != 0)
            OnMouseScroll(mouseScroll);
        if (Input.GetButtonDown("Jump"))
            OnJump();
        if (Input.GetKeyDown(KeyCode.E))
            OnInventoryKeyPressed();
    }

    private void OnLeftClick() {
        int layerMask = 1 << 9; // Terrain layer

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, interactionDistance, layerMask))
        {
            Debug.DrawLine(_cam.transform.position, _cam.transform.position + _cam.transform.forward * interactionDistance, Color.blue);
            Vector3 cellPosition = hit.point - hit.normal * 0.1f;
            if (World.IsThereCellInPosition(cellPosition)) {
                World.DestroyCellInPosition(cellPosition);

                GameObject t = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Prefabs/InventoryBlock.prefab", typeof(GameObject));
                GameObject spawnedGO = Instantiate(t, hit.point, Quaternion.identity);
                InventoryObject io = new InventoryObjectBlock(World.GetCELL_TYPEInPosition(cellPosition));
                spawnedGO.GetComponent<SpawnedInventoryObject>().SetInventoryObject(io);

                _rhac.Play("Pick_RightHand");
                //_rhac.SetBool("picking", true);
            }
            /*_currentCell = hit.transform.GetComponent<Cell>();
            _currentCell.StartBreaking();
            if (Input.GetMouseButtonDown(0))
            {
                    
            }*/
        }
        else
        {
            Debug.DrawLine(_cam.transform.position, _cam.transform.position + _cam.transform.forward * interactionDistance, Color.red);
            /*_rhac.SetBool("picking", false);
            try { _currentCell.StopBreaking(); } catch (System.NullReferenceException e) { }*/
        }
        

    }

    private void OnRightClick() {
        int layerMask = 1 << 9;
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, interactionDistance, layerMask))
        {
            Debug.DrawLine(_cam.transform.position, _cam.transform.position + _cam.transform.forward * interactionDistance, Color.blue);

            Vector3 cellPosition = hit.point + 0.5f * hit.normal.normalized;
            if (Inventory.s.IsCurrentABlock())
            {
                World.PutCellInPosition(cellPosition);

                /*_currentCell = hit.transform.GetComponent<Cell>();
                _currentCell.PlaceOnSide(hit.point);*/

                _rhac.Play("PlaceCell_RightHand");
            }
            
        }
    }

    private void OnMouseScroll(float delta) 
    {
        Inventory.s.ChangeCurrentAccessibleObject(delta);
    }

    private void OnJump() 
    {
        _rb.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
    }

    private void OnInventoryKeyPressed() 
    {
        UIEventsManager.s.Notify(UIEvents.INVENTORY_KEY_PRESSED);
    }

    #endregion

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