using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedInventoryObject : MonoBehaviour
{
    private InventoryObject io;
    public InventoryObject InventoryObject { get { return io; } }

    private void Awake()
    {
        io = new InventoryObject(InventoryObject.OBJECT_TYPE.NONE);
    }

    public void SetInventoryObject(InventoryObject iObject)
    {
        io = iObject;
    }

    public void SetType(InventoryObject.OBJECT_TYPE objectType)
    {
        if(objectType == InventoryObject.OBJECT_TYPE.BLOCK) 
        { 
            
        }
        io.type = objectType;

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<FPS_Player>()) 
        {
            Inventory.s.AddObjectPickedUp(this);
        }
    }
}
