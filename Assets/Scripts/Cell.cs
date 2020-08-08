using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class Cell : MonoBehaviour
{
    #region MeshRenderers
    private MeshRenderer _front;
    private MeshRenderer _back;
    private MeshRenderer _left;
    private MeshRenderer _right;
    private MeshRenderer _top;
    private MeshRenderer _bottom;

    private List<MeshRenderer> _meshes;
    private List<Vector3> _meshNormals;
    #endregion

    #region Break sprites
    public List<Sprite> breakFrames;

    private List<SpriteRenderer> _breakInstances;
    #endregion

    public float baseTimeToDestroy = 5.0f;  // Time to destroy this block with the hand
    public int type;

    private float _breakingTime = 0;
    private int currentBreakFrame = 0;
    private bool _breaking;

    void Awake() {
        #region Init mesh renderers
        _front = transform.GetChild(0).GetComponent<MeshRenderer>();
        _back = transform.GetChild(1).GetComponent<MeshRenderer>();
        _left = transform.GetChild(2).GetComponent<MeshRenderer>();
        _right = transform.GetChild(3).GetComponent<MeshRenderer>();
        _top = transform.GetChild(4).GetComponent<MeshRenderer>();
        _bottom = transform.GetChild(5).GetComponent<MeshRenderer>();

        _meshes = new List<MeshRenderer>();
        _meshes.Add(_front);
        _meshes.Add(_back);
        _meshes.Add(_left);
        _meshes.Add(_right);
        _meshes.Add(_top);
        _meshes.Add(_bottom);

        _meshNormals = new List<Vector3>();
        _meshNormals.Add(-Vector3.forward);
        _meshNormals.Add(Vector3.forward);
        _meshNormals.Add(-Vector3.right);
        _meshNormals.Add(Vector3.right);
        _meshNormals.Add(Vector3.up);
        _meshNormals.Add(-Vector3.up);
        #endregion

        _breakInstances = new List<SpriteRenderer>();
        for (int i = 0; i < 6; ++i)
        {
            GameObject go = new GameObject("Break" + i);
            go.transform.parent = transform;

            go.transform.position = _meshes[i].transform.position + _meshNormals[i] * 0.0001f;
            go.transform.rotation = _meshes[i].transform.rotation;

            SpriteRenderer sr = go.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            _breakInstances.Add(sr);

            sr.sprite = breakFrames[0];
            sr.enabled = false;

            GetComponent<BoxCollider>().enabled = false;

            _meshes[i].enabled = false;

            /*
            RaycastHit hit;
            int layerMask = 1 << 8;
            // If a cell recently created is surrounded by other cells, those must be redrawn
            if (Physics.Raycast(transform.position, transform.TransformDirection(_meshNormals[i]), out hit, 0.75f, layerMask))
            {
                hit.transform.GetComponent<Cell>().Place();
            }*/
        }

        //Place();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //DrawDebugLines();

        if (_breaking)
        {
            _breakingTime += Time.deltaTime;

            if (((_breakingTime / baseTimeToDestroy) > (((float)currentBreakFrame + 1.0f)) / breakFrames.Count) && 
                currentBreakFrame < breakFrames.Count - 1) {
                currentBreakFrame++;

                for (int i = 0; i < 6; ++i)
                {
                    _breakInstances[i].sprite = breakFrames[currentBreakFrame];
                }
            }



            if (_breakingTime >= baseTimeToDestroy)
            {
                Break();
            }
        }

    }


    // This method will decide which sides of the cell will be drawn
    public void Place() {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;




        for (int i = 0; i < 6; ++i) {

            Vector3 targetPos = transform.position + _meshNormals[i];
            bool cellInTarget = WorldGenerator.s.IsThereCellInPosition(targetPos);

            if (cellInTarget)
            {
                _meshes[i].enabled = false;

            }
            else { 
                _meshes[i].enabled = true;
                GetComponent<BoxCollider>().enabled = true;
            }
        }

    }


    public void Break() {
        int layerMask = 1 << 8;

        GetComponent<BoxCollider>().enabled = false;
        WorldGenerator.s.DestroyCellInPosition(transform.position);

        for (int i = -1; i <= 1; ++i) {
            for (int j = -1; j <= 1; ++j)
            {
                for (int k = -1; k <= 1; ++k)
                {
                    Vector3 targetPos = transform.position + new Vector3(i, j, k);
                    bool cellInTarget = WorldGenerator.s.IsThereCellInPosition(targetPos);
                    if (cellInTarget)
                    {
                        Cell c = WorldGenerator.s.GetCellInPosition(targetPos);
                        try { c.Place(); } catch (System.Exception e) { }
                    }
                }
            }
        }
        /*
        for (int i = 0; i < 6; ++i)
        {

            Vector3 targetPos = transform.position + _meshNormals[i];
            bool cellInTarget = WorldGenerator.s.IsThereCellInPosition(targetPos);
            if (cellInTarget)
            {
                Cell c = WorldGenerator.s.GetCellInPosition(targetPos);
                try { c.Place(); } catch (System.Exception e) { }
            }
        }*/

        WorldGenerator.s.EnqueueCell(this);
        //Destroy(this.gameObject);
    }

    private void DrawDebugLines() {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;


        for (int i = 0; i < 6; ++i)
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(_meshNormals[i]), out hit, 0.75f, layerMask))
            {
                if (i == 2) Debug.DrawLine(transform.position, transform.position + _meshNormals[i] * 0.75f, Color.blue);
                
            }
            else
            {
                if (i == 2) Debug.DrawLine(transform.position, transform.position + _meshNormals[i] * 0.75f, Color.red);
            }
        }
    }


    public void StartBreaking() {
        if (!_breaking && baseTimeToDestroy > 0)
        {
            _breaking = true;

            for (int i = 0; i < 6; ++i)
            {
                _breakInstances[i].enabled = true;
            }
        }
    }

    public void StopBreaking() {
        _breaking = false;

        _breakingTime = 0;
        currentBreakFrame = 0;
        for (int i = 0; i < 6; ++i)
        {
            _breakInstances[i].enabled = false;
        }
    }

    public void PlaceOnSide(Vector3 hitPosition) {
        float x = hitPosition.x;
        float y = hitPosition.y;
        float z = hitPosition.z;
        Vector3 targetPos = Vector3.zero;
        
        #region Target position of the new cube
        if (x == transform.position.x + 0.5f) {
            targetPos = transform.position + new Vector3(1,0,0);    
        }
        else if (x == transform.position.x - 0.5f)
        {
            targetPos = transform.position - new Vector3(1, 0, 0);
        }
        else if (y == transform.position.y + 0.5f)
        {
            targetPos = transform.position + new Vector3(0, 1, 0);
        }
        else if (y == transform.position.y - 0.5f)
        {
            targetPos = transform.position - new Vector3(0, 1, 0);
        }
        else if (z == transform.position.z + 0.5f)
        {
            targetPos = transform.position + new Vector3(0, 0, 1);
        }
        else if (z == transform.position.z - 0.5f)
        {
            targetPos = transform.position - new Vector3(0, 0, 1);
        }
        #endregion

        GameObject newCellGO = Instantiate(gameObject, targetPos, Quaternion.identity);
        Cell newCell = newCellGO.GetComponent<Cell>();

        newCell.Place();
        Place();
    }

    public void Hide() {
        for (int i = 0; i < _meshes.Count; ++i) {
            _meshes[i].enabled = false;
        }
        GetComponent<BoxCollider>().enabled = false;
    }
    /*
    private void OnDrawGizmos()
    {
        for (int i = 0; i < 6; ++i)
        {

            Vector3 targetPos = transform.position + _meshNormals[i];
            if (transform.position == new Vector3(1, 1, 1))
                UnityEngine.Debug.Log("Error display");
            bool cellInTarget = WorldGenerator.s.IsThereCellInPosition(targetPos);

            if (cellInTarget)
            {
                //_meshes[i].enabled = false;

            }
            else
            {
                //_meshes[i].enabled = true;
                GetComponent<BoxCollider>().enabled = true;
                Gizmos.color = Color.green;
                Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
            }

        }
    }
*/
}
