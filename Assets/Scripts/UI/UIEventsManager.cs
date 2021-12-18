using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIEventsManager : MonoBehaviour
{
    public static UIEventsManager s;

    #region Events
    UnityEvent currentObjectChangedEvent;
    UnityEvent currentObjectPlacedEvent;
    UnityEvent objectAddedEvent;
    UnityEvent loadStartEvent;
    UnityEvent loadEndEvent;
    UnityEvent inventoryKey;
    #endregion

    private void Awake()
    {
        if (s) Destroy(this.gameObject);
        else s = this;
    }

    private void Start()
    {
        currentObjectChangedEvent = new UnityEvent();
        currentObjectPlacedEvent = new UnityEvent();
        objectAddedEvent = new UnityEvent();
        loadStartEvent = new UnityEvent();
        loadEndEvent = new UnityEvent();
        inventoryKey = new UnityEvent();

        currentObjectChangedEvent.AddListener(OnCurrentObjectChanged);
        currentObjectPlacedEvent.AddListener(OnCurrentObjectPlaced);
        objectAddedEvent.AddListener(OnObjectAdded);
        loadStartEvent.AddListener(OnLoadStart);
        loadEndEvent.AddListener(OnLoadEnd);
        inventoryKey.AddListener(OnInventoryKey);
    }

    #region Event Methods
    private void OnCurrentObjectChanged()
    {
        UIManager.s.CurrentObjectChanged();
    }

    private void OnObjectAdded()
    {
        UIManager.s.UpdateAccessibleObjectsUI();
    }

    private void OnCurrentObjectPlaced() 
    {
        //UIManager.s.CurrentObjectPlaced();
        UIManager.s.UpdateAccessibleObjectsUI();
    }

    private void OnLoadStart()
    {
        UIManager.s.FadeOut();
    }

    private void OnLoadEnd()
    {
        UIManager.s.FadeIn();
    }

    private void OnInventoryKey() 
    {
        UIManager.s.InventoryKeyPressed();
    }

    #endregion

    public void Notify(UIEvents uiEvent) 
    {
        switch (uiEvent) 
        {
            case UIEvents.CURRENT_OBJECT_CHANGED: currentObjectChangedEvent.Invoke(); break;
            case UIEvents.CURRENT_OBJECT_PLACED: currentObjectPlacedEvent.Invoke(); break;
            case UIEvents.OBJECT_ADDED: objectAddedEvent.Invoke(); break;
            case UIEvents.LOAD_START: loadStartEvent.Invoke(); break;
            case UIEvents.LOAD_END: loadEndEvent.Invoke(); break;
            case UIEvents.INVENTORY_KEY_PRESSED: inventoryKey.Invoke(); break;
            default: return;
        }
    }

}
