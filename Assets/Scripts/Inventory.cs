using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    public static Inventory s;

    const int AccesibleInventorySize = 9;
    const int InsideInventorySize = 27;
    [SerializeField]
    private InventoryObject[] accessibleObjects;
    public InventoryObject[] AccessibleObjects { get { return accessibleObjects; } }
    [SerializeField]
    private InventoryObject[] insideInventoryObjects;

    private int currentAccessibleObjectIndex = 0;
    public int CurrentObjectIndex { get { return currentAccessibleObjectIndex; } }
    public int CurrentObjectStack { get { return accessibleObjects[currentAccessibleObjectIndex].currentStack; } }

    private void Awake()
    {
        if (s)
            Destroy(this.gameObject);
        else s = this;

        InitializeInventory();
    }

    private void InitializeInventory()
    {
        accessibleObjects = new InventoryObject[AccesibleInventorySize];
        insideInventoryObjects = new InventoryObject[InsideInventorySize];

        for (int i = 0; i < 27; ++i)
        {
            if (i < AccesibleInventorySize)
                accessibleObjects[i] = new InventoryObject();
            insideInventoryObjects[i] = new InventoryObject();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddObjectPickedUp(SpawnedInventoryObject obj) 
    {
        InventoryObject io = obj.InventoryObject;
        InventoryObjectBlock block = io as InventoryObjectBlock;
        if (!ReferenceEquals(block, null))
        {
            AddBlockPickedUp(block);
            Destroy(obj.gameObject);
        }
    }


    private void AddBlockPickedUp(InventoryObjectBlock block) 
    {
        for (int i = 0; i < AccesibleInventorySize; ++i)
        {
            if (accessibleObjects[i].type == InventoryObject.OBJECT_TYPE.NONE)
            {
                accessibleObjects[i] = block;
                UIEventsManager.s.Notify(UIEvents.OBJECT_ADDED);
                break;
            }
            else 
            {
                InventoryObjectBlock blockInInventory = accessibleObjects[i] as InventoryObjectBlock;
                if (!ReferenceEquals(blockInInventory, null))
                {
                    if (blockInInventory.CellType == block.CellType)
                    {
                        blockInInventory.currentStack = blockInInventory.currentStack + block.currentStack;
                        UIEventsManager.s.Notify(UIEvents.OBJECT_ADDED);
                        break;
                    }
                }
            }
        }
    }

    public void ChangeCurrentAccessibleObject(float delta)
    {
        currentAccessibleObjectIndex = mod(currentAccessibleObjectIndex + ((delta > 0) ? 1 : -1), accessibleObjects.Length);
        UIEventsManager.s.Notify(UIEvents.CURRENT_OBJECT_CHANGED);
    }
    private int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    #region Place blocks
    public CELL_TYPE GetCurrentObjectCellType() 
    {
        InventoryObjectBlock block = accessibleObjects[currentAccessibleObjectIndex] as InventoryObjectBlock;
        return block.CellType;
    }

    public bool IsCurrentABlock() {
        InventoryObjectBlock block = accessibleObjects[currentAccessibleObjectIndex] as InventoryObjectBlock;
        if (!ReferenceEquals(block, null))
            return true;
        else return false;
    }

    public void BlockPlaced()
    {
        InventoryObjectBlock block = accessibleObjects[currentAccessibleObjectIndex] as InventoryObjectBlock;
        if (!ReferenceEquals(block, null)) {
            block.currentStack--;
            if (block.currentStack <= 0) { 
                accessibleObjects[currentAccessibleObjectIndex].type = InventoryObject.OBJECT_TYPE.NONE;
            }
            UIEventsManager.s.Notify(UIEvents.CURRENT_OBJECT_PLACED);
        }
        
    }

    #endregion
}


