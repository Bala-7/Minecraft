using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryObject
{
    [Serializable]
    public enum OBJECT_TYPE { NONE = 0, BLOCK, TOOL, OTHER }
    public OBJECT_TYPE type { get; set; }

    public bool stackable = true;
    public int StackSize = 64;
    public int currentStack = 1;

    public InventoryObject() { type = OBJECT_TYPE.NONE; }

    public InventoryObject(OBJECT_TYPE objectType) { type = objectType; }
}

public class InventoryObjectBlock : InventoryObject 
{
    private CELL_TYPE _cellType;
    public CELL_TYPE CellType { get { return _cellType; } }

    public InventoryObjectBlock(CELL_TYPE cellType) 
    {
        type = OBJECT_TYPE.BLOCK;
        _cellType = cellType;
    
    }
}
